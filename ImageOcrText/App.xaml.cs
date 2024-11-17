namespace ImageOcrText
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window dimensions and location for desktop apps
        /// </summary>
        /// <param name="activationState"></param>
        /// <returns></returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Microsoft.Maui.Controls.Window(new NavigationPage(new MainPage()))
            {
                X = 200,
                Y = 50,
                Height = 900,
                Width = 900,
                MinimumHeight = 800,
                MinimumWidth = 600,
                MaximumHeight = 1000,
                MaximumWidth = 900
            };

            return window;
        }
    }
}
