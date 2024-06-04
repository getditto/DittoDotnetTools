using System.Text.Json;
using DittoSDK;
using DittoTools.Heartbeat;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace SampleApp.Pages;

public partial class HeartbeatPage : ContentPage
{
    private const string Query = $"SELECT * FROM {DittoTools.Heartbeat.Constants.collectionName}";
    private readonly DittoHeartbeatConfig _mockConfig = DittoHeartbeatConfig.MockConfig;

    private Ditto _ditto;
    private bool _isHeartbeatRunning;
    private DittoHeartbeat _heartbeat;
    private DittoStoreObserver _infoObserver;

    public List<DittoHeartbeatInfo> HeartBeatInfos { get; set; } = new List<DittoHeartbeatInfo>();

    public HeartbeatPage(Ditto ditto)
    {
        InitializeComponent();
        _ditto = ditto;
    }

    void PlayPauseClicked(System.Object sender, System.EventArgs e)
    {
        _isHeartbeatRunning = !_isHeartbeatRunning;
        playPauseButton.IconImageSource = _isHeartbeatRunning ? "icon-pause.png" : "icon-play.png";
        this.mainLabel.IsVisible = !_isHeartbeatRunning;
        this.mainListView.IsVisible = _isHeartbeatRunning;
        ToggleHeartbeat(_isHeartbeatRunning);
    }

    void ToggleHeartbeat(bool isOn)
    {
        if (isOn)
        {
            Action<DittoHeartbeatInfo> callback = (info) =>
            {
                if (_infoObserver == null || _infoObserver.IsCancelled)
                {
                    _infoObserver = _ditto.Store.RegisterObserver(Query, (result) =>
                    {
                        HeartBeatInfos = result
                            .Items
                            .Select(s => JsonSerializer.Deserialize<DittoHeartbeatInfo>(s.JsonString()))
                            .Where(s => s != null)
                            .ToList();
                        OnPropertyChanged(nameof(HeartBeatInfos));
                    });
                }
            };

            _heartbeat = new DittoHeartbeat();
            _heartbeat.StartHeartbeat(_ditto, _mockConfig, callback);
        }
        else
        {
            _heartbeat?.StopHeartbeat();
            _infoObserver.Cancel();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _heartbeat?.StopHeartbeat();
        _isHeartbeatRunning = false;
        _infoObserver?.Cancel();
    }
}
