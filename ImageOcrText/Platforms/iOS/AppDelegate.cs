using Foundation;
using UIKit;

namespace ImageOcrText
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        public static string CurrentPage { get; set; } = "MainPage";

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        /// <summary>
        /// Prevent the app from rotating when the MainPage is displayed (!!!BUG!!! in iOS for the editor)
        /// </summary>
        /// <param name="application"></param>
        /// <param name="forWindow"></param>
        /// <returns></returns>
        [Export("application:supportedInterfaceOrientationsForWindow:")]
        public UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, IntPtr forWindow)
        {
            if (CurrentPage == "MainPage")
            {
                return UIInterfaceOrientationMask.Portrait;
            }
            return UIInterfaceOrientationMask.All;
        }
    }
}
