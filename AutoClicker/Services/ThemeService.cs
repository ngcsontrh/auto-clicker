namespace AutoClicker.Services
{
    public sealed class ThemeService : IThemeService
    {
        public string ThemeMode { get; private set; } = "System";

        public event EventHandler? ThemeChanged;

        public void SetThemeMode(string themeMode)
        {
            if (!AppSettings.IsValidThemeMode(themeMode))
            {
                throw new ArgumentException("The theme mode is invalid.", nameof(themeMode));
            }

            if (ThemeMode == themeMode)
            {
                return;
            }

            ThemeMode = themeMode;
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
