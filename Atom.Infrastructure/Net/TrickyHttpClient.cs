using System.Collections.Concurrent;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Genius.Atom.Infrastructure.Net;

public interface ITrickyHttpClient
{
    Task<string?> DownloadContentAsync(string url, CancellationToken cancel);
}

internal sealed class TrickyHttpClient : ITrickyHttpClient
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _lockers = new();
    private readonly ILogger<TrickyHttpClient> _logger;

    private const int DELAY_MS = 500;
    private const int MAX_REPEATS = 5;

    public TrickyHttpClient(ILogger<TrickyHttpClient> logger)
    {
        _logger = logger;
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
            return await DownloadInternalAsync(url, cancel);
        }
        finally
        {
            locker.Release();
        }
    }

    private async Task<string?> DownloadInternalAsync(string url, CancellationToken cancel)
    {
        var handler = new HttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        for (var irepeat = 1; irepeat <= MAX_REPEATS; irepeat++)
        {
            using var httpClient = new HttpClient(handler);

            // To confuse the hosts
            httpClient.DefaultRequestHeaders.Add("X-Cookies-Accepted", "1");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("User-Agent", CreateRandomUserAgent());

            using var response = await httpClient.GetAsync(url, cancel);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(DELAY_MS * (irepeat + 1));
                    continue;
                }

                // Something went wrong
                _logger.LogError("Failed to fetch '{Url}'. Error Code = {ResponseStatusCode}", url, response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        return null;
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
            var webkit = Randomizer.RandomInt(500, 599).ToString();
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
