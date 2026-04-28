using System.Reflection;
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
        var envVars = LoadEnvVariables();
        var appId = RequireEnv(envVars, "DITTO_APP_ID");
        var playgroundToken = RequireEnv(envVars, "DITTO_PLAYGROUND_TOKEN");
        var authUrl = RequireEnv(envVars, "DITTO_AUTH_URL");
        var websocketUrl = RequireEnv(envVars, "DITTO_WEBSOCKET_URL");

        var ditto = new Ditto(
            DittoIdentity.OnlinePlayground(appId, playgroundToken, false, authUrl),
            Path.Combine(FileSystem.Current.AppDataDirectory, "ditto"));

        ditto.UpdateTransportConfig(config =>
        {
            config.Connect.WebsocketUrls.Add(websocketUrl);
        });

        ditto.DisableSyncWithV3();
        ditto.StartSync();

        return ditto;
    }

    /// <summary>
    /// Loads environment variables from the <c>.env</c> resource embedded at the repo root.
    /// </summary>
    private static Dictionary<string, string> LoadEnvVariables()
    {
        var envVars = new Dictionary<string, string>();
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "SampleApp..env";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            var availableResources = string.Join(Environment.NewLine, assembly.GetManifestResourceNames());
            throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found. " +
                "Copy .env.sample to .env at the repo root and fill in your values from https://portal.ditto.live. " +
                $"Available resources:{Environment.NewLine}{availableResources}");
        }

        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
            {
                continue;
            }

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
            {
                continue;
            }

            string key = line.Substring(0, separatorIndex).Trim();
            string value = line.Substring(separatorIndex + 1).Trim();

            if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }

            envVars[key] = value;
        }

        return envVars;
    }

    private static string RequireEnv(Dictionary<string, string> envVars, string key)
    {
        if (!envVars.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"{key} not found in .env. " +
                "Copy .env.sample to .env at the repo root and fill in your values from https://portal.ditto.live.");
        }
        return value;
    }
}
