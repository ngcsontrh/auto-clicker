using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AutoClicker.Services;
using System.Drawing;

namespace AutoClicker.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IHotkeyService _hotkeyService;
        private readonly IMouseService _mouseService;
        private readonly SettingsService _settingsService;
        private CancellationTokenSource? _clickCancellation;
        private CancellationTokenSource? _locationPickCancellation;
        private bool _isLoadingSettings;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StartStopText))]
        [NotifyPropertyChangedFor(nameof(CanPickLocation))]
        public partial bool IsRunning { get; set; }

        [ObservableProperty]
        public partial double IntervalHours { get; set; }

        [ObservableProperty]
        public partial double IntervalMinutes { get; set; }

        [ObservableProperty]
        public partial double IntervalSeconds { get; set; }

        [ObservableProperty]
        public partial double IntervalMilliseconds { get; set; } = 100;

        [ObservableProperty]
        public partial bool IsRandomOffsetEnabled { get; set; }

        [ObservableProperty]
        public partial double RandomOffsetMilliseconds { get; set; } = 40;

        [ObservableProperty]
        public partial int MouseButtonIndex { get; set; }

        [ObservableProperty]
        public partial int ClickTypeIndex { get; set; }

        [ObservableProperty]
        public partial double RepeatCount { get; set; } = 1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsFixedRepeat))]
        public partial bool RepeatUntilStopped { get; set; } = true;

        public bool IsFixedRepeat
        {
            get => !RepeatUntilStopped;
            set
            {
                if (value)
                {
                    RepeatUntilStopped = false;
                }
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(UseSelectedLocation))]
        public partial bool UseCurrentLocation { get; set; } = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanPickLocation))]
        [NotifyPropertyChangedFor(nameof(PickLocationButtonText))]
        public partial bool IsPickingLocation { get; set; }

        [ObservableProperty]
        public partial string? LocationPickErrorMessage { get; set; }

        public bool CanPickLocation => !IsRunning && !IsPickingLocation;

        public string PickLocationButtonText => IsPickingLocation ? "Picking..." : "Pick location";

        public bool UseSelectedLocation
        {
            get => !UseCurrentLocation;
            set
            {
                if (value)
                {
                    UseCurrentLocation = false;
                }
            }
        }

        [ObservableProperty]
        public partial double CursorX { get; set; }

        [ObservableProperty]
        public partial double CursorY { get; set; }

        [ObservableProperty]
        public partial string? ErrorMessage { get; set; }

        [ObservableProperty]
        public partial string? SettingsErrorMessage { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StartStopText))]
        public partial string HotkeyKey { get; set; } = "F6";

        public string StartStopText => $"{(IsRunning ? "Stop" : "Start")} ({HotkeyKey})";

        [RelayCommand]
        private void ToggleRunning()
        {
            IsRunning = !IsRunning;
        }

        [RelayCommand]
        private async Task PickLocationAsync()
        {
            if (!CanPickLocation)
            {
                return;
            }

            var cancellation = new CancellationTokenSource();
            _locationPickCancellation = cancellation;
            IsPickingLocation = true;
            LocationPickErrorMessage = null;

            try
            {
                var location = await _mouseService.PickLocationAsync(cancellation.Token);
                CursorX = location.X;
                CursorY = location.Y;
                UseCurrentLocation = false;
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (OperationCanceledException)
            {
                // Escape cancels the location selection without changing the saved coordinates.
            }
            catch (Exception exception)
            {
                LocationPickErrorMessage = exception.Message;
            }
            finally
            {
                if (ReferenceEquals(_locationPickCancellation, cancellation))
                {
                    _locationPickCancellation = null;
                }

                IsPickingLocation = false;
                cancellation.Dispose();
            }
        }

        public MainPageViewModel(
            IHotkeyService hotkeyService,
            IMouseService mouseService,
            SettingsService settingsService)
        {
            _hotkeyService = hotkeyService;
            _mouseService = mouseService;
            _settingsService = settingsService;
            _hotkeyService.HotkeyPressed += ToggleRunning;
            LoadSettings();
        }

        private void LoadSettings()
        {
            AppSettings settings;
            try
            {
                settings = _settingsService.Load();
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException or
                System.Text.Json.JsonException or InvalidDataException)
            {
                SettingsErrorMessage = $"Could not load settings: {exception.Message}";
                return;
            }

            _isLoadingSettings = true;
            try
            {
                IntervalHours = settings.IntervalHours;
                IntervalMinutes = settings.IntervalMinutes;
                IntervalSeconds = settings.IntervalSeconds;
                IntervalMilliseconds = settings.IntervalMilliseconds;
                IsRandomOffsetEnabled = settings.IsRandomOffsetEnabled;
                RandomOffsetMilliseconds = settings.RandomOffsetMilliseconds;
                MouseButtonIndex = settings.MouseButtonIndex;
                ClickTypeIndex = settings.ClickTypeIndex;
                RepeatCount = settings.RepeatCount;
                RepeatUntilStopped = settings.RepeatUntilStopped;
                UseCurrentLocation = settings.UseCurrentLocation;
                CursorX = settings.CursorX;
                CursorY = settings.CursorY;

                if (!_hotkeyService.TrySetKey(settings.HotkeyKey, out var error))
                {
                    SettingsErrorMessage = $"Could not load the saved hotkey: {error}";
                }
                else
                {
                    HotkeyKey = settings.HotkeyKey;
                }
            }
            finally
            {
                _isLoadingSettings = false;
            }
        }

        private void SaveSettings()
        {
            if (_isLoadingSettings)
            {
                return;
            }

            var settings = new AppSettings
            {
                IntervalHours = IntervalHours,
                IntervalMinutes = IntervalMinutes,
                IntervalSeconds = IntervalSeconds,
                IntervalMilliseconds = IntervalMilliseconds,
                IsRandomOffsetEnabled = IsRandomOffsetEnabled,
                RandomOffsetMilliseconds = RandomOffsetMilliseconds,
                MouseButtonIndex = MouseButtonIndex,
                ClickTypeIndex = ClickTypeIndex,
                RepeatCount = RepeatCount,
                RepeatUntilStopped = RepeatUntilStopped,
                UseCurrentLocation = UseCurrentLocation,
                CursorX = CursorX,
                CursorY = CursorY,
                HotkeyKey = HotkeyKey
            };

            try
            {
                _settingsService.Save(settings);
                SettingsErrorMessage = null;
            }
            catch (Exception exception) when (
                exception is IOException or UnauthorizedAccessException or InvalidDataException)
            {
                SettingsErrorMessage = $"Could not save settings: {exception.Message}";
            }
        }

        partial void OnIntervalHoursChanged(double value) => SaveSettings();
        partial void OnIntervalMinutesChanged(double value) => SaveSettings();
        partial void OnIntervalSecondsChanged(double value) => SaveSettings();
        partial void OnIntervalMillisecondsChanged(double value) => SaveSettings();
        partial void OnIsRandomOffsetEnabledChanged(bool value) => SaveSettings();
        partial void OnRandomOffsetMillisecondsChanged(double value) => SaveSettings();
        partial void OnMouseButtonIndexChanged(int value) => SaveSettings();
        partial void OnClickTypeIndexChanged(int value) => SaveSettings();
        partial void OnRepeatCountChanged(double value) => SaveSettings();
        partial void OnRepeatUntilStoppedChanged(bool value) => SaveSettings();
        partial void OnUseCurrentLocationChanged(bool value) => SaveSettings();
        partial void OnCursorXChanged(double value) => SaveSettings();
        partial void OnCursorYChanged(double value) => SaveSettings();
        partial void OnHotkeyKeyChanged(string value) => SaveSettings();

        partial void OnIsRunningChanged(bool value)
        {
            if (value)
            {
                _locationPickCancellation?.Cancel();
                ErrorMessage = null;
                var cancellation = new CancellationTokenSource();
                _clickCancellation = cancellation;
                _ = RunAutoClickAsync(cancellation);
            }
            else
            {
                _clickCancellation?.Cancel();
            }
        }

        private async Task RunAutoClickAsync(CancellationTokenSource cancellation)
        {
            var token = cancellation.Token;
            try
            {
                var button = MouseButtonIndex switch
                {
                    0 => MouseButton.Left,
                    1 => MouseButton.Right,
                    2 => MouseButton.Middle,
                    _ => throw new InvalidOperationException("Select a valid mouse button.")
                };
                var clickType = ClickTypeIndex switch
                {
                    0 => MouseClickType.Single,
                    1 => MouseClickType.Double,
                    _ => throw new InvalidOperationException("Select a valid click type.")
                };
                var repeatUntilStopped = RepeatUntilStopped;
                var repeatCount = Math.Ceiling(RepeatCount);
                if (!repeatUntilStopped && (!double.IsFinite(repeatCount) || repeatCount < 1))
                {
                    throw new InvalidOperationException("Repeat count must be at least 1.");
                }

                double completedClicks = 0;
                while (repeatUntilStopped || completedClicks < repeatCount)
                {
                    token.ThrowIfCancellationRequested();
                    Point? position = UseCurrentLocation
                        ? null
                        : new Point(Convert.ToInt32(CursorX), Convert.ToInt32(CursorY));

                    _mouseService.Click(button, clickType, position);
                    completedClicks++;

                    if (!repeatUntilStopped && completedClicks >= repeatCount)
                    {
                        break;
                    }

                    await Task.Delay(GetClickInterval(), token);
                }

                if (!token.IsCancellationRequested)
                {
                    IsRunning = false;
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                ErrorMessage = $"Auto-click stopped: {exception.Message}";
                IsRunning = false;
            }
            finally
            {
                if (ReferenceEquals(_clickCancellation, cancellation))
                {
                    _clickCancellation = null;
                }

                cancellation.Dispose();
            }
        }

        private TimeSpan GetClickInterval()
        {
            var intervalMilliseconds =
                IntervalHours * 3_600_000 +
                IntervalMinutes * 60_000 +
                IntervalSeconds * 1_000 +
                IntervalMilliseconds;

            if (!double.IsFinite(intervalMilliseconds) || intervalMilliseconds < 0)
            {
                throw new InvalidOperationException("Click interval must be a non-negative, finite value.");
            }

            if (IsRandomOffsetEnabled)
            {
                if (!double.IsFinite(RandomOffsetMilliseconds) || RandomOffsetMilliseconds < 0)
                {
                    throw new InvalidOperationException("Random offset must be a non-negative, finite value.");
                }

                intervalMilliseconds = Math.Max(
                    0,
                    intervalMilliseconds + (Random.Shared.NextDouble() * 2 - 1) * RandomOffsetMilliseconds);
            }

            return TimeSpan.FromMilliseconds(intervalMilliseconds);
        }

        public bool TryInitializeHotkey(nint windowHandle, out string error)
        {
            return _hotkeyService.TryInitialize(windowHandle, out error);
        }

        public bool TrySetHotkey(string key, out string error)
        {
            if (!_hotkeyService.TrySetKey(key, out error))
            {
                return false;
            }

            HotkeyKey = key;
            return true;
        }

    }
}
