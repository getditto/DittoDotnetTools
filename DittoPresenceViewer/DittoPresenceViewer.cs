namespace DittoTools.PresenceViewer;

using System.Text;
using System.Text.Json;
using DittoSDK;
using global::DittoTools.PresenceViewer.Utils;

public class DittoPresenceViewer : ContentView, IDisposable
{
    private readonly WebView _webView;
    private DittoPresenceObserver? _observer;
    private readonly JsonSerializerOptions _presenceSerializerOptionsOptions;
    
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
            Source = "index.html"
        };

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
        _observer?.Stop();
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
        var presenceGraphJson = JsonSerializer.Serialize(presence, presenceViewer._presenceSerializerOptionsOptions)
            .Replace("\"isDittoCloudConnected\"", "\"isConnectedToDittoCloud\""); //INFO: hack. This property should be named isConnectedToDittoCloud to work with Presence Viewer, like in other SDKs 

        var b64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(presenceGraphJson));

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var currentMs = 0;
            var timeout = 3000;
            const int interval = 50;

            while (!presenceViewer._webView.IsLoaded && currentMs < timeout)
            {
                await Task.Delay(interval);
                currentMs += interval;
            }

            var result = presenceViewer._webView.EvaluateJavaScriptAsync($"Presence.updateNetwork('{b64String}');");
        });
    }
}
