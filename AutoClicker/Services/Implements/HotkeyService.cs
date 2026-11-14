using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.Shell;

namespace AutoClicker.Services.Implements
{
    public sealed class HotkeyService : IHotkeyService
    {
        private const int StartStopHotkeyId = 1;

        private readonly SUBCLASSPROC _windowSubclassProc;
        private nint _windowHandle;
        private VirtualKey? _registeredKey;

        public HotkeyService()
        {
            _windowSubclassProc = WindowSubclassProc;
        }

        public string Key { get; private set; } = "F6";

        public bool IsRegistered => _registeredKey.HasValue;

        public bool IsListening { get; private set; }

        public event Action? HotkeyPressed;

        public bool TryInitialize(nint windowHandle, out string error)
        {
            if (IsListening)
            {
                if (!IsRegistered)
                {
                    return TryRegister(ParseHotkeyKey(Key), out error);
                }

                error = string.Empty;
                return true;
            }

            _windowHandle = windowHandle;
            if (!PInvoke.SetWindowSubclass(
                    (HWND)_windowHandle,
                    _windowSubclassProc,
                    (nuint)StartStopHotkeyId,
                    0))
            {
                error = $"Could not listen for the global hotkey: {new Win32Exception(Marshal.GetLastPInvokeError()).Message}";
                return false;
            }

            IsListening = true;
            return TryRegister(ParseHotkeyKey(Key), out error);
        }

        public bool TrySetKey(string key, out string error)
        {
            if (key is not ("F1" or "F2" or "F3" or "F4" or "F5" or "F6" or
                "F7" or "F8" or "F9" or "F10" or "F11" or "F12"))
            {
                error = "Select a valid hotkey from F1 through F12.";
                return false;
            }

            var virtualKey = ParseHotkeyKey(key);
            if (IsListening && !TryRegister(virtualKey, out error))
            {
                return false;
            }

            Key = key;
            error = string.Empty;
            return true;
        }

        private static VirtualKey ParseHotkeyKey(string key)
        {
            var keyNumber = int.Parse(key.AsSpan(1));
            return (VirtualKey)((int)VirtualKey.F1 + keyNumber - 1);
        }

        private LRESULT WindowSubclassProc(
            HWND hWnd,
            uint message,
            WPARAM wParam,
            LPARAM lParam,
            nuint subclassId,
            nuint refData)
        {
            if (message == PInvoke.WM_HOTKEY && (nuint)wParam == (nuint)StartStopHotkeyId)
            {
                HotkeyPressed?.Invoke();
                return (LRESULT)(nint)0;
            }

            if (message == PInvoke.WM_NCDESTROY && IsListening)
            {
                PInvoke.RemoveWindowSubclass(hWnd, _windowSubclassProc, subclassId);
                if (IsRegistered)
                {
                    PInvoke.UnregisterHotKey(hWnd, StartStopHotkeyId);
                }

                IsListening = false;
                _registeredKey = null;
            }

            return PInvoke.DefSubclassProc(hWnd, message, wParam, lParam);
        }

        private bool TryRegister(VirtualKey key, out string error)
        {
            var previousKey = _registeredKey;
            if (IsRegistered &&
                !PInvoke.UnregisterHotKey((HWND)_windowHandle, StartStopHotkeyId))
            {
                error = $"Could not unregister the previous hotkey: {new Win32Exception(Marshal.GetLastPInvokeError()).Message}";
                return false;
            }

            _registeredKey = null;

            if (PInvoke.RegisterHotKey(
                    (HWND)_windowHandle,
                    StartStopHotkeyId,
                    HOT_KEY_MODIFIERS.MOD_NOREPEAT,
                    (uint)key))
            {
                _registeredKey = key;
                error = string.Empty;
                return true;
            }

            error = $"Could not register {key} as a global hotkey: {new Win32Exception(Marshal.GetLastPInvokeError()).Message}";
            if (previousKey.HasValue)
            {
                if (PInvoke.RegisterHotKey(
                        (HWND)_windowHandle,
                        StartStopHotkeyId,
                        HOT_KEY_MODIFIERS.MOD_NOREPEAT,
                        (uint)previousKey.Value))
                {
                    _registeredKey = previousKey;
                }
                else
                {
                    error += $" The previous hotkey could not be restored: {new Win32Exception(Marshal.GetLastPInvokeError()).Message}";
                }
            }

            return false;
        }
    }
}
