using System;
using System.Windows.Media;

namespace DesktopClock
{
    public class DesignWindow
    {
        public DesignWindow()
        {
            CurrentTime = DateTime.Now;
            BackColor = new SolidColorBrush(Color.FromArgb(160, 255, 255, 255));
        }

        public DateTime CurrentTime { get; set; }

        public Brush BackColor { get; set; }
    }
}
