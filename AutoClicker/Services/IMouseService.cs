using System.Drawing;

namespace AutoClicker.Services
{
    public interface IMouseService
    {
        /// <summary>Returns the current cursor position in screen pixels.</summary>
        Point GetCursorPosition();

        /// <summary>
        /// Performs a single or double click. If <paramref name="position"/> is null,
        /// clicks at the current cursor position.
        /// </summary>
        void Click(MouseButton button, MouseClickType clickType, Point? position = null);

        /// <summary>Waits for a screen location to be selected with the left mouse button.</summary>
        Task<Point> PickLocationAsync(CancellationToken cancellationToken);
    }

    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public enum MouseClickType
    {
        Single,
        Double
    }
}
