using System.Net;
using Genius.Atom.Infrastructure.Net;
using Genius.Atom.Infrastructure.TestingUtil;

namespace Genius.Atom.Infrastructure.Tests.Net;

public sealed class TrickyHttpClientTests
{
    private const string ProductUrl = "https://example.com/products/42";
    private const string GateUrl = "https://consent.example-gate.com/consent?siteKey=abc&callbackUrl=";
    private const string ReturnUrl = "https://example.com/consent-confirm?redirectUri=%2Fproducts%2F42";

    [Fact]
    public async Task DownloadContentAsync_GivenPlainPage_WhenFetched_ThenReturnsItsContent()
    {
        // Arrange
        var handler = new StubHandler();
        handler.Respond(ProductUrl, "the page");
        using var sut = CreateSystemUnderTest(handler);

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Equal("the page", content);
        Assert.Equal([ProductUrl], handler.RequestedUrls);
    }

    [Fact]
    public async Task DownloadContentAsync_GivenConsentGate_WhenFetched_ThenAcceptsItAndRepeatsTheRequest()
    {
        // Arrange
        var handler = new StubHandler();
        // The gate answers the first attempt with its own page on its own host, ...
        handler.Respond(ProductUrl, "the gate", landsOn: GateUrl + Uri.EscapeDataString(ReturnUrl));
        handler.Respond(ReturnUrl, "consent taken");
        using var sut = CreateSystemUnderTest(handler);
        // ... and the site serves the page once the return URL has been fetched.
        handler.OnRequested(ReturnUrl, () => handler.Respond(ProductUrl, "the page"));

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Equal("the page", content);
        Assert.Equal([ProductUrl, ReturnUrl, ProductUrl], handler.RequestedUrls);
    }

    [Theory]
    // The interstitial hands the URL to decodeURIComponent for its script to redirect with, ...
    [InlineData("<script>const url = new URL(decodeURIComponent('{0}'))</script>")]
    // ... or assigns it straight away, in any of the spellings that has, ...
    [InlineData("<script>window.location.href = \"{1}\";</script>")]
    [InlineData("<script>location.replace('{1}')</script>")]
    [InlineData("<script>window.location = \"{1}\";</script>")]
    // ... or leaves it to a meta refresh.
    [InlineData("<meta http-equiv=\"refresh\" content=\"0;url={1}\">")]
    public async Task DownloadContentAsync_GivenGateOnTheSiteItself_WhenFetched_ThenFollowsItOnAndRepeats(
        string interstitial)
    {
        // Arrange
        var handler = new StubHandler();
        handler.Respond(ProductUrl, string.Format(interstitial,
            Uri.EscapeDataString(ReturnUrl), ReturnUrl));
        handler.Respond(ReturnUrl, "consent taken");
        using var sut = CreateSystemUnderTest(handler);
        handler.OnRequested(ReturnUrl, () => handler.Respond(ProductUrl, "the page"));

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Equal("the page", content);
        Assert.Equal([ProductUrl, ReturnUrl, ProductUrl], handler.RequestedUrls);
    }

    [Fact]
    public async Task DownloadContentAsync_GivenPageMentioningAnotherSite_WhenFetched_ThenLeavesItAlone()
    {
        // Arrange
        var handler = new StubHandler();
        handler.Respond(ProductUrl, "<script>window.location.href = \"https://somewhere-else.com/x\";</script>");
        using var sut = CreateSystemUnderTest(handler);

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Contains("somewhere-else", content);
        Assert.Equal([ProductUrl], handler.RequestedUrls);
    }

    [Fact]
    public async Task DownloadContentAsync_GivenFullPage_WhenFetched_ThenIsNotSearchedForAGate()
    {
        // Arrange: a real page is over the interstitial size and may well link to itself elsewhere.
        var handler = new StubHandler();
        var page = new string('x', 64 * 1024 + 1)
            + $"<a href=\"{ReturnUrl}\">terms</a><script>location.href = \"{ReturnUrl}\";</script>";
        handler.Respond(ProductUrl, page);
        using var sut = CreateSystemUnderTest(handler);

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Equal(page, content);
        Assert.Equal([ProductUrl], handler.RequestedUrls);
    }

    [Fact]
    public async Task DownloadContentAsync_GivenGateSendingElsewhere_WhenFetched_ThenDoesNotFollowIt()
    {
        // Arrange
        var handler = new StubHandler();
        handler.Respond(ProductUrl, "the gate",
            landsOn: GateUrl + Uri.EscapeDataString("https://somewhere-else.com/anything"));
        using var sut = CreateSystemUnderTest(handler);

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify: a gate only gets to send this client back to the site it asked for.
        Assert.Equal("the gate", content);
        Assert.Equal([ProductUrl], handler.RequestedUrls);
    }

    [Fact]
    public async Task DownloadContentAsync_GivenTooManyRequests_WhenFetched_ThenRepeatsUntilServed()
    {
        // Arrange
        var handler = new StubHandler();
        handler.Respond(ProductUrl, "too many", HttpStatusCode.TooManyRequests);
        using var sut = CreateSystemUnderTest(handler);
        handler.OnRequested(ProductUrl, () => handler.Respond(ProductUrl, "the page"));

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Equal("the page", content);
        Assert.Equal([ProductUrl, ProductUrl], handler.RequestedUrls);
    }

    [Fact]
    public async Task DownloadContentAsync_GivenFailure_WhenFetched_ThenReturnsNull()
    {
        // Arrange
        var handler = new StubHandler();
        handler.Respond(ProductUrl, "gone", HttpStatusCode.NotFound);
        using var sut = CreateSystemUnderTest(handler);

        // Act
        var content = await sut.DownloadContentAsync(ProductUrl, TestContext.Current.CancellationToken);

        // Verify
        Assert.Null(content);
    }

    private static TrickyHttpClient CreateSystemUnderTest(StubHandler handler)
        => new(new FakeLogger<TrickyHttpClient>(), new HttpClient(handler));

    /// <summary>
    ///   Answers the requests this client makes, and records them in order. <c>landsOn</c> stands for
    ///   the URL a redirect chain would have ended on, which is what the client reads the host off.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, (string Content, HttpStatusCode Status, string? LandsOn)> _responses = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Action> _afterRequested = new(StringComparer.OrdinalIgnoreCase);

        public List<string> RequestedUrls { get; } = [];

        public void Respond(string url, string content, HttpStatusCode status = HttpStatusCode.OK, string? landsOn = null)
            => _responses[url] = (content, status, landsOn);

        /// <summary>Runs once, right after the given URL has been requested.</summary>
        public void OnRequested(string url, Action action)
            => _afterRequested[url] = action;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.ToString();
            RequestedUrls.Add(url);

            if (!_responses.TryGetValue(url, out var stubbed))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request });
            }

            if (stubbed.LandsOn is not null)
            {
                request.RequestUri = new Uri(stubbed.LandsOn);
            }

            var response = new HttpResponseMessage(stubbed.Status)
            {
                Content = new StringContent(stubbed.Content),
                RequestMessage = request,
            };

            if (_afterRequested.Remove(url, out var action))
            {
                action();
            }

            return Task.FromResult(response);
        }
    }
}
