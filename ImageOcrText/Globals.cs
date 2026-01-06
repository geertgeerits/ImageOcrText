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
        public static bool bTextToSpeechAvailable;
        public static bool bTextToSpeechIsBusy;
        public static int nLanguageOcrIndex;
        public static string cLanguageOcr = "";
        public static List<string> supportedLanguagesOcr = [];
        public static readonly string cImageTextToSpeech = "ic_action_volume_up.png";
        public static readonly string cImageTextToSpeechCancel = "ic_action_volume_mute.png";
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
        public static void SetCultureSelectedLanguage(string cCultureName)
        {
            try
            {
                CultureInfo switchToCulture = new(cCultureName);
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
                cLanguageIso = cLanguageIso[..^1];
            }

            return cLanguageIso;
        }
    }
}
