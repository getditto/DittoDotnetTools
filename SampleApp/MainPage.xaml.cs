using System.Collections.ObjectModel;
using DittoSDK;
using SampleApp.Pages;

namespace SampleApp;

public partial class MainPage : ContentPage
{
    public ObservableCollection<NavigationOption> Options { get; set; }

    public MainPage()
	{
        Options = new ObservableCollection<NavigationOption>
        {
            new NavigationOption { Name = "Presence Viewer", TargetType = typeof(PresenceViewerPage) },
            new NavigationOption { Name = "Heartbeat", TargetType = typeof(HeartbeatPage) },
        };

        InitializeComponent();
    }

    async void ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var selectedOption = e.SelectedItem as NavigationOption;

        if (selectedOption != null)
        {
            var page = (Page)ActivatorUtilities.CreateInstance(Utils.ServiceProvider.Current, selectedOption.TargetType);
            await Navigation.PushAsync(page);

            ((ListView)sender).SelectedItem = null;
        }
    }
}


public class NavigationOption
{
    public required string Name { get; set; }
    public required Type TargetType { get; set; }
}