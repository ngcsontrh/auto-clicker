using System.Threading.Tasks;
using System.ComponentModel;
using AutoClicker.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AutoClicker
{
    /// <summary>
    /// The main content page displayed inside the application window.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; }

        public MainPage()
        {
            ViewModel = App.Services.GetRequiredService<MainPageViewModel>();
            InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Loaded += MainPage_Loaded;
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainPageViewModel.IsRunning))
            {
                if (ViewModel.IsRunning)
                {
                    StartButton.Background = new SolidColorBrush(Color.FromArgb(255, 196, 43, 28));
                }
                else
                {
                    StartButton.ClearValue(Control.BackgroundProperty);
                }
            }
            else if (e.PropertyName == nameof(MainPageViewModel.ErrorMessage) &&
                !string.IsNullOrEmpty(ViewModel.ErrorMessage))
            {
                await ShowAutoClickErrorAsync(ViewModel.ErrorMessage);
            }
            else if (e.PropertyName == nameof(MainPageViewModel.LocationPickErrorMessage) &&
                !string.IsNullOrEmpty(ViewModel.LocationPickErrorMessage))
            {
                await ShowLocationPickErrorAsync(ViewModel.LocationPickErrorMessage);
            }
            else if (e.PropertyName == nameof(MainPageViewModel.SettingsErrorMessage) &&
                !string.IsNullOrEmpty(ViewModel.SettingsErrorMessage))
            {
                await ShowSettingsErrorAsync(ViewModel.SettingsErrorMessage);
            }
        }

        private async void HotkeySettingButton_Click(object sender, RoutedEventArgs e)
        {
            var hotkeyPicker = new ComboBox
            {
                MinWidth = 160,
                SelectedIndex = int.Parse(ViewModel.HotkeyKey.AsSpan(1)) - 1
            };

            for (var keyNumber = 1; keyNumber <= 12; keyNumber++)
            {
                hotkeyPicker.Items.Add($"F{keyNumber}");
            }

            var dialog = new ContentDialog
            {
                Title = "Start/Stop hotkey",
                Content = hotkeyPicker,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary &&
                hotkeyPicker.SelectedIndex >= 0)
            {
                var selectedKey = $"F{hotkeyPicker.SelectedIndex + 1}";
                if (!ViewModel.TrySetHotkey(selectedKey, out var error))
                {
                    await ShowHotkeyErrorAsync(error);
                }
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.TryInitializeHotkey(App.WindowHandle, out var error))
            {
                await ShowHotkeyErrorAsync(error);
            }

            if (!string.IsNullOrEmpty(ViewModel.SettingsErrorMessage))
            {
                await ShowSettingsErrorAsync(ViewModel.SettingsErrorMessage);
            }

            StartButton.Focus(FocusState.Programmatic);
        }

        private async Task ShowHotkeyErrorAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Global hotkey unavailable",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowAutoClickErrorAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Auto-click stopped",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowLocationPickErrorAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Location picker unavailable",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async Task ShowSettingsErrorAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Settings unavailable",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
