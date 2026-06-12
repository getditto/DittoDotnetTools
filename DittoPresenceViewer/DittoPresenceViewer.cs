namespace DittoTools.PresenceViewer;

using System.Text;
using System.Text.Json;
using DittoSDK;
using DittoSDK.Transport;
using global::DittoTools.PresenceViewer.Utils;

public class DittoPresenceViewer : ContentView, IDisposable
{
    private readonly WebView _webView;
    private DittoPresenceObserver? _observer;
    private readonly JsonSerializerOptions _presenceSerializerOptionsOptions;

    // True once WebView.Navigated fires with Success AND window.Presence is reachable.
    private bool _pageReady;

    // The most-recently-received presence payload (base64). Kept so that the
    // first graph (which often arrives before navigation completes) is not lost.
    private string? _pendingB64;

    // Guards concurrent access to _pageReady / _pendingB64 from the observer
    // thread and the Navigated callback, both of which marshal to the main thread.
    private readonly object _stateLock = new object();

    public DittoPresenceViewer()
    {
        _presenceSerializerOptionsOptions = new JsonSerializerOptions()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        _presenceSerializerOptionsOptions.Converters.Add(new ByteArrayToIntArrayConverter());
        _presenceSerializerOptionsOptions.Converters.Add(new StringToEnumConverter<DittoConnectionType>());

        _webView = new WebView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Source = "index.html",
        };

        _webView.Navigated += OnWebViewNavigated;

        Content = _webView;
    }

    public Ditto Ditto
    {
        get => (Ditto)GetValue(DittoProperty);
        set => SetValue(DittoProperty, value);
    }

    public static readonly BindableProperty DittoProperty = BindableProperty.Create(
        propertyName: nameof(Ditto),
        returnType: typeof(Ditto),
        declaringType: typeof(DittoPresenceViewer),
        defaultValue: default(Ditto),
        propertyChanged: OnDittoChanged);

    public void Dispose()
    {
        _webView.Navigated -= OnWebViewNavigated;
        _observer?.Stop();
    }

    // Called on the main thread by MAUI when the WebView finishes loading a page.
    private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result != WebNavigationResult.Success)
        {
            return;
        }

        // The page HTML has loaded, but index.html uses a single
        // <script type=module>, which executes asynchronously after parse.
        // Poll until window.Presence is reachable (up to ~1 s).
        const int maxAttempts = 20;
        const int intervalMs = 50;

        for (int i = 0; i < maxAttempts; i++)
        {
            var result = await _webView.EvaluateJavaScriptAsync(
                "typeof window.Presence !== 'undefined' && typeof window.Presence.updateNetwork === 'function' ? 'ready' : 'wait'");

            // Some platforms return quoted strings (e.g. "\"ready\""); normalise
            // by trimming whitespace and stripping surrounding double-quotes.
            var normalized = result?.Trim().Trim('"');
            if (normalized == "ready")
            {
                break;
            }

            await Task.Delay(intervalMs);
        }

        // Whether or not polling timed out, mark ready and flush any pending payload.
        string? toFlush;
        lock (_stateLock)
        {
            _pageReady = true;
            toFlush = _pendingB64;
        }

        if (toFlush != null)
        {
            await PushToWebViewAsync(toFlush);
        }
    }

    private static void OnDittoChanged(BindableObject source, object o, object n)
    {
        if (source is not DittoPresenceViewer presenceViewer)
        {
            return;
        }

        presenceViewer._observer?.Stop();
        presenceViewer._observer = null;

        if (n is Ditto newDitto)
        {
            presenceViewer._observer = newDitto.Presence.Observe((graph) =>
            {
                OnDittoPresenceChange(presenceViewer, graph);
            });
        }
    }

    private static void OnDittoPresenceChange(DittoPresenceViewer presenceViewer, DittoPresenceGraph presence)
    {
        // In SDK v5 DittoPeer.IsConnectedToDittoServer carries
        // [JsonPropertyName("isConnectedToDittoCloud")], so the JSON output
        // already uses the name the Presence Viewer web component expects.
        var presenceGraphJson = JsonSerializer.Serialize(
            presence, presenceViewer._presenceSerializerOptionsOptions);
        var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(presenceGraphJson));

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bool ready;
            lock (presenceViewer._stateLock)
            {
                presenceViewer._pendingB64 = b64;
                ready = presenceViewer._pageReady;
            }

            if (ready)
            {
                await presenceViewer.PushToWebViewAsync(b64);
            }
            // If not ready, the payload is stored in _pendingB64 and will be
            // flushed by OnWebViewNavigated once the module has loaded.
        });
    }

    // Evaluates the update call defensively so a timing edge-case (e.g. page
    // reload) cannot throw a JS ReferenceError that silently swallows the update.
    private async Task PushToWebViewAsync(string b64)
    {
        await _webView.EvaluateJavaScriptAsync(
            $"if (window.Presence && window.Presence.updateNetwork) {{ window.Presence.updateNetwork('{b64}'); }}");
    }
}
