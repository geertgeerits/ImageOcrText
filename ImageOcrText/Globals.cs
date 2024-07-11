//// Global usings
global using ImageOcrText.Resources.Languages;
global using System.Globalization;
global using System.Diagnostics;
//global using Sentry;

namespace ImageOcrText
{
    //// Global variables and methods
    internal static class Globals
    {
        //// Global variables
        public static string cTheme = "";
        public static string cLanguage = "";
        public static bool bLanguageChanged;
        public static string cLanguageSpeech = "";
        public static string[]? cLanguageLocales;
        public static bool bLanguageLocalesExist;
        public static bool bTextToSpeechIsBusy;
        public static int nLanguageOcrIndex = 0;
        public static string cLanguageOcr = "";
        public static List<string> supportedLanguagesOcr = [];
        public static IEnumerable<Locale>? locales;
        public static CancellationTokenSource? cts;
        public static string cImageTextToSpeech = "ic_action_volume_mute.png";
        public static string cImageTextToSpeechCancel = "ic_action_volume_up.png";
        public static bool bLicense;

        //// Global methods

        /// <summary>
        /// Set the theme
        /// </summary>
        public static void SetTheme()
        {
            Application.Current!.UserAppTheme = cTheme switch
            {
                "Light" => AppTheme.Light,
                "Dark" => AppTheme.Dark,
                _ => AppTheme.Unspecified,
            };
        }

        /// <summary>
        /// Set the current UI culture of the selected language
        /// </summary>
        public static void SetCultureSelectedLanguage()
        {
            try
            {
                //CodeLang.Culture = new CultureInfo(cLanguage);
                CultureInfo switchToCulture = new(cLanguage);
                LocalizationResourceManager.Instance.SetCulture(switchToCulture);
            }
            catch
            {
                // Do nothing
            }
        }

        /// <summary>
        /// Get ISO language (and country) code from locales
        /// </summary>
        /// <returns></returns>
        public static string GetIsoLanguageCode()
        {
            // Split before first space and remove last character '-' if there
            string cLanguageIso = cLanguageSpeech.Split(' ').First();

            if (cLanguageIso.EndsWith('-'))
            {
                cLanguageIso = cLanguageIso.Remove(cLanguageIso.Length - 1, 1);
            }

            return cLanguageIso;
        }

        /// <summary>
        /// Button text to speech event - Convert text to speech
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cText"></param>
        /// <returns></returns>
        public static async Task ConvertTextToSpeechAsync(object sender, string cText)
        {
            var imageButton = (ImageButton)sender;

            // Start with the text to speech
            if (cText != null && cText != "")
            {
                bTextToSpeechIsBusy = true;
                imageButton.Source = cImageTextToSpeechCancel;

                try
                {
                    cts = new CancellationTokenSource();

                    SpeechOptions options = new()
                    {
                        Locale = locales?.Single(l => $"{l.Language}-{l.Country} {l.Name}" == cLanguageSpeech)
                    };

                    await TextToSpeech.Default.SpeakAsync(cText, options, cancelToken: cts.Token);
                    bTextToSpeechIsBusy = false;
                }
                catch (Exception ex)
                {
                    //SentrySdk.CaptureException(ex);
#if DEBUG
                    await Application.Current!.MainPage!.DisplayAlert(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
#endif
                }

                imageButton.Source = cImageTextToSpeech;
            }
        }

        /// <summary>
        /// Cancel the text to speech
        /// </summary>
        /// <returns></returns>
        public static string CancelTextToSpeech()
        {
            // Cancel speech if a cancellation token exists & hasn't been already requested
            if (bTextToSpeechIsBusy)
            {
                if (cts?.IsCancellationRequested ?? true)
                    return cImageTextToSpeechCancel;

                cts.Cancel();
                bTextToSpeechIsBusy = false;
            }

            return cImageTextToSpeech;
        }
    }
}
