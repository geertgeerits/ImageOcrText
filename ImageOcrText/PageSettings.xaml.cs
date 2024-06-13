namespace ImageOcrText
{
    public sealed partial class PageSettings : ContentPage
    {
        //// Local variables
        private readonly Stopwatch stopWatch = new();

        public PageSettings()
    	{
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                DisplayAlert("InitializeComponent: PageSettings", ex.Message, "OK");
                return;
            }

            //// Put text in the chosen language in the controls and variables
            SetLanguage();

            //// Set the current language in the picker
            pckLanguage.SelectedIndex = Globals.cLanguage switch
            {
                "cs" => 0,      // Čeština - Czech
                "da" => 1,      // Dansk - Danish
                "de" => 2,      // Deutsch - German
                "es" => 4,      // Español - Spanish
                "fr" => 5,      // Français - French
                "it" => 6,      // Italiano - Italian
                "hu" => 7,      // Magyar - Hungarian
                "nl" => 8,      // Nederlands - Dutch
                "nb" => 9,      // Norsk Bokmål - Norwegian Bokmål
                "pl" => 10,     // Polski - Polish
                "pt" => 11,     // Português - Portuguese
                "ro" => 12,     // Română - Romanian
                "fi" => 13,     // Suomi - Finnish
                "sv" => 14,     // Svenska - Swedish
                _ => 3,         // English
            };

            //// Fill the picker with the speech languages and set the saved language in the picker
            FillPickerWithSpeechLanguages();

            //// Fill the picker with the OCR languages and set the saved language in the picker
            FillPickerWithOcrLanguages();

            // Start the stopWatch for resetting all the settings
            stopWatch.Start();
        }

        /// <summary>
        /// Picker language clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerLanguageChanged(object sender, EventArgs e)
        {
            string cLanguageOld = Globals.cLanguage;

            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.cLanguage = selectedIndex switch
                {
                    0 => "cs",      // Čeština - Czech
                    1 => "da",      // Dansk - Danish
                    2 => "de",      // Deutsch - German
                    4 => "es",      // Español - Spanish
                    5 => "fr",      // Français - French
                    6 => "it",      // Italiano - Italian
                    7 => "hu",      // Magyar - Hungarian
                    8 => "nl",      // Nederlands - Dutch
                    9 => "nb",      // Norsk Bokmål - Norwegian Bokmål
                    10 => "pl",     // Polski - Polish
                    11 => "pt",     // Português - Portuguese
                    12 => "ro",     // Română - Romanian
                    13 => "fi",     // Suomi - Finnish
                    14 => "sv",     // Svenska - Swedish
                    _ => "en",      // English
                };
            }

            if (cLanguageOld != Globals.cLanguage)
            {
                Globals.bLanguageChanged = true;

                // Set the current UI culture of the selected language
                Globals.SetCultureSelectedLanguage();

                // Put text in the chosen language in the controls and variables
                SetLanguage();

                // Search the new language in the cLanguageLocales array and select the new speech language
                int nTotalItems = Globals.cLanguageLocales.Length;

                for (int nItem = 0; nItem < nTotalItems; nItem++)
                {
                    if (Globals.cLanguageLocales[nItem].StartsWith(Globals.cLanguage))
                    {
                        pckLanguageSpeech.SelectedIndex = nItem;
                        break;
                    }
                }

                // Set the OCR language
                Globals.supportedLanguagesOcr[0] = OcrLang.LanguageOcrAll_Text;
                pckLanguageOcr.Items.Clear();
                FillPickerWithOcrLanguages();
            }
        }

        /// <summary>
        /// Put text in the chosen language in the controls and variables
        /// </summary>
        private void SetLanguage()
        {
            // Set the current theme in the picker
            List<string> ThemeList =
            [
                OcrLang.ThemeSystem_Text,
                OcrLang.ThemeLight_Text,
                OcrLang.ThemeDark_Text
            ];
            pckTheme.ItemsSource = ThemeList;

            pckTheme.SelectedIndex = Globals.cTheme switch
            {
                "Light" => 1,       // Light
                "Dark" => 2,        // Dark
                _ => 0,             // System
            };
        }

        /// <summary>
        /// Fill the picker with the speech languages from the array
        /// </summary>
        private void FillPickerWithSpeechLanguages()
        {
            // .Country = KR ; .Id = ''  ; .Language = ko ; .Name = Korean (South Korea) ;

            // If there are no locales then return
            bool bIsSetSelectedIndex = false;

            if (!Globals.bLanguageLocalesExist)
            {
                pckLanguageSpeech.IsEnabled = false;
                return;
            }

            // Put the sorted locales from the array in the picker and select the saved language
            int nTotalItems = Globals.cLanguageLocales.Length;

            for (int nItem = 0; nItem < nTotalItems; nItem++)
            {
                pckLanguageSpeech.Items.Add(Globals.cLanguageLocales[nItem]);

                if (Globals.cLanguageSpeech == Globals.cLanguageLocales[nItem])
                {
                    pckLanguageSpeech.SelectedIndex = nItem;
                    bIsSetSelectedIndex = true;
                }
            }

            // If the language is not found set the picker to the first item
            if (!bIsSetSelectedIndex)
            {
                pckLanguageSpeech.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Fill the picker with the OCR languages from the list
        /// </summary>
        private void FillPickerWithOcrLanguages()
        {
            // Put OCR languages from the list in the picker and select the saved OCR language
            int nTotalItems = Globals.supportedLanguagesOcr.Count;

            for (int nItem = 0; nItem < nTotalItems; nItem++)
            {
                pckLanguageOcr.Items.Add(Globals.supportedLanguagesOcr[nItem]);
            }

            pckLanguageOcr.SelectedIndex = Globals.nLanguageOcrIndex;
        }

        /// <summary>
        /// Picker speech language clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerLanguageSpeechChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.cLanguageSpeech = picker.Items[selectedIndex];
            }
        }

        /// <summary>
        /// Picker OCR language clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerLanguageOcrChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.nLanguageOcrIndex = selectedIndex;
                Globals.cLanguageOcr = Globals.supportedLanguagesOcr[selectedIndex];
            }

            if (selectedIndex == 0)
            {
                Globals.cLanguageOcr = "";
            }

            Debug.WriteLine("Settings OnPickerLanguageOcrChanged: LanguageOcr: " + Globals.cLanguageOcr);  // For testing
        }

        /// <summary>
        /// Picker theme clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPickerThemeChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                Globals.cTheme = selectedIndex switch
                {
                    1 => "Light",       // Light
                    2 => "Dark",        // Dark
                    _ => "System",      // System
                };

                // Set the theme
                Globals.SetTheme();
            }
        }

        /// <summary>
        /// Button save settings clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSettingsSaveClicked(object sender, EventArgs e)
        {
            Preferences.Default.Set("SettingTheme", Globals.cTheme);
            Preferences.Default.Set("SettingLanguage", Globals.cLanguage);
            Preferences.Default.Set("SettingLanguageSpeech", Globals.cLanguageSpeech);
            Preferences.Default.Set("SettingLanguageOcrIndex", Globals.nLanguageOcrIndex);

            // Wait 500 milliseconds otherwise the settings are not saved in Android
            Task.Delay(500).Wait();

            // Restart the application
            //Application.Current.MainPage = new AppShell();
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }

        /// <summary>
        /// Button reset settings clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSettingsResetClicked(object sender, EventArgs e)
        {
            // Clear the list with the OCR languages
            Globals.supportedLanguagesOcr.Clear();

            // Get the elapsed time in milli seconds
            stopWatch.Stop();

            if (stopWatch.ElapsedMilliseconds < 2001)
            {
                // Clear all settings after the first clicked event within the first 2 seconds after opening the setting page
                Preferences.Default.Clear();
            }
            else
            {
                // Reset some settings
                Preferences.Default.Remove("SettingTheme");
                Preferences.Default.Remove("SettingLanguage");
                Preferences.Default.Remove("SettingLanguageSpeech");
                Preferences.Default.Remove("SettingLanguageOcrIndex");
            }

            // Wait 500 milliseconds otherwise the settings are not saved in Android.
            Task.Delay(500).Wait();

            // Restart the application
            //Application.Current.MainPage = new AppShell();
            Application.Current.MainPage = new NavigationPage(new MainPage());
        }
    }
}