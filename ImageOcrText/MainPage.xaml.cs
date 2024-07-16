/* Program .....: ImageOcrText.sln
 * Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
 * Copyright ...: (C) 2024-2024
 * Version .....: 1.0.7
 * Date ........: 2024-07-16 (YYYY-MM-DD)
 * Language ....: Microsoft Visual Studio 2022: .NET MAUI 8 - C# 12.0
 * Description .: Convert text from an image or picture to raw text via OCR
 * Note ........: 
 * Dependencies : NuGet Package: Plugin.Maui.OCR Version 1.0.12 - by kfrancis - https://github.com/kfrancis/ocr
 *                (NuGet Package: Xamarin.AndroidX.Fragment.Ktx - Version 1.7.0.2)
 * Thanks to ...: Gerald Versluis for his video's on YouTube about .NET MAUI
 *                https://www.youtube.com/watch?v=alY_6Qn0_60 */

using Plugin.Maui.OCR;

namespace ImageOcrText
{
    public sealed partial class MainPage : ContentPage
    {
        //// Local variables
        private string cLicense = "";

        public MainPage()
        {
            try
            {
                InitializeComponent();
#if IOS
                //// Workaround for the !!!BUG!!! in iOS from Maui 8.0.21+?
                //// Word wrap in editor is not working when going from landscape to portrait
                //// Vertical scrollbar is set to horizontal scrollbar when going from landscape to portrait
                DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged!;
#endif
            }
            catch (Exception ex)
            {
                //SentrySdk.CaptureException(ex);
#if DEBUG
                DisplayAlert("InitializeComponent: MainPage", ex.Message, "OK");
#endif
                return;
            }

            //// Get the saved settings
            Globals.cTheme = Preferences.Default.Get("SettingTheme", "System");
            Globals.cLanguage = Preferences.Default.Get("SettingLanguage", "");
            Globals.cLanguageSpeech = Preferences.Default.Get("SettingLanguageSpeech", "");
            Globals.nLanguageOcrIndex = Preferences.Default.Get("SettingLanguageOcrIndex", 0);
            Globals.bLicense = Preferences.Default.Get("SettingLicense", false);

            //// The height of the title bar is lower when an iPhone is in horizontal position
            if (DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                imgbtnAbout.VerticalOptions = LayoutOptions.Start;
                lblTitle.VerticalOptions = LayoutOptions.Start;
                lblTitle.VerticalTextAlignment = TextAlignment.Start;
                imgbtnSettings.VerticalOptions = LayoutOptions.Start;
            }

            //// Set the info button to the Center position in the title bar for iOS
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                imgbtnAbout.HorizontalOptions = LayoutOptions.Center;

                // !!!BUG!!! in iOS from Maui 8.0.21+?
                // The width of the editor has to be set otherwise the editor is a vertical line
                if (DeviceInfo.Idiom == DeviceIdiom.Phone)
                {
                    edtOcrResult.MinimumWidthRequest = 320;
                }
                else
                {
                    edtOcrResult.MinimumWidthRequest = 700;
                }
            }

            //// !!!BUG!!! in Windows - The vertical allignment of the language labels is wrong in WinUI
            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                lblLanguageOcr.Padding = new Thickness(0, 10, 0, 0);
                lblTextToSpeech.Padding = new Thickness(0, 10, 0, 0);
            }

            //// Set the theme
            Globals.SetTheme();

            //// Get and set the system OS user language
            try
            {
                if (string.IsNullOrEmpty(Globals.cLanguage))
                {
                    Globals.cLanguage = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                }
            }
            catch (Exception)
            {
                Globals.cLanguage = "en";
            }

            //// Set the text language
            SetTextLanguage();

            //// Initialize text to speech and get and set the speech language
            string cCultureName = "";

            try
            {
                if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
                {
                    cCultureName = Thread.CurrentThread.CurrentCulture.Name;
                }
            }
            catch (Exception)
            {
                cCultureName = "en-US";
            }

            //// Initialize text to speech
            InitializeTextToSpeech(cCultureName);

            //// Set the language for the OCR plugin to 'All supported languages', necessary after a reset of the application
            Globals.cLanguageOcr = "";

            //// Clear the clipboard
            //Clipboard.Default.SetTextAsync(null);  // For testing

            //// Set focus to the editor
            edtOcrResult.Focus();

            //// Test for crashes Sentry
            //SentrySdk.CaptureMessage("Hello Sentry");
            //throw new Exception("This is a test exception");
        }

#if IOS
        /// <summary>
        /// Workaround for the !!!BUG!!! in iOS from Maui 8.0.21+?
        /// Word wrap in editor is not working when going from landscape to portrait
        /// Vertical scrollbar is set to horizontal scrollbar when going from landscape to portrait
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnMainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            DisplayOrientation orientation = e.DisplayInfo.Orientation;

            switch (orientation)
            {
                case DisplayOrientation.Portrait:
                    // Handle logic for portrait orientation
                    string cOcrResult = edtOcrResult.Text;
                    edtOcrResult.Text = "";
                    await Task.Delay(100);
                    edtOcrResult.Text = cOcrResult;
                    Debug.WriteLine("Portrait");
                    break;
                case DisplayOrientation.Landscape:
                    // Handle logic for landscape orientation
                    Debug.WriteLine("Landscape");
                    break;
            }
        }
#endif
        /// <summary>
        /// Initialize the OCR plugin using the Appearing event of the MainPage.xaml
        /// </summary>
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // Initialize the OCR plugin
            await OcrPlugin.Default.InitAsync();
#if !ANDROID
            // Error 'The method or operation is not implemented' when trying to get the supported languages for OCR in Android
            
            // Initialize supported languages OCR
            if (Globals.supportedLanguagesOcr.Count == 0)
            {
                InitializeSupportedLanguagesOcr();
            }

            // Set the language for the OCR plugin
            if (Globals.nLanguageOcrIndex > Globals.supportedLanguagesOcr.Count)
            {
                Globals.nLanguageOcrIndex = 0;
            }

            if (Globals.nLanguageOcrIndex > 0 && Globals.nLanguageOcrIndex <= Globals.supportedLanguagesOcr.Count)
            {
                Globals.cLanguageOcr = Globals.supportedLanguagesOcr[Globals.nLanguageOcrIndex];
            }
#endif
            // Insert the item 'All supported languages' at the beginning if the list is empty
            if (Globals.supportedLanguagesOcr.Count == 0)
            {
                Globals.supportedLanguagesOcr.Add(OcrLang.LanguageOcrAll_Text);
                Globals.nLanguageOcrIndex = 0;
                Globals.cLanguageOcr = "";
            }

            lblLanguageOcr.Text = Globals.cLanguageOcr;
        }

        /// <summary>
        /// Initialize supported OCR languages for the OCR plugin
        /// </summary>
        private async void InitializeSupportedLanguagesOcr()
        {
            try
            {
                // Get the supported languages for the OCR plugin
                Globals.supportedLanguagesOcr = OcrPlugin.Default.SupportedLanguages.ToList();

                // Sort the list of supported languages
                Globals.supportedLanguagesOcr.Sort();

                if (Globals.supportedLanguagesOcr.Count > 0)
                {
                    // Insert the item 'All supported languages' at the beginning
                    Globals.supportedLanguagesOcr.Insert(0, OcrLang.LanguageOcrAll_Text);
#if DEBUG
                    // For testing
                    foreach (string language in Globals.supportedLanguagesOcr)
                    {
                        Debug.WriteLine(language);
                    }
#endif
                }
                else
                {
#if DEBUG
                    await DisplayAlert(OcrLang.ErrorTitle_Text, OcrLang.LanguageOcrError_Text, OcrLang.ButtonClose_Text);
#endif
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await DisplayAlert("Error", ex.Message, "OK");
#endif
            }
        }

        /// <summary>
        /// Set text and speech language using the Appearing event of the MainPage.xaml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPageAppearing(object sender, EventArgs e)
        {
            // Set the text language
            if (Globals.bLanguageChanged)
            {
                SetTextLanguage();
                Globals.bLanguageChanged = false;
            }

            // Set the speech language
            lblTextToSpeech.Text = Globals.GetIsoLanguageCode();
        }

        /// <summary>
        /// Show license using the Loaded event of the MainPage.xaml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageLoaded(object sender, EventArgs e)
        {
            // Show license
            if (Globals.bLicense == false)
            {
                Globals.bLicense = await DisplayAlert(OcrLang.LicenseTitle_Text, cLicense, OcrLang.Agree_Text, OcrLang.Disagree_Text);

                if (Globals.bLicense)
                {
                    Preferences.Default.Set("SettingLicense", true);
                }
                else
                {
#if IOS
                    //Thread.CurrentThread.Abort();  // Not allowed in iOS
                    imgbtnAbout.IsEnabled = false;
                    imgbtnSettings.IsEnabled = false;
                    imgbtnPickImage.IsEnabled = false;
                    imgbtnTextToSpeech.IsEnabled = false;
                    imgbtnTakePicture.IsEnabled = false;
                    imgbtnCopyToClipboard.IsEnabled = false;
                    imgbtnShare.IsEnabled = false;
                    imgbtnClear.IsEnabled = false;

                    await DisplayAlert(OcrLang.LicenseTitle_Text, OcrLang.CloseApplication_Text, OcrLang.ButtonClose_Text);
#else
                    Application.Current?.Quit();
#endif
                }
            }
        }

        //// TitleView buttons clicked events
        private async void OnPageAboutClicked(object sender, EventArgs e)
        {
            imgbtnTextToSpeech.Source = Globals.CancelTextToSpeech();
            await Navigation.PushAsync(new PageInfo());
        }

        private async void OnPageSettingsClicked(object sender, EventArgs e)
        {
            imgbtnTextToSpeech.Source = Globals.CancelTextToSpeech();
            await Navigation.PushAsync(new PageSettings());
        }

        /// <summary>
        /// Click event: Pick an image from the gallery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            activityIndicator.IsRunning = true;

            imgbtnTextToSpeech.Source = Globals.CancelTextToSpeech();

            Debug.WriteLine("Mainpage OnPickImageClicked: " + Globals.cLanguageOcr);  // For testing

            try
            {
                FileResult? pickResult = await MediaPicker.Default.PickPhotoAsync();

                OcrOptions options = new OcrOptions.Builder()
                .SetLanguage(Globals.cLanguageOcr)
                .SetTryHard(true)
                .Build();

                if (pickResult != null)
                {
                    using Stream imageAsStream = await pickResult.OpenReadAsync();
                    byte[] imageAsBytes = new byte[imageAsStream.Length];
                    _ = await imageAsStream.ReadAsync(imageAsBytes);

                    OcrResult ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageAsBytes, options);

                    if (!ocrResult.Success)
                    {
                        await DisplayAlert(OcrLang.ErrorTitle_Text, OcrLang.ImageToTextError_Text, OcrLang.ButtonClose_Text);
                        return;
                    }

                    edtOcrResult.Text = ocrResult.AllText;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

            activityIndicator.IsRunning = false;
        }

        /// <summary>
        /// Click event: Take a picture with the camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTakePictureClicked(object sender, EventArgs e)
        {
            activityIndicator.IsRunning = true;

            imgbtnTextToSpeech.Source = Globals.CancelTextToSpeech();

            try
            {
                FileResult? pickResult = await MediaPicker.Default.CapturePhotoAsync();

                OcrOptions options = new OcrOptions.Builder()
                .SetLanguage(Globals.cLanguageOcr)
                .SetTryHard(true)
                .Build();

                if (pickResult != null)
                {
                    using Stream imageAsStream = await pickResult.OpenReadAsync();
                    byte[] imageAsBytes = new byte[imageAsStream.Length];
                    await imageAsStream.ReadAsync(imageAsBytes);

                    OcrResult ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageAsBytes, options);

                    if (!ocrResult.Success)
                    {
                        await DisplayAlert(OcrLang.ErrorTitle_Text, OcrLang.ImageToTextError_Text, OcrLang.ButtonClose_Text);
                        return;
                    }

                    edtOcrResult.Text = ocrResult.AllText;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

            activityIndicator.IsRunning = false;
        }

        /// <summary>
        /// Click event: Copy the text to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCopyToClipboardClicked(object sender, EventArgs e)
        {
            try
            {
                if (edtOcrResult.Text != "")
                {
                    await Clipboard.Default.SetTextAsync(edtOcrResult.Text);
                }
            }
            catch (Exception ex)
            {
                //SentrySdk.CaptureException(ex);
#if DEBUG
                await Application.Current!.MainPage!.DisplayAlert(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
#endif
            }
        }

        /// <summary>
        /// Click event: Share the text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnShareClicked(object sender, EventArgs e)
        {
            if (edtOcrResult.Text != "")
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = edtOcrResult.Text,
                    Title = OcrLang.NameProgram_Text
                });
            }
        }

        /// <summary>
        /// Click event: Clear the text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClearClicked(object sender, EventArgs e)
        {
            imgbtnTextToSpeech.Source = Globals.CancelTextToSpeech();
            edtOcrResult.Text = "";
        }

        /// <summary>
        /// Put text in the chosen language in the controls
        /// </summary>
        private void SetTextLanguage()
        {
            // Set the current UI culture of the selected language
            Globals.SetCultureSelectedLanguage();

            cLicense = $"{OcrLang.License_Text}\n\n{OcrLang.LicenseMit2_Text}";
        }

        /// <summary>
        /// Initialize text to speech and fill the the array with the speech languages
        /// </summary>
        /// <param name="cCultureName"></param>
        private async void InitializeTextToSpeech(string cCultureName)
        {
            // .Country = KR ; .Id = ''  ; .Language = ko ; .Name = Korean (South Korea) ;

            // Initialize text to speech
            int nTotalItems;

            try
            {
                Globals.locales = await TextToSpeech.Default.GetLocalesAsync();

                nTotalItems = Globals.locales.Count();

                if (nTotalItems == 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                //SentrySdk.CaptureException(ex);
#if DEBUG
                await DisplayAlert(OcrLang.ErrorTitle_Text, $"{ex.Message}\n\n{OcrLang.TextToSpeechError_Text}", OcrLang.ButtonClose_Text);
#endif
                return;
            }

            lblTextToSpeech.IsVisible = true;
            imgbtnTextToSpeech.IsVisible = true;
            Globals.bLanguageLocalesExist = true;

            // Put the locales in the array and sort the array
            Globals.cLanguageLocales = new string[nTotalItems];
            int nItem = 0;

            foreach (Locale l in Globals.locales)
            {
                Globals.cLanguageLocales[nItem] = $"{l.Language}-{l.Country} {l.Name}";
                nItem++;
            }

            Array.Sort(Globals.cLanguageLocales);

            // Search for the language after a first start or reset of the application
            if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
            {
                SearchArrayWithSpeechLanguages(cCultureName);
            }

            lblTextToSpeech.Text = Globals.GetIsoLanguageCode();
        }

        /// <summary>
        /// Search for the language after a first start or reset of the application
        /// </summary>
        /// <param name="cCultureName"></param>
        private static void SearchArrayWithSpeechLanguages(string cCultureName)
        {
            try
            {
                if (Globals.cLanguageLocales is null)
                {
                    Globals.cLanguageSpeech = cCultureName;
                    return;
                }

                int nTotalItems = Globals.cLanguageLocales.Length;

                for (int nItem = 0; nItem < nTotalItems; nItem++)
                {
                    if (Globals.cLanguageLocales[nItem].StartsWith(cCultureName))
                    {
                        Globals.cLanguageSpeech = Globals.cLanguageLocales[nItem];
                        break;
                    }
                }

                // If the language is not found try it with the language (Globals.cLanguage) of the user setting for this app
                if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
                {
                    for (int nItem = 0; nItem < nTotalItems; nItem++)
                    {
                        if (Globals.cLanguageLocales[nItem].StartsWith(Globals.cLanguage))
                        {
                            Globals.cLanguageSpeech = Globals.cLanguageLocales[nItem];
                            break;
                        }
                    }
                }

                // If the language is still not found use the first language in the array
                if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
                {
                    Globals.cLanguageSpeech = Globals.cLanguageLocales[0];
                }
            }
            catch (Exception ex)
            {
                //SentrySdk.CaptureException(ex);
#if DEBUG
                Application.Current!.MainPage!.DisplayAlert(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
#endif
            }
        }

        /// <summary>
        /// Button text to speech event - Convert text to speech
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextToSpeechClicked(object sender, EventArgs e)
        {
            // Cancel the text to speech
            if (Globals.bTextToSpeechIsBusy)
            {
                imgbtnTextToSpeech.Source = Globals.CancelTextToSpeech();
                return;
            }

            // Convert the text to speech
            _ = Globals.ConvertTextToSpeechAsync(imgbtnTextToSpeech, edtOcrResult.Text);
        }
    }
}
