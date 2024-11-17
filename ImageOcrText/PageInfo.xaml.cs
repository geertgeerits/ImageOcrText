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
                DisplayAlert("InitializeComponent: PageInfo", ex.Message, "OK");
                return;
            }
#if WINDOWS
            //// Set the margins for the controls in the title bar for Windows
            lblTitlePage.Margin = new Thickness(70, 5, 0, 0);
#endif
            //// Put text in the chosen language in the controls and variables
            lblVersion.Text = $"{OcrLang.Version_Text} 1.0.8";
            lblCopyright.Text = $"{OcrLang.Copyright_Text} © 2024-2024 Geert Geerits";
            lblEmail.Text = $"{OcrLang.Email_Text} geertgeerits@gmail.com";
            lblWebsite.Text = $"{OcrLang.Website_Text}: ../imagetotext";
            lblPrivacyPolicy.Text = $"\n{OcrLang.PrivacyPolicyTitle_Text} {OcrLang.PrivacyPolicy_Text}";
            lblLicense.Text = $"\n{OcrLang.LicenseTitle_Text}: {OcrLang.License_Text}";
            lblExplanation.Text = $"\n{OcrLang.InfoExplanation_Text}";
        }

        /// <summary>
        /// Open e-mail program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBtnEmailLinkClicked(object sender, EventArgs e)
        {
            if (Email.Default.IsComposeSupported)
            {
                string subject = OcrLang.NameProgram_Text;
                string body = "";
                string[] recipients = ["geertgeerits@gmail.com"];

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = body,
                    BodyFormat = EmailBodyFormat.PlainText,
                    To = new List<string>(recipients)
                };

                try
                {
                    await Email.Default.ComposeAsync(message);
                }
                catch (Exception ex)
                {
                    await DisplayAlert(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
                }
            }
        }

        /// <summary>
        /// Open the website link in the default browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBtnWebsiteLinkClicked(object sender, EventArgs e)
        {
            try
            {
                Uri uri = new("https://geertgeerits.wixsite.com/geertgeerits/image-to-text");
                BrowserLaunchOptions options = new()
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred,
                    TitleMode = BrowserTitleMode.Show
                };

                await Browser.Default.OpenAsync(uri, options);
            }
            catch (Exception ex)
            {
                await DisplayAlert(OcrLang.ErrorTitle_Text, ex.Message, OcrLang.ButtonClose_Text);
            }
        }
    }
}