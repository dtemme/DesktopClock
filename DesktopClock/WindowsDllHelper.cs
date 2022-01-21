using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace DesktopClock
{
    public class WindowsDllHelper
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        static public Color GetPixelColor(int x, int y)
        {
            var hdc = GetDC(IntPtr.Zero);
            var pixel = GetPixel(hdc, x, y);
            _ = ReleaseDC(IntPtr.Zero, hdc);
            var color = Color.FromRgb(
                (byte)(pixel & 0x000000FF),
                (byte)((pixel & 0x0000FF00) >> 8),
                (byte)((pixel & 0x00FF0000) >> 16));
            return color;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    }
}
