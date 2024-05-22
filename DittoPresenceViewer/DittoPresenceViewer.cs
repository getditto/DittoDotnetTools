namespace DittoTools.PresenceViewer;

using System.Text;
using System.Text.Json;
using DittoSDK;
using global::DittoTools.PresenceViewer.Utils;

public class DittoPresenceViewer : ContentView, IDisposable
{
    private readonly WebView _webView;
    private DittoPresenceObserver? _observer;

    public DittoPresenceViewer()
    {
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
        Console.WriteLine($" -  - Presence Change - -  {presence.RemotePeers?.Count() ?? 0}");
        var options = new JsonSerializerOptions()
        {
            IncludeFields = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        options.Converters.Add(new ByteArrayToIntArrayConverter());
        options.Converters.Add(new StringToEnumConverter<DittoConnectionType>());

        var presenceGraphJSON = JsonSerializer.Serialize(presence, options);
        var b64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(presenceGraphJSON));


        MainThread.BeginInvokeOnMainThread(() =>
        {
            var result = presenceViewer._webView.EvaluateJavaScriptAsync($"Presence.updateNetwork('{b64String}');");
            Console.WriteLine(result);
        });

    }
}
