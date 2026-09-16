using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Net;

/// <summary>
///   A client to download content from the web.
/// </summary>
public interface ITrickyHttpClient
{
    /// <summary>
    ///   Downloads the content from the specified URL.
    /// </summary>
    /// <param name="url">The url to download content from.</param>
    /// <param name="cancel">The cancellation token to be used for safe download cancellation.</param>
    /// <returns>The downloaded content.</returns>
    Task<string?> DownloadContentAsync(string url, CancellationToken cancel);
}

internal sealed class TrickyHttpClient : ITrickyHttpClient, IDisposable
{
    private const int DELAY_MS = 500;
    private const int MAX_REPEATS = 5;

    /// <summary>
    ///   The query parameters a gate names the URL it sends the visitor back to, when it answers
    ///   from a host of its own.
    /// </summary>
    private static readonly string[] ReturnUrlParameters =
        ["callbackUrl", "returnUrl", "redirectUrl", "redirect_uri", "returnTo"];

    /// <summary>
    ///   Above this size a response is a page rather than an interstitial, and is not worth
    ///   searching for a way onwards.
    /// </summary>
    private const int MAX_INTERSTITIAL_LENGTH = 64 * 1024;

    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    ///   How a gate that answers from the site's own host states where to go on: a meta refresh, an
    ///   assignment to <c>location</c>, or a URL handed to <c>decodeURIComponent</c> for one of
    ///   those to use. Only a same-site target is ever followed, so a false positive can at worst
    ///   fetch another page of the site that was asked for.
    /// </summary>
    private static readonly Regex[] ContinuationUrlPatterns =
    [
        new(@"<meta[^>]+http-equiv=[""']?refresh[""']?[^>]+content=[""'][^""']*?url=(?<url>[^""'\s]+)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout),
        new(@"location(?:\.href|\.replace|\.assign)?\s*(?:=|\()\s*[""'](?<url>[^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout),
        new(@"decodeURIComponent\(\s*[""'](?<url>[^""']+)[""']",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout),
    ];

    private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockers = new();

    /// <summary>
    ///   Settled once and then kept, rather than made up per request: the cookies this client now
    ///   carries between requests would otherwise arrive alongside a user agent that keeps changing,
    ///   which is exactly what a bot check looks for.
    /// </summary>
    /// <remarks>
    ///   The agent it settles on is deliberately an implausible one. Claiming to be a current
    ///   browser invites a check of whether the connection looks like that browser as well, which
    ///   this client cannot pass; amazon.de and amazon.nl, for two, answer a request claiming to be
    ///   a current Chrome with a page about automated access, and one claiming to be Chrome 7 on
    ///   Windows 98 with the product.
    /// </remarks>
    private readonly string _userAgent = CreateRandomUserAgent();
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly ILogger<TrickyHttpClient> _logger;

    public TrickyHttpClient(ILogger<TrickyHttpClient> logger)
        : this(logger, CreateDefaultHttpClient(), ownsHttpClient: true)
    {
    }

    /// <summary>
    ///   Takes the <see cref="HttpClient"/> to download through, so that a test can drive the client
    ///   without reaching the network. The cookie handling this class relies on is the caller's to
    ///   set up in that case.
    /// </summary>
    internal TrickyHttpClient(ILogger<TrickyHttpClient> logger, HttpClient httpClient, bool ownsHttpClient = false)
    {
        _logger = logger;
        _httpClient = httpClient;
        _ownsHttpClient = ownsHttpClient;
    }

    public async Task<string?> DownloadContentAsync(string url, CancellationToken cancel)
    {
        Guard.NotNull(url);

        var uri = new Uri(url);

        var locker = _lockers.GetOrAdd(uri.Host, (_) => new SemaphoreSlim(1));
        await locker.WaitAsync(cancel);
        try
        {
            await Task.Delay(DELAY_MS, cancel);
            return await DownloadInternalAsync(uri, cancel);
        }
        finally
        {
            locker.Release();
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }

        foreach (var locker in _lockers.Values)
        {
            locker.Dispose();
        }

        _lockers.Clear();
    }

    private async Task<string?> DownloadInternalAsync(Uri uri, CancellationToken cancel)
    {
        for (var irepeat = 1; irepeat <= MAX_REPEATS; irepeat++)
        {
            using var response = await GetAsync(uri, cancel);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(DELAY_MS * (irepeat + 1), cancel);
                    continue;
                }

                // Something went wrong
                _logger.LogError("Failed to fetch '{Url}'. Error Code = {ResponseStatusCode}", uri, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancel);

            if (await TryPassConsentGateAsync(uri, response, content, cancel))
            {
                using var retried = await GetAsync(uri, cancel);
                return retried.IsSuccessStatusCode
                    ? await retried.Content.ReadAsStringAsync(cancel)
                    : null;
            }

            return content;
        }

        return null;
    }

    private async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancel)
    {
        // Not disposed here: the response holds on to the request message, which is where the URI a
        // redirect chain ended on is read from.
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        // To confuse the hosts
        request.Headers.Add("X-Cookies-Accepted", "1");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("Accept-Encoding", "gzip, deflate");
        request.Headers.Add("User-Agent", _userAgent);

        return await _httpClient.SendAsync(request, cancel).ConfigureAwait(false);
    }

    /// <summary>
    ///   Walks through a consent gate, if that is where the request ended up.
    /// </summary>
    /// <remarks>
    ///   A consent gate answers a page request with a page of its own, stating the URL it sends the
    ///   visitor to once consent is given — either in the address it redirected to, or inside the
    ///   interstitial it serves from the site's own host. Fetching that URL is what sets the cookies
    ///   the site then recognizes, and since this client keeps a cookie jar for its whole lifetime,
    ///   a site's gate only has to be walked through once.
    /// </remarks>
    /// <returns>True when the gate was walked through and the original request is worth repeating.</returns>
    private async Task<bool> TryPassConsentGateAsync(Uri requestedUri, HttpResponseMessage response,
        string content, CancellationToken cancel)
    {
        var landedUri = response.RequestMessage?.RequestUri ?? requestedUri;
        var landedOnAnotherSite = !IsSameSite(landedUri, requestedUri);

        var gateUri = landedOnAnotherSite
            // A gate answering from a host of its own states the way back in the URL it sent the
            // visitor to, ...
            ? FindReturnUrl(landedUri, requestedUri)
            // ... while one answering from the site's own host states it in the page it answers with.
            : FindContinuationUrl(content, requestedUri);

        if (gateUri is null)
        {
            if (landedOnAnotherSite)
            {
                _logger.LogWarning("Fetching '{Url}' ended up on '{LandedHost}', which offers no way back.",
                    requestedUri, landedUri.Host);
            }

            return false;
        }

        _logger.LogInformation("Fetching '{Url}' was intercepted; following '{GateUrl}' to carry on.",
            requestedUri, gateUri);

        using var gateResponse = await GetAsync(gateUri, cancel);
        if (!gateResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not get past the gate at '{GateUrl}'. Error Code = {ResponseStatusCode}",
                gateUri, gateResponse.StatusCode);
            return false;
        }

        return true;
    }

    /// <summary>
    ///   Finds the URL an interstitial served by the site itself sends the visitor on to. Only a
    ///   same-site URL addressing something other than what was asked for counts, which rules out
    ///   both a gate pointing elsewhere and a page that merely mentions its own address.
    /// </summary>
    private static Uri? FindContinuationUrl(string content, Uri requestedUri)
    {
        if (content.Length > MAX_INTERSTITIAL_LENGTH)
        {
            return null;
        }

        foreach (var pattern in ContinuationUrlPatterns)
        {
            Match match;
            try
            {
                match = pattern.Match(content);
            }
            catch (RegexMatchTimeoutException)
            {
                continue;
            }

            while (match.Success)
            {
                var candidate = match.Groups["url"].Value;
                // A gate commonly percent-encodes the URL to hand it to decodeURIComponent.
                if (IsUsableContinuation(candidate, requestedUri, out var uri)
                    || IsUsableContinuation(Uri.UnescapeDataString(candidate), requestedUri, out uri))
                {
                    return uri;
                }

                match = match.NextMatch();
            }
        }

        return null;
    }

    private static bool IsUsableContinuation(string candidate, Uri requestedUri, out Uri? continuationUri)
    {
        continuationUri = null;

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || !IsSameSite(uri, requestedUri)
            || string.Equals(uri.AbsolutePath, requestedUri.AbsolutePath, StringComparison.Ordinal))
        {
            return false;
        }

        continuationUri = uri;
        return true;
    }

    /// <summary>
    ///   Reads the URL a gate sends the visitor back to, and only accepts one that points at the
    ///   site that was asked for in the first place — a gate is not to be trusted with sending this
    ///   client anywhere else.
    /// </summary>
    private static Uri? FindReturnUrl(Uri landedUri, Uri requestedUri)
    {
        var query = HttpUtility.ParseQueryString(landedUri.Query);

        foreach (var parameter in ReturnUrlParameters)
        {
            var value = query[parameter];
            if (!string.IsNullOrWhiteSpace(value)
                && Uri.TryCreate(value, UriKind.Absolute, out var candidate)
                && (candidate.Scheme == Uri.UriSchemeHttp || candidate.Scheme == Uri.UriSchemeHttps)
                && IsSameSite(candidate, requestedUri))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool IsSameSite(Uri left, Uri right)
        => string.Equals(left.Host, right.Host, StringComparison.OrdinalIgnoreCase);

    private static HttpClient CreateDefaultHttpClient()
    {
        // One handler for the lifetime of the client: a cookie jar that forgets between requests
        // cannot get past a consent gate, and a handler per request exhausts the socket pool.
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            CookieContainer = new CookieContainer(),
            UseCookies = true,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        };

        return new HttpClient(handler, disposeHandler: true);
    }

    private static string CreateRandomUserAgent()
    {
        var platform = new [] { "Machintosh", "Windows", "X11" }.TakeRandom();
        var os = (platform switch {
            "Machintosh" => new [] { "68K", "PPC" },
            "Windows" => new [] { "Win3.11", "WinNT3.51", "WinNT4.0", "Windows NT 5.0", "Windows NT 5.1", "Windows NT 5.2", "Windows NT 6.0", "Windows NT 6.1", "Windows NT 6.2", "Win95", "Win98", "Win 9x 4.90", "WindowsCE" },
            "X11" => new [] { "Linux i686", "Linux x86_64" },
            _ => Array.Empty<string>()
        }).TakeRandom();
        var browser = new [] { "Chrome", "Firefox", "IE" }.TakeRandom();

        if (browser == "Chrome")
        {
            var webkit = Randomizer.RandomInt(500, 599).ToString(CultureInfo.CurrentCulture);
            var version = $"{Randomizer.RandomInt(0, 24)}.0{Randomizer.RandomInt(0, 1500)}.{Randomizer.RandomInt(0, 999)}";

            return $"Mozilla/5.0 ({os}) AppleWebKit{webkit}.0 (KHTML, live Gecko) Chrome/{version} Safari/{webkit}";
        }
        if (browser == "Firefox")
        {
            var year = Randomizer.RandomInt(2000, 2021);
            var month = Randomizer.RandomInt(1, 12);
            var day = Randomizer.RandomInt(1, 28);
            var gecko = $"{year}{month:00}{day:00}";
            var version = $"{Randomizer.RandomInt(1, 15)}.0";

            return $"Mozilla/5.0 ({os}; rv:{version}) Gecko/{gecko} Firefox/{version}";
        }
        if (browser == "IE")
        {
            var version = $"{Randomizer.RandomInt(1, 10)}.0";
            var engine = $"{Randomizer.RandomInt(1, 5)}.0";
            var option = Randomizer.RandomBool();
            string token;
            if (option)
            {
                var v = new[] { ".NET CLR", "SV1", "Tablet PC", "Win64; IA64", "Win64; x64", "WOW64" }.TakeRandom();
                token = $"{v};";
            }
            else
            {
                token = "";
            }

            return $"Mozilla/5.0 (compatible; MSIE {version}; {os}; {token}Trident/{engine})";
        }

        throw new NotSupportedException();
    }
}
