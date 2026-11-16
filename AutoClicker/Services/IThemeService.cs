namespace AutoClicker.Services
{
    public interface IThemeService
    {
        string ThemeMode { get; }

        event EventHandler? ThemeChanged;

        void SetThemeMode(string themeMode);
    }
}
