using DittoSDK;
using DittoSDK.Sync;

namespace SampleApp;

public partial class AppShell : Shell
{
    private readonly Ditto _ditto;

    public AppShell(Ditto ditto)
    {
        _ditto = ditto;
        InitializeComponent();

        // Request Ditto runtime permissions (Bluetooth, Wi-Fi Aware, Location)
        // then start sync. Both steps run async so the UI is shown immediately
        // and sync begins once the user grants (or has previously granted) permissions.
        // Mirroring the pattern from MauiTasksApp/ViewModels/TasksPageviewModel.cs.
        MainThread.BeginInvokeOnMainThread(StartDittoAsync);
    }

    private async void StartDittoAsync()
    {
        try
        {
            await DittoSyncPermissions.RequestPermissionsAsync();
            _ditto.Sync.Start();
        }
        catch (Exception ex)
        {
            // async void swallows unhandled exceptions; log here so the failure
            // is visible rather than silently crashing the UI thread.
            System.Diagnostics.Debug.WriteLine($"[AppShell] StartDittoAsync failed: {ex}");
        }
    }
}
