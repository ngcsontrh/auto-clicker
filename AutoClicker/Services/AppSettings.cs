namespace AutoClicker.Services
{
    public sealed class AppSettings
    {
        public double IntervalHours { get; set; }
        public double IntervalMinutes { get; set; }
        public double IntervalSeconds { get; set; }
        public double IntervalMilliseconds { get; set; } = 100;
        public bool IsRandomOffsetEnabled { get; set; }
        public double RandomOffsetMilliseconds { get; set; } = 40;
        public int MouseButtonIndex { get; set; }
        public int ClickTypeIndex { get; set; }
        public double RepeatCount { get; set; } = 1;
        public bool RepeatUntilStopped { get; set; } = true;
        public bool UseCurrentLocation { get; set; } = true;
        public double CursorX { get; set; }
        public double CursorY { get; set; }
        public string HotkeyKey { get; set; } = "F6";

        public bool IsValid =>
            double.IsFinite(IntervalHours) && IntervalHours >= 0 &&
            double.IsFinite(IntervalMinutes) && IntervalMinutes >= 0 &&
            double.IsFinite(IntervalSeconds) && IntervalSeconds >= 0 &&
            double.IsFinite(IntervalMilliseconds) && IntervalMilliseconds >= 0 &&
            double.IsFinite(RandomOffsetMilliseconds) && RandomOffsetMilliseconds >= 0 &&
            MouseButtonIndex is >= 0 and <= 2 &&
            ClickTypeIndex is >= 0 and <= 1 &&
            double.IsFinite(RepeatCount) && RepeatCount >= 1 &&
            double.IsFinite(CursorX) && double.IsFinite(CursorY) &&
            IsValidHotkeyKey(HotkeyKey);

        public static bool IsValidHotkeyKey(string? key) =>
            key is "F1" or "F2" or "F3" or "F4" or "F5" or "F6" or
                "F7" or "F8" or "F9" or "F10" or "F11" or "F12";
    }
}
