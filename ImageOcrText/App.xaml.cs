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
            return new Window(new AppShell())
            {
                X = 200,
                Y = 50,
                Height = 900,
                Width = 900,
                MinimumHeight = 800,
                MinimumWidth = 900,
                MaximumHeight = 1100,
                MaximumWidth = 1100
            };
        }
    }
}
