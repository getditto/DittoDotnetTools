using DittoSDK;
using Microsoft.Extensions.Logging;
using SampleApp.Pages;

namespace SampleApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton(SetupDitto());
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<PresenceViewerPage>();


        return builder.Build();
	}


    private static Ditto SetupDitto()
    {
        var id = DittoIdentity.OnlinePlayground("<APP_ID>", "<TOKEN>", false);

        var ditto = new Ditto(id);
        ditto.DisableSyncWithV3();
        ditto.StartSync();

        return ditto;
    }
}
