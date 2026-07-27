using DittoSDK;

namespace SampleApp;

public partial class App : Application
{
    public App(Ditto ditto)
    {
        InitializeComponent();

        MainPage = new AppShell(ditto);
    }
}
