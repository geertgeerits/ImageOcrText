namespace ImageOcrText
{
    internal sealed class ClassSpeech
    {
        private static string[]? cLanguageLocales;
        private static IEnumerable<Locale>? locales;
        private static CancellationTokenSource? cts;
        
        /// <summary>
        /// Initialize text to speech and fill the the array with the speech languages
        /// Android: .Language = ko- .Country = KR  .Name = Korean (South Korea) ! .Id = ko-kr-x-ism-local
        /// iOS:     .Language = ko- .Country = KR- .Name = Yuna ! .Id = com.apple.voice.compact.ko-KR.Yuna
        /// Windows: .Language = ko- .Country = KR- .Name = Microsoft David ! .Id = HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_enUS_DavidM
        /// </summary>
        public static async Task<bool> InitializeTextToSpeechAsync()
        {
            try
            {
                // Initialize text to speech
                locales = await TextToSpeech.Default.GetLocalesAsync();
                int nTotalItems = locales.Count();
                Debug.WriteLine($"Number of locales retrieved: {nTotalItems}");

                if (nTotalItems == 0)
                {
                    Debug.WriteLine("No locales found. Text-to-speech may not be supported on this device.");
                    return false;
                }

                // Populate the locales
                cLanguageLocales = new string[nTotalItems];
                int nItem = 0;
#if WINDOWS
                foreach (var l in locales)
                {
                    cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name} ! {l.Id[(l.Id.LastIndexOf('\\') + 1)..]}";
                    nItem++;
                    //Debug.WriteLine($"locales: {l.Language}-{l.Country} {l.Name} ! {l.Id[(l.Id.LastIndexOf('\\') + 1)..]}");
                }
#else
                foreach (var l in locales)
                {
                    cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name} ! {l.Id}";
                    nItem++;
                    //Debug.WriteLine($"locales: {l.Language}-{l.Country} {l.Name} ! {l.Id}");
                }
#endif
                // Sort the locales
                Array.Sort(cLanguageLocales);

                return true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine($"Error in InitializeTextToSpeechAsync: {ex.Message}");
                await Application.Current!.Windows[0].Page!.DisplayAlertAsync(OcrLang.ErrorTitle_Text, $"{ex.Message}\n\n{OcrLang.TextToSpeechError_Text}", OcrLang.ButtonClose_Text);
#endif
                return false;
            }
        }

        /// <summary>
        /// Fill the picker with the speech languages
        /// </summary>
        /// <param name="picker"></param>
        public static void FillPickerWithSpeechLanguages(Picker picker)
        {
            // If there are no locales, disable the picker and return
            if (cLanguageLocales is null)
            {
                picker.IsEnabled = false;
                return;
            }

            // Populate the picker with the Language, Country and Name (without the Id) from the sorted locales array
            for (int nItem = 0; nItem < cLanguageLocales.Length; nItem++)
            {
                picker.Items.Add(cLanguageLocales[nItem].Split(" ! ")[0]);
            }

            // Select the saved language
            picker.SelectedIndex = SearchArrayWithSpeechLanguages(Globals.cLanguageSpeech);

            Debug.WriteLine("FillPickerWithSpeechLanguages - Globals.cLanguageSpeech: " + Globals.cLanguageSpeech);
        }

        /// <summary>
        /// Picker speech language clicked event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void PickerLanguageSpeechChanged(object sender, EventArgs e)
        {
            Picker picker = (Picker)sender;
            int selectedIndex = picker.SelectedIndex;

            if (selectedIndex != -1)
            {
                if (cLanguageLocales != null && selectedIndex < cLanguageLocales.Length)
                {
                    Globals.cLanguageSpeech = cLanguageLocales[selectedIndex];
                }
            }
        }

        /// <summary>
        /// Search the selected language in the cLanguageLocales array
        /// </summary>
        /// <param name="cCultureName"></param>
        public static int SearchArrayWithSpeechLanguages(string cCultureName)
        {
            Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName IN: " + cCultureName);

            try
            {
                int nTotalItems = cLanguageLocales?.Length ?? 0;

                if (cLanguageLocales is not null)
                {
                    if (!string.IsNullOrEmpty(cCultureName))
                    {
                        // Search for the Indonesian or Hebrew or Yiddish language code, if not found search for the old language code
                        // Android generating old/wrong language codes - https://stackoverflow.com/questions/44245959/android-generating-wrong-language-code-for-indonesia
                        if (cCultureName.StartsWith("id") || cCultureName.StartsWith("he") || cCultureName.StartsWith("yi"))
                        {
                            for (int nItem = 0; nItem < nTotalItems; nItem++)
                            {
                                if (cLanguageLocales[nItem].StartsWith(cCultureName))
                                {
                                    Globals.cLanguageSpeech = cLanguageLocales[nItem];
                                    return nItem;
                                }
                            }

                            // Map new language codes to old codes
                            cCultureName = GetCurrentLanguageTag(cCultureName);
                            Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName NEW to OLD: " + cCultureName);
                        }

                        // Search for the speech language as 'en-US ! Microsoft David'
                        for (int nItem = 0; nItem < nTotalItems; nItem++)
                        {
                            if (cLanguageLocales[nItem] == (cCultureName))
                            {
                                Globals.cLanguageSpeech = cLanguageLocales[nItem];
                                return nItem;
                            }
                        }

                        // Search for the speech language as 'en-US'
                        for (int nItem = 0; nItem < nTotalItems; nItem++)
                        {
                            if (cLanguageLocales[nItem].StartsWith(cCultureName))
                            {
                                Globals.cLanguageSpeech = cLanguageLocales[nItem];
                                return nItem;
                            }
                        }

                        // Select the characters before the first hyphen if there is a hyphen in the string
                        cCultureName = cCultureName.Split('-')[0];

                        // Search for the speech language as 'en'
                        for (int nItem = 0; nTotalItems > nItem; nItem++)
                        {
                            if (cLanguageLocales[nItem].StartsWith(cCultureName))
                            {
                                Globals.cLanguageSpeech = cLanguageLocales[nItem];
                                return nItem;
                            }
                        }
                    }
                }

                // If the language is not found use the first language in the array
                if (string.IsNullOrEmpty(Globals.cLanguageSpeech) && nTotalItems > 0)
                {
                    Globals.cLanguageSpeech = cLanguageLocales![0];
                    return 0;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Application.Current!.Windows[0].Page!.DisplayAlertAsync(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
#endif
            }

            return 0;
        }

        /// <summary>
        /// Map new language codes to old codes
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentLanguageTag(string languageTag)
        {
            // Map new language codes to old ones
            return languageTag switch
            {
                "id" => "in",           // Indonesian - Changed in 1989 to 'id'
                "id-ID" => "in-ID",
                "he" => "iw",           // Hebrew - Changed in 1989 to 'he'
                "he-IL" => "iw-IL",
                "yi" => "ji",           // Yiddish - Changed in 1989 to 'yi'
                _ => languageTag
            };
        }

        /// <summary>
        /// Convert text to speech
        /// </summary>
        /// <param name="cText"></param>
        /// <returns></returns>
        public static async Task ConvertTextToSpeechAsync(object sender, string cText)
        {
            /* If you do not wait long enough to press the arrow key in the Task 'MakeExplainTurnAsync()',
               an error message will sometimes appear: 'The operation was canceled'.
               This only occurs if the 'Explained by speech' setting is enabled.
               The error occurs in the method 'ConvertTextToSpeechAsync()'. */

            // Cancel the text to speech
            if (Globals.bTextToSpeechIsBusy)
            {
                if (cts?.IsCancellationRequested ?? true)
                {
                    return;
                }

                cts.Cancel();
            }

            var imageButton = (ImageButton)sender;

            // Start with the text to speech
            Debug.WriteLine("ConvertTextToSpeechAsync + cText: " + cText);
            Debug.WriteLine("ConvertTextToSpeechAsync + Globals.cLanguageSpeech: " + Globals.cLanguageSpeech);

            if (!string.IsNullOrEmpty(cText))
            {
                Globals.bTextToSpeechIsBusy = true;
                imageButton.Source = Globals.cImageTextToSpeechCancel;

                try
                {
                    cts = new CancellationTokenSource();
#if WINDOWS
                    SpeechOptions options = new()
                    {
                        Locale = locales?.FirstOrDefault(static l => $"{l.Language}-{l.Country} {l.Name} ! {l.Id[(l.Id.LastIndexOf('\\') + 1)..]}" == Globals.cLanguageSpeech)
                    };
#else
                    SpeechOptions options = new()
                    {
                        Locale = locales?.FirstOrDefault(static l => $"{l.Language}-{l.Country} {l.Name} ! {l.Id}" == Globals.cLanguageSpeech)
                    };
#endif
                    await TextToSpeech.Default.SpeakAsync(cText, options, cancelToken: cts.Token);
                    Globals.bTextToSpeechIsBusy = false;
                }
                catch (Exception ex)
                {
#if DEBUG
                    await Application.Current!.Windows[0].Page!.DisplayAlertAsync("ConvertTextToSpeechAsync", $"{ex.Message}\n{ex.StackTrace}", OcrLang.ButtonClose_Text);
                    Debug.WriteLine($"Method ConvertTextToSpeechAsync:\n{ex.Message}\n{ex.StackTrace}");
#endif
                }

                imageButton.Source = Globals.cImageTextToSpeech;
            }
        }

        /// <summary>
        /// Cancel speech if a cancellation token exists and hasn't been already requested
        /// </summary>
        public static string CancelTextToSpeech()
        {
            if (Globals.bTextToSpeechIsBusy)
            {
                if (cts?.IsCancellationRequested ?? true)
                {
                    return Globals.cImageTextToSpeechCancel;
                }

                cts.Cancel();
                Globals.bTextToSpeechIsBusy = false;
            }

            return Globals.cImageTextToSpeech;
        }
    }
}
