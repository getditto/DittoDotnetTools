using DittoSDK;

namespace SampleApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        _ = DittoSyncPermissions.RequestPermissionsAsync();
    }
}
