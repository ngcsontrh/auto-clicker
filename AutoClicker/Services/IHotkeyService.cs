using System;
using Windows.System;

namespace AutoClicker.Services
{
    public interface IHotkeyService
    {
        /// <summary>The configured start/stop key.</summary>
        VirtualKey Key { get; }

        /// <summary>True when <see cref="Key"/> is registered as a system-wide hotkey.</summary>
        bool IsRegistered { get; }

        /// <summary>True when the window is hooked to receive hotkey messages.</summary>
        bool IsListening { get; }

        /// <summary>Raised when the registered global hotkey is pressed.</summary>
        event Action? HotkeyPressed;

        /// <summary>Hooks the window and registers the current key as a global hotkey.</summary>
        bool TryInitialize(nint windowHandle, out string error);

        /// <summary>
        /// Changes the key. If the service is listening the new key is registered first;
        /// on failure the previous key is restored.
        /// </summary>
        bool TrySetKey(VirtualKey key, out string error);
    }
}
