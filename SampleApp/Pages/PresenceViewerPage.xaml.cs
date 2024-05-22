using DittoSDK;

namespace SampleApp.Pages;

public partial class PresenceViewerPage : ContentPage
{
    public Ditto Ditto { get; set; }

    public PresenceViewerPage(Ditto ditto)
    {
        Ditto = ditto;

        InitializeComponent();
    }
}
