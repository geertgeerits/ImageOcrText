using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.OCR;

namespace ImageOcrText
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseOcr()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })

            .ConfigureLifecycleEvents(events =>
             {
#if ANDROID
                    events.AddAndroid(android => android
                        .OnPause((activity) => ProcessEvent(nameof(AndroidLifecycle.OnPause))));
#endif

#if IOS
                 events.AddiOS(ios => ios
                     .OnResignActivation((app) => ProcessEvent(nameof(iOSLifecycle.OnResignActivation))));
#endif

#if WINDOWS
                    events.AddWindows(windows => windows
                        .OnVisibilityChanged((window, args) => ProcessEvent(nameof(WindowsLifecycle.OnVisibilityChanged))));
#endif

                 static bool ProcessEvent(string eventName, string type = null)
                 {
                     //System.Diagnostics.Debug.WriteLine($"Lifecycle event: {eventName}{(type == null ? string.Empty : $" ({type})")}");

                     // Cancel speech if a cancellation token exists & hasn't been already requested.
                     if (Globals.bTextToSpeechIsBusy)
                     {
                         if (Globals.cts?.IsCancellationRequested ?? true)
                             return true;

                         Globals.cts.Cancel();
                         Globals.bTextToSpeechIsBusy = false;
                     }

                     return true;
                 }
             });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
