#  Ditto .NET Tools  


<img src="icon.png" alt="Ditto Logo" width="100">
<br/>

Ditto .NET Tools are diagnostic tools for Ditto. 

These tools are available through NuGet. 

For support, please contact Ditto Support (<support@ditto.live>).

## Usage 

These tools require you to have an already-initialized Ditto instance. Take a look at the [C# Documentation](https://docs.ditto.live/get-started/install-guides/c-sharp). 


Tools are individually shipped under different NuGet packages. 

### 1. Presence Viewer 

[![NuGet](https://img.shields.io/nuget/v/DittoTools.PresenceViewer.svg)](https://www.nuget.org/packages/DittoTools.PresenceViewer)

The Presence Viewer displays a mesh graph that allows you to see all connected peers within the mesh and the transport each peer is using to make a connection.

 <img src="Img/presenceViewer.png" alt="Presence Viewer Image" width="300">  

You can use Presence Viewer in your .NET MAUI applications: 

1. Add the package: 

```
<PackageReference Include="DittoTools.PresenceViewer" Version="1.0.0-alpha.1" />
```

2. Create a blank .NET MAUI Page and reference the Presence Viewer. 

Add the namespace definition: 

```xml
xmlns:presence="clr-namespace:DittoTools.PresenceViewer;assembly=DittoPresenceViewer"
```

Then simply include the view making sure to properly assign the Ditto instance: 

```xml 
<presence:DittoPresenceViewer
    Ditto="{Binding Ditto, Source={x:Reference self}}"/>
```

Full example: 

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="DittoMauiTasksApp.PresenceViewerPage"
             xmlns:presence="clr-namespace:DittoTools.PresenceViewer;assembly=DittoPresenceViewer"
             x:Name="self"
             Shell.PresentationMode="Modal"
             Title="PresenceViewerPage">
    <ContentPage.ToolbarItems>
        <ToolbarItem Text="Close"
                     Clicked="ToolbarItem_Clicked"/>
    </ContentPage.ToolbarItems>
    <presence:DittoPresenceViewer
        Ditto="{Binding Ditto, Source={x:Reference self}}"/>
</ContentPage>
```

And the code behind: 

```csharp
using System;
using DittoSDK;
using Microsoft.Maui.Controls;

namespace DittoMauiTasksApp;

public partial class PresenceViewerPage : ContentPage
{
    public Ditto Ditto { get; set; }

    public PresenceViewerPage(Ditto ditto)
    {
        this.Ditto = ditto;

        InitializeComponent();
    }

    async void ToolbarItem_Clicked(System.Object sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
```
_ _ _ 

### 2. Heartbeat

The Ditto Heartbeat tool allows you to monitor, locally or remotely, the peers in your mesh. It allows you to regularly report data and health of the device.

**Configure Heartbeat**

These are the values you need to provide to the Heartbeat:
1. `Id` - Unique value that identifies the device.
2. `SecondsInterval` - The frequency at which the Heartbeat will scrape the data.
3. `Metadata` (optional) - Any metadata you want to attach to this heartbeat.

There is a `DittoHeartbeatConfig` struct you can use to construct your configuration.

```csharp
// Provided with the Heartbeat tool
public class DittoHeartbeatConfig 
{
    public string Id { get; private set; }

    public int SecondsInterval { get; private set; }

    public Dictionary<string, object>? Metadata { get; private set; }
}
```

This tool generates a `DittoHeartbeatInfo` object with the given data:

```csharp
public class DittoHeartbeatInfo
{
   public string Id { get; internal set; }

   public string Schema { get; internal set; }

   public int SecondsInterval { get; internal set;  }

   public string LastUpdated { get; internal set; }

   public string Sdk { get; internal set; }

   public int PresenceSnapshotDirectlyConnectedPeersCount { get; internal set; }

   public Dictionary<string, object> PresenceSnapshotDirectlyConnectedPeers { get; internal set; }

   public Dictionary<string, object>? Metadata { get; internal set;  }

   public string PeerKey { get; internal set; }
}
```

You can either check the provided UI in the sample app, or create your own and use the data as you please. 

**Read data:**

```csharp
var hearbeat = new DittoHeartbeat();
var config = new DittoHeartbeatConfig("<ID>", 10);
heartbeat.StartHeartbeat(ditto, config, (heartbeatInfo) => {
    // use data
});
```


## Ditto Tools Example App 


The [Ditto Tools Example App](https://github.com/getditto/DittoDotnetTools/tree/main/SampleApp) included in this repo allows you to try the DittoDotnetTools in a standalone .NET MAUI app. 

