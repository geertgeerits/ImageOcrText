namespace ImageOcrText
{
    public sealed partial class PageInfo : ContentPage
    {
    	public PageInfo()
    	{
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                DisplayAlertAsync("InitializeComponent: PageInfo", ex.Message, "OK");
                return;
            }
#if WINDOWS
            //// Set the margins for the controls in the title bar for Windows
            lblTitlePage.Margin = new Thickness(70, 5, 0, 0);
#endif
            //// Put text in the chosen language in the controls and variables
            lblVersion.Text = $"{OcrLang.Version_Text} 1.0.11";
            lblCopyright.Text = $"{OcrLang.Copyright_Text} © 2024-2026 Geert Geerits";
            lblPrivacyPolicy.Text = $"\n{OcrLang.PrivacyPolicyTitle_Text} {OcrLang.PrivacyPolicy_Text}";
            lblLicense.Text = $"\n{OcrLang.LicenseTitle_Text}: {OcrLang.License_Text}";
            lblExplanation.Text = $"\n{OcrLang.InfoExplanation_Text}";
            lblTrademarks.Text = $"\n{OcrLang.Trademarks_Text}";
        }
    }

    /// <summary>
    /// Open e-mail app and open webpage (reusable hyperlink class)
    /// </summary>
    public sealed partial class HyperlinkSpan : Span
    {
        public static readonly BindableProperty UrlProperty =
            BindableProperty.Create(nameof(Url), typeof(string), typeof(HyperlinkSpan), null);

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public HyperlinkSpan()
        {
            FontFamily = "OpenSansRegular";
            FontAttributes = FontAttributes.Bold;
            FontSize = 16;
            TextDecorations = TextDecorations.Underline;

            GestureRecognizers.Add(new TapGestureRecognizer
            {
                // Launcher.OpenAsync is provided by Essentials
                //Command = new Command(async () => await Launcher.OpenAsync(Url))
                Command = new Command(async () => await OpenHyperlink(Url))
            });
        }

        /// <summary>
        /// Open the e-mail program or the website link
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task OpenHyperlink(string url)
        {
            if (url.StartsWith("mailto:"))
            {
                await OpenEmailLink(url[7..]);
            }
            //else
            //{
            //    await OpenWebsiteLink(url);
            //}
        }

        /// <summary>
        /// Open the e-mail program
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task OpenEmailLink(string url)
        {
            if (Email.Default.IsComposeSupported)
            {
                string subject = "Image to Text (OCR)";
                string body = "";
                string[] recipients = [url];

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = [.. recipients]
                };

                try
                {
                    await Email.Default.ComposeAsync(message);
                }
                catch (Exception ex)
                {
                    await Application.Current!.Windows[0].Page!.DisplayAlertAsync(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
                }
            }
        }

        ///// <summary>
        ///// Open the website link in the default browser
        ///// </summary>
        ///// <param name="url"></param>
        ///// <returns></returns>
        //private static async Task OpenWebsiteLink(string url)
        //{
        //    try
        //    {
        //        Uri uri = new(url);
        //        BrowserLaunchOptions options = new()
        //        {
        //            LaunchMode = BrowserLaunchMode.SystemPreferred,
        //            TitleMode = BrowserTitleMode.Show
        //        };

        //        await Browser.Default.OpenAsync(uri, options);
        //    }
        //    catch (Exception ex)
        //    {
        //        await Application.Current!.Windows[0].Page!.DisplayAlertAsync(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
        //    }
        //}
    }
}