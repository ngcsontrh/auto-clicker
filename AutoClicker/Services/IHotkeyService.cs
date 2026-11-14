using System;
using Windows.System;

namespace AutoClicker.Services
{
    public interface IHotkeyService
    {
        /// <summary>The configured start/stop key.</summary>
        string Key { get; }

        /// <summary>True when <see cref="Key"/> is registered as a system-wide hotkey.</summary>
        bool IsRegistered { get; }

        /// <summary>True when the window is hooked to receive hotkey messages.</summary>
        bool IsListening { get; }

        /// <summary>Raised when the registered global hotkey is pressed.</summary>
        event Action? HotkeyPressed;

        /// <summary>Hooks the window and registers the current key as a global hotkey.</summary>
        bool TryInitialize(nint windowHandle, out string error);

        /// <summary>Changes the key using its function-key name (F1 through F12).</summary>
        bool TrySetKey(string key, out string error);
    }
}
