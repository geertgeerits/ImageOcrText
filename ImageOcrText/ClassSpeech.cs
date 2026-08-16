namespace ImageOcrText
{
    internal sealed class ClassSpeech
    {
        private static string[]? cLanguageLocales;                  // Array to hold the speech languages (Language-Country Name : Id)
        private static IEnumerable<Locale>? locales;                // Collection of available locales for text-to-speech
        private static CancellationTokenSource? cts;                // CancellationTokenSource for managing cancellation of text-to-speech operations
        private static bool bTextToSpeechLanguageSelected;          // Flag to indicate if a text-to-speech language has been selected

        /// <summary>
        /// Initialize text to speech and fill the the array with the speech languages ( : is separator before the Id)
        /// Android: .Language = ko- .Country = KR  .Name = Korean (South Korea) : .Id = ko-kr-x-ism-local
        /// iOS:     .Language = ko- .Country = KR- .Name = Yuna : .Id = com.apple.voice.compact.ko-KR.Yuna
        /// Windows: .Language = ko- .Country = KR- .Name = Microsoft David : .Id = HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_enUS_DavidM
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

                // Populate the array with the selected speech languages (Language-Country Name : Id)
                // Create an array to hold the speech languages (Language-Country Name : Id)
                cLanguageLocales = new string[nTotalItems];
                int nItem = 0;

                // Define the allowed language to filter the locales
                // Get the primary language code from the UI language (e.g., "en" from "en-US")
                string allowedLanguages = Globals.cLanguage.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)[0];

#if ANDROID
                // Populate the locales with the Id for Android because the Id is needed to select the correct voice for text-to-speech
                // starting with Samsung S25 ???
                /*
                Id: en-us-x-tpf-local
                Language: en
                Country: US
                Name: English (United States)
                Display: en-US English (United States)
                */
                foreach (Locale l in locales)
                {
                    if (allowedLanguages.Contains(l.Language))
                    {
                        cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name} : {l.Id}";
                        nItem++;
                    }
                }

#elif IOS
                // l.Language can be "en" or "en-US" (or "en_US") so we use the l.Country anyway if in the future the l.Country is needed for the voice selection
                // Exclude the voices that contain 'synthesis.voice' in the Id because they are not real voices and stupid
                /*
                Id: com.apple.eloquence.en-US.Eddy
                Language: en-US
                Country: 
                Name: Eddy
                Display: en-US- Eddy : com.apple.eloquence.en-US.Eddy
                */
                foreach (Locale l in locales)
                {
                    string lang = l.Language ?? string.Empty;
                    string primary = lang.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)[0];

                    if ((allowedLanguages.Contains(lang) || allowedLanguages.Contains(primary)) && !l.Id.Contains("synthesis.voice"))
                    {
                        cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name}";
                        nItem++;
                    }
                }

#else           // Windows and other platforms
                // l.Language can be "en" or "en-US" (or "en_US") so we use the l.Country anyway if in the future the l.Country is needed for the voice selection
                /*
                Id: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_enUS_DavidM
                Language: en-US
                Country: 
                Name: Microsoft David
                Display: en-US- Microsoft David : HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens\MSTTS_V110_enUS_DavidM
                */
                foreach (Locale l in locales)
                {
                    // l.Language can be "en" or "en-US" (or "en_US")
                    string lang = l.Language ?? string.Empty;
                    string primary = lang.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)[0];

                    if (allowedLanguages.Contains(lang) || allowedLanguages.Contains(primary))
                    {
                        cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name}";
                        nItem++;
                    }
                }
#endif
                // Shrink the array to the number of items actually written (handles iOS branch skipping entries)
                if (nItem < cLanguageLocales.Length)
                {
                    Array.Resize(ref cLanguageLocales, nItem);
                }

                // Sort the locales
                Array.Sort(cLanguageLocales);

                //foreach (string item in cLanguageLocales)
                //{
                //    Debug.WriteLine($"Sorted locales: {item}");
                //}

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
            // Initialize text to speech and fill the array with the selected speech languages
            _ = InitializeTextToSpeechAsync();

            // If there are no locales, disable the picker and return
            if (cLanguageLocales is null)
            {
                picker.IsEnabled = false;
                bTextToSpeechLanguageSelected = false;
                return;
            }

            // Clear the picker items
            picker.Items.Clear();

            // Populate the picker with the Language, Country, Name, (Id) from the sorted locales array
            foreach (string locale in cLanguageLocales)
            {
                picker.Items.Add(locale);
            }

            // If there are no languages in the picker, disable the picker and return
            if (picker.Items.Count == 0)
            {
                picker.IsEnabled = false;
                bTextToSpeechLanguageSelected = false;

                // Show a popup message to the user
                Application.Current!.Windows[0].Page!.DisplayAlertAsync("", OcrLang.TextToSpeechError_Text, OcrLang.ButtonClose_Text);

                return;
            }
            else
            {
                picker.IsEnabled = true;
                bTextToSpeechLanguageSelected = true;
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

            if (picker.SelectedIndex != -1)
            {
                if (cLanguageLocales != null && picker.SelectedIndex < cLanguageLocales.Length)
                {
                    Globals.cLanguageSpeech = cLanguageLocales[picker.SelectedIndex];
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
                int index;

                if (cLanguageLocales is not null)
                {
                    if (!string.IsNullOrEmpty(cCultureName))
                    {
                        // Search for the Indonesian or Hebrew or Yiddish language code, if not found search for the old language code
                        // Android generating old/wrong language codes - https://stackoverflow.com/questions/44245959/android-generating-wrong-language-code-for-indonesia
                        if (cCultureName.StartsWith("id") || cCultureName.StartsWith("he") || cCultureName.StartsWith("yi"))
                        {
                            index = Array.FindIndex(cLanguageLocales, s => s.StartsWith(cCultureName, StringComparison.Ordinal));
                            if (index >= 0)
                            {
                                Globals.cLanguageSpeech = cLanguageLocales[index];
                                return index;
                            }
                            Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName OLD found: " + cCultureName);

                            // Map new language codes to old codes
                            cCultureName = GetCurrentLanguageTag(cCultureName);
                            Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName NEW to OLD: " + cCultureName);
                        }

                        // Search for the speech language as 'en-US : Microsoft David'
                        index = Array.BinarySearch(cLanguageLocales, cCultureName, StringComparer.Ordinal);
                        if (index >= 0)
                        {
                            Globals.cLanguageSpeech = cLanguageLocales[index];
                            return index;
                        }
                        //Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName 'FULL' not found: " + cCultureName);

                        // Search for the speech language as 'en-US'
                        index = Array.FindIndex(cLanguageLocales, s => s.StartsWith(cCultureName, StringComparison.Ordinal));
                        if (index >= 0)
                        {
                            Globals.cLanguageSpeech = cLanguageLocales[index];
                            return index;
                        }
                        //Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName 'en-US' not found: " + cCultureName);

                        // Select the characters before the first hyphen if there is a hyphen in the string
                        cCultureName = cCultureName.Split('-')[0];

                        // Search for the speech language as 'en'
                        index = Array.FindIndex(cLanguageLocales, s => s.StartsWith(cCultureName, StringComparison.Ordinal));
                        if (index >= 0)
                        {
                            Globals.cLanguageSpeech = cLanguageLocales[index];
                            return index;
                        }
                        //Debug.WriteLine("SearchArrayWithSpeechLanguages - cCultureName 'en' found: " + cCultureName);
                    }
                }

                // If the language is not found use the first language in the array
                if (cLanguageLocales?.Length > 0)
                {
                    // Select the first language in the array
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
            /* If there is too rapid switching between starting and stopping speech,
               an error message will sometimes appear: 'The operation was canceled'.
               This only occurs if 'text to speech' is available and busy.
               The error occurs in this method. */

            // Cancel the text to speech
            if (Globals.bTextToSpeechIsBusy)
            {
                if (cts != null && !cts.IsCancellationRequested)
                {
                    // Cancel outstanding speech and give it a short moment to settle
                    cts.Cancel();
                    await Task.Delay(100);
                }
                else
                {
                    // No cancellable token present — clear busy flag so we can proceed
                    Globals.bTextToSpeechIsBusy = false;
                }
            }

            ImageButton imageButton = (ImageButton)sender;

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

                    SpeechOptions options = new()
                    {
#if ANDROID
                        Locale = locales?.FirstOrDefault(static l => $"{l.Language}-{l.Country} {l.Name} : {l.Id}" == Globals.cLanguageSpeech)
#else
                        Locale = locales?.FirstOrDefault(static l => $"{l.Language}-{l.Country} {l.Name}" == Globals.cLanguageSpeech)
#endif
                    };

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
                if (cts != null && !cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }

                Globals.bTextToSpeechIsBusy = false;
            }

            return Globals.cImageTextToSpeech;
        }
    }}
