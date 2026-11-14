using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;

namespace AutoClicker.Services.Implements
{
    public sealed class MouseService : IMouseService
    {
        private int _isPickingLocation;

        public async Task<Point> PickLocationAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Exchange(ref _isPickingLocation, 1) != 0)
            {
                throw new InvalidOperationException("A location is already being picked.");
            }

            var completion = new TaskCompletionSource<Point>(TaskCreationOptions.RunContinuationsAsynchronously);
            var suppressLeftButtonUp = false;
            HOOKPROC? mouseHookProcedure = null;
            HOOKPROC? keyboardHookProcedure = null;
            UnhookWindowsHookExSafeHandle? mouseHook = null;
            UnhookWindowsHookExSafeHandle? keyboardHook = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                mouseHookProcedure = (code, wParam, lParam) =>
                {
                    if (code == PInvoke.HC_ACTION && (nuint)wParam == PInvoke.WM_LBUTTONUP)
                    {
                        if (suppressLeftButtonUp)
                        {
                            suppressLeftButtonUp = false;
                            var mouseData = Marshal.PtrToStructure<MSLLHOOKSTRUCT>((nint)lParam);
                            completion.TrySetResult(new Point(mouseData.pt.X, mouseData.pt.Y));
                            return (LRESULT)(nint)1;
                        }
                    }
                    else if (code == PInvoke.HC_ACTION && (nuint)wParam == PInvoke.WM_LBUTTONDOWN)
                    {
                        suppressLeftButtonUp = true;
                        return (LRESULT)(nint)1;
                    }

                    return PInvoke.CallNextHookEx(
                        mouseHook is null ? default : (HHOOK)mouseHook.DangerousGetHandle(),
                        code,
                        wParam,
                        lParam);
                };
                keyboardHookProcedure = (code, wParam, lParam) =>
                {
                    if (code == PInvoke.HC_ACTION &&
                        ((nuint)wParam == PInvoke.WM_KEYDOWN || (nuint)wParam == PInvoke.WM_SYSKEYDOWN) &&
                        Marshal.PtrToStructure<KBDLLHOOKSTRUCT>((nint)lParam).vkCode == (uint)VIRTUAL_KEY.VK_ESCAPE)
                    {
                        completion.TrySetCanceled(cancellationToken);
                        return (LRESULT)(nint)1;
                    }

                    return PInvoke.CallNextHookEx(
                        keyboardHook is null ? default : (HHOOK)keyboardHook.DangerousGetHandle(),
                        code,
                        wParam,
                        lParam);
                };

                mouseHook = PInvoke.SetWindowsHookEx(
                    WINDOWS_HOOK_ID.WH_MOUSE_LL,
                    mouseHookProcedure,
                    default,
                    0);
                if (mouseHook.IsInvalid)
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not start the location picker.");
                }

                keyboardHook = PInvoke.SetWindowsHookEx(
                    WINDOWS_HOOK_ID.WH_KEYBOARD_LL,
                    keyboardHookProcedure,
                    default,
                    0);
                if (keyboardHook.IsInvalid)
                {
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), "Could not enable Escape cancellation.");
                }

                using var cancellationRegistration = cancellationToken.Register(
                    () => completion.TrySetCanceled(cancellationToken));
                return await completion.Task.ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    keyboardHook?.Dispose();
                    mouseHook?.Dispose();
                }
                finally
                {
                    GC.KeepAlive(mouseHookProcedure);
                    GC.KeepAlive(keyboardHookProcedure);
                    Volatile.Write(ref _isPickingLocation, 0);
                }
            }
        }

        public void Click(MouseButton button, MouseClickType clickType, Point? position = null)
        {
            var (downFlag, upFlag) = button switch
            {
                MouseButton.Left => (MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN, MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP),
                MouseButton.Right => (MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN, MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP),
                MouseButton.Middle => (MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN, MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP),
                _ => throw new ArgumentOutOfRangeException(nameof(button))
            };

            var clickCount = clickType switch
            {
                MouseClickType.Single => 1,
                MouseClickType.Double => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(clickType))
            };

            if (position is Point targetPosition &&
                !PInvoke.SetCursorPos(targetPosition.X, targetPosition.Y))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            var inputs = new INPUT[clickCount * 2];
            for (var index = 0; index < clickCount; index++)
            {
                inputs[index * 2] = CreateMouseInput(downFlag);
                inputs[index * 2 + 1] = CreateMouseInput(upFlag);
            }

            unsafe
            {
                fixed (INPUT* inputPointer = inputs)
                {
                    var sentCount = PInvoke.SendInput((uint)inputs.Length, inputPointer, Marshal.SizeOf<INPUT>());
                    if (sentCount != inputs.Length)
                    {
                        if (sentCount % 2 != 0)
                        {
                            var releaseInput = CreateMouseInput(upFlag);
                            INPUT* releasePointer = &releaseInput;
                            PInvoke.SendInput(1, releasePointer, Marshal.SizeOf<INPUT>());
                        }

                        throw new InvalidOperationException("Windows did not accept all mouse input events.");
                    }
                }
            }
        }

        public Point GetCursorPosition()
        {
            if (!PInvoke.GetCursorPos(out var cursorPosition))
            {
                throw new Win32Exception(Marshal.GetLastPInvokeError());
            }

            return new Point(cursorPosition.X, cursorPosition.Y);
        }

        private static INPUT CreateMouseInput(MOUSE_EVENT_FLAGS flags)
        {
            return new INPUT
            {
                type = INPUT_TYPE.INPUT_MOUSE,
                Anonymous =
                {
                    mi = new MOUSEINPUT
                    {
                        dwFlags = flags
                    }
                }
            };
        }
    }
}
