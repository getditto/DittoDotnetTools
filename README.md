# DittoDotnetTools 

DittoDotnetTools are diagnostic tools for Ditto. 

These tools are available through NuGet. 

Issues and pull requests welcome!

## Usage 

These tools require you to have an already-initialized Ditto instance. Take a look at the [C# Documentation](https://docs.ditto.live/get-started/install-guides/c-sharp). 


Tools are individually shipped under different NuGet packages. 

### 1. Presence Viewer 

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