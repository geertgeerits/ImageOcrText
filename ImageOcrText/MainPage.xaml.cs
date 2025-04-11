/* Program .....: ImageOcrText.sln
 * Author ......: Geert Geerits - E-mail: geertgeerits@gmail.com
 * Copyright ...: (C) 2024-2025
 * Version .....: 1.0.8
 * Date ........: 2025-04-11 (YYYY-MM-DD)
 * Language ....: Microsoft Visual Studio 2022: .NET MAUI 9 - C# 13.0
 * Description .: Convert text from an image or picture to raw text via OCR
 * Note ........: Only portrait mode is supported for iOS (!!!BUG!!! problems with the editor in iOS when turning from landscape to portrait)
 * Dependencies : NuGet Package: Plugin.Maui.OCR Version 1.0.15 - by kfrancis - https://github.com/kfrancis/ocr
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
            }
            catch (Exception ex)
            {
#if DEBUG
                DisplayAlert("InitializeComponent: MainPage", ex.Message, "OK");
#endif
                return;
            }
#if WINDOWS
            //// Set the margins for the controls in the title bar for Windows
            imgbtnAbout.Margin = new Thickness(20, 0, 0, 0);
            lblTitle.Margin = new Thickness(20, 10, 0, 0);
#endif
#if IOS
            //// AutoSize has to be disabled for iOS
            edtOcrResult.AutoSize = EditorAutoSizeOption.Disabled;

            //// Set the scale of the activity indicator for iOS
            activityIndicator.Scale = 2;

            //// Workaround for the !!!BUG!!! in iOS
            //// VerticalOptions in editor is not working when going from portrait to landscape
            //DeviceDisplay.MainDisplayInfoChanged += OnMainDisplayInfoChanged!;

            //// Disable the default behavior of automatically scrolling the view when the keyboard appears
            //DisconnectKeyboardAutoScroll();
#endif
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

            //// Get and set the user interface language after a first start or reset of the application
            try
            {
                if (string.IsNullOrEmpty(Globals.cLanguage))
                {
                    Globals.cLanguage = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;

                    // Chinese needs the language code as zh-CN and zh-TW
                    if (Globals.cLanguage == "zh")
                    {
                        Globals.cLanguage = Thread.CurrentThread.CurrentUICulture.Name;
                    }
                }
            }
            catch (Exception)
            {
                Globals.cLanguage = "en";
            }
            finally
            {
                // Save the UI language
                Preferences.Default.Set("SettingLanguage", Globals.cLanguage);
                Debug.WriteLine("MainPage - Globals.cLanguage: " + Globals.cLanguage);
            }

            //// Set the text language
            SetTextLanguage();

            //// Initialize text to speech and get and set the speech language
            InitializeTextToSpeechAsync();

            //// Set the language for the OCR plugin to 'All supported languages', necessary after a reset of the application
            Globals.cLanguageOcr = "";

            //// Clear the clipboard
            //Clipboard.Default.SetTextAsync(null);  // For testing

            //// Set focus to the editor
            edtOcrResult.Focus();
        }

        /// <summary>
        /// Initialize text to speech and get and set the speech language
        /// Must be called in the constructor of the MainPage and not in the ClassSpeech.cs
        /// The InitializeTextToSpeechAsync method is called asynchronously after the UI components are initialized
        /// Once the asynchronous operation completes, the Globals.bTextToSpeechAvailable value is checked, and the UI is updated accordingly
        /// </summary>
        private async void InitializeTextToSpeechAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(Globals.cLanguageSpeech))
                {
                    Globals.cLanguageSpeech = Thread.CurrentThread.CurrentUICulture.Name;
                }
            }
            catch (Exception)
            {
                Globals.cLanguageSpeech = "en-US";
            }

            // Initialize text to speech
            Globals.bTextToSpeechAvailable = await ClassSpeech.InitializeTextToSpeechAsync();

            if (Globals.bTextToSpeechAvailable)
            {
                lblTextToSpeech.IsVisible = true;
                imgbtnTextToSpeech.IsVisible = true;
                lblTextToSpeech.Text = Globals.GetIsoLanguageCode();

                // Search the selected language in the cLanguageLocales array
                ClassSpeech.SearchArrayWithSpeechLanguages(Globals.cLanguageSpeech);

                // Save the speech language
                Preferences.Default.Set("SettingLanguageSpeech", Globals.cLanguageSpeech);
            }

            Debug.WriteLine("MainPage - Globals.bTextToSpeechAvailable: " + Globals.bTextToSpeechAvailable);
            Debug.WriteLine("MainPage - Globals.cLanguageSpeech: " + Globals.cLanguageSpeech);
        }

        /// <summary>
        /// Initialize the OCR plugin using the Appearing event of the MainPage.xaml
        /// </summary>
        protected async override void OnAppearing()
        {
            base.OnAppearing();
#if IOS
            // Prevent the app from rotating when the MainPage is displayed (!!!BUG!!! in iOS for the editor)
            AppDelegate.CurrentPage = "MainPage";
#endif
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
        /// // Prevent the app from rotating when the MainPage is displayed (!!!BUG!!! in iOS for the editor)
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
#if IOS
            AppDelegate.CurrentPage = string.Empty;
#endif
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
        ///  On page disappearing event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageDisappearing(object sender, EventArgs e)
        {
            // Hide the soft keyboard when the page disappears
            if (edtOcrResult.IsSoftInputShowing())
            {
                await edtOcrResult.HideSoftInputAsync(System.Threading.CancellationToken.None);
            }
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
            imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();
            await Navigation.PushAsync(new PageInfo());
        }

        private async void OnPageSettingsClicked(object sender, EventArgs e)
        {
            imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();
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

            imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();

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

            imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();

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
                    await imageAsStream.ReadExactlyAsync(imageAsBytes);

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
#if DEBUG
                await Application.Current!.Windows[0].Page!.DisplayAlert(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
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
            imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();
            edtOcrResult.Text = "";
        }

        /// <summary>
        /// Put text in the chosen language in the controls
        /// </summary>
        private void SetTextLanguage()
        {
            // Set the current UI culture of the selected language
            Globals.SetCultureSelectedLanguage(Globals.cLanguage);

            cLicense = $"{OcrLang.License_Text}\n\n{OcrLang.LicenseMit2_Text}";
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
                imgbtnTextToSpeech.Source = ClassSpeech.CancelTextToSpeech();
                return;
            }

            // Convert the text to speech
            /* CsWinRT1030 Type 'Microsoft.Maui.Controls.ImageButton' implements generic WinRT interfaces which requires generated
               code using unsafe for trimming and AOT compatibility if passed across the WinRT ABI.
               Project needs to be updated with '<AllowUnsafeBlocks>true</AllowUnsafeBlocks>'.ImageOcrText(net9.0 - windows10.0.19041.0) */
            _ = ClassSpeech.ConvertTextToSpeechAsync(imgbtnTextToSpeech, edtOcrResult.Text);
        }
    }
}
