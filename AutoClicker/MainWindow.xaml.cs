using AutoClicker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AutoClicker
{
    /// <summary>
    /// The application window. This hosts a Frame that displays pages. Add your
    /// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
    /// can use Page features such as navigation events and the Loaded lifecycle.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IThemeService _themeService;

        public MainWindow()
        {
            InitializeComponent();
            _themeService = App.Services.GetRequiredService<IThemeService>();
            _themeService.ThemeChanged += ThemeService_ThemeChanged;
            ApplyTheme();

            ExtendsContentIntoTitleBar = false;

            AppWindow.SetIcon("Assets/AppIcon.ico");

            AppWindow.Resize(new Windows.Graphics.SizeInt32(780, 500));

            CenterWindow();

            // Navigate the root frame to the main page on startup.
            RootFrame.Navigate(typeof(MainPage));
        }

        private void ThemeService_ThemeChanged(object? sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            WindowRoot.RequestedTheme = _themeService.ThemeMode switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default
            };
        }

        private void CenterWindow()
        {
            var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest);
            if (displayArea is not null)
            {
                var centeredPosition = AppWindow.Position;
                centeredPosition.X = (displayArea.WorkArea.Width - AppWindow.Size.Width) / 2;
                centeredPosition.Y = (displayArea.WorkArea.Height - AppWindow.Size.Height) / 2;
                AppWindow.Move(centeredPosition);
            }
        }
    }
}
