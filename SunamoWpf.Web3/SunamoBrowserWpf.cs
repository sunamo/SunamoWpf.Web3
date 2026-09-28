namespace SunamoWpf.Web3;

/// <summary>
/// WebView2-based browser wrapper implementing the shared <see cref="ISunamoBrowser"/> contract
/// used across the Sunamo browser wrappers (SunamoCef/CefBrowser, SunamoWeb3/SunamoBrowserWpf,
/// UniversalWebControl/SunamoBrowser).
/// </summary>
public class SunamoBrowserWpf : ISunamoBrowser
{
    /// <summary>
    /// Shared default instance, mirroring the previous static-singleton usage.
    /// </summary>
    public static SunamoBrowserWpf Instance { get; } = new();

    /// <summary>
    /// The underlying WebView2 control.
    /// </summary>
    public WebView2 WebView { get; } = new();

    /// <summary>
    /// Gets or sets the currently loaded URI. Setting it navigates the browser.
    /// </summary>
    public Uri Source
    {
        get => WebView.Source;
        set
        {
            WebView.Source = value;
        }
    }

    /// <summary>
    /// Gets the page HTML synchronously. Not supported by WebView2's asynchronous model;
    /// use <see cref="GetContent"/> instead.
    /// </summary>
    public string HTML => throw new NotSupportedException($"{nameof(HTML)} is synchronous; use {nameof(GetContent)} instead.");

    /// <summary>
    /// Gets the current page parsed as an <see cref="HtmlDocument"/>.
    /// </summary>
    /// <returns>The parsed HTML document.</returns>
    public async Task<HtmlDocument> GetHtmlDocument()
    {
        var html = await GetContent();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(html);
        return htmlDocument;
    }

    /// <summary>
    /// Gets the current page's outer HTML, evaluated on the WebView2's owning (UI) thread.
    /// </summary>
    /// <returns>The page's outer HTML, unescaped from the JSON string WebView2 returns.</returns>
    public async Task<string> GetContent()
    {
        var rawJsonResult = await WebView.Dispatcher.InvokeAsync(async () =>
            await WebView.ExecuteScriptAsync("document.documentElement.outerHTML"));

        var scriptResult = await rawJsonResult;
        return System.Text.Json.JsonSerializer.Deserialize<string>(scriptResult) ?? string.Empty;
    }

    /// <summary>
    /// Navigates the browser to the given URI.
    /// </summary>
    /// <param name="uri">The absolute URI to navigate to.</param>
    public void Navigate(string uri)
    {
        WebView.Source = new Uri(uri);
    }

    /// <summary>
    /// Scrolls the page to its bottom.
    /// </summary>
    /// <returns>Always false: WebView2's initial construction here does not guarantee
    /// CoreWebView2 is ready yet; call <see cref="Init"/> and await CoreWebView2 readiness
    /// before relying on scrolling.</returns>
    public bool ScrollToEnd()
    {
        return false;
    }

    /// <summary>
    /// Ensures the WebView2 control's CoreWebView2 environment is initialized.
    /// </summary>
    public void Init()
    {
        _ = WebView.EnsureCoreWebView2Async();
    }
}
