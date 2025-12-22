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

            .ConfigureLifecycleEvents(static events =>
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
             });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        /// <summary>
        /// Process lifecycle event
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool ProcessEvent(string eventName, string? type = null)
        {
            Debug.WriteLine($"Lifecycle event: {eventName}{(type is null ? string.Empty : $" ({type})")}");

            // Cancel speech
            ClassSpeech.CancelTextToSpeech();

            return true;
        }
    }
}
