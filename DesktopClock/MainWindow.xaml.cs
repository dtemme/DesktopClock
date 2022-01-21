using System;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DesktopClock.Properties;

namespace DesktopClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isWhiteTheme;
        private bool isRandomColor;
        private bool isMouseHover;
        private readonly Autostart autostart = new();
        private Color lastColor = default;

        public MainWindow()
        {
            AutostartActive = autostart.Exists();
            IsTransparent = Settings.Default.IsTransparent;
            InitializeComponent();

            isWhiteTheme = Settings.Default.IsWhiteTheme;
            isRandomColor = Settings.Default.RandomColor;
            SetWindowPosition();

            Coloration();
            HideFrame();
            StartTimer();
        }

        private void CurrentTimeTimerElapsedEvent(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, delegate () { CurrentTime = DateTime.Now; });
        }

        private void CloseEvent(object sender, MouseButtonEventArgs e) => CloseApplication();

        private void DragEvent(object sender, MouseButtonEventArgs e) => DragWindow();

        private void ColorSwitchEvent(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == colorSwitch)
                SwitchColor();
            else if (sender == colorSwitchRandom)
                isRandomColor = !isRandomColor;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseHover = true;
            ShowFrame();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseHover = false;
            HideFrame();
        }

        private void SetWindowPosition()
        {
            Left = Settings.Default.StartPosition.X;
            Top = Settings.Default.StartPosition.Y;
        }

        private void DragWindow()
        {
            try
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    Window.DragMove();
                    AdjustForegroundColor();
                }
            }
            catch (InvalidOperationException) { }
        }

        private static void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        private void ShowFrame()
        {
            BackColor.Opacity = 1;
            ElementVisibility = Visibility.Visible;
        }

        private void HideFrame()
        {
            if (IsTransparent)
                BackColor.Opacity = 0;
            ElementVisibility = Visibility.Hidden;
        }

        private void StartTimer()
        {
            var currentTimeTimer = new Timer(1000);
            currentTimeTimer.Elapsed += new ElapsedEventHandler(CurrentTimeTimerElapsedEvent);
            currentTimeTimer.Elapsed += TimerElapsed;
            currentTimeTimer.Start();
        }

        private void SwitchColor()
        {
            isWhiteTheme = !isWhiteTheme;
            isRandomColor = false;
            Coloration();
        }

        private void Coloration()
        {
            var whiteBrush = new SolidColorBrush(Colors.White);
            var blackBrush = new SolidColorBrush(Colors.Black);
            var transWhiteBrush = new SolidColorBrush(TransparentColor(whiteBrush.Color, 160));
            var transBlackBrush = new SolidColorBrush(TransparentColor(blackBrush.Color, 160));
            if (isWhiteTheme)
                //White font 
                AttachColors(whiteBrush, transBlackBrush, blackBrush);
            else
                //Black font
                AttachColors(blackBrush, transWhiteBrush, whiteBrush);
        }

        private static Color TransparentColor(Color color, byte alpha) => Color.FromArgb(alpha, color.R, color.G, color.B);

        private void TimerElapsed(object sender, ElapsedEventArgs e) { AdjustForegroundColor(); }

        private void AdjustForegroundColor()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (!isRandomColor || Application.Current == null) //when app is closed timer may elapse once more
                    return;
                var color = WindowsDllHelper.GetPixelColor((int)Application.Current.MainWindow.Left - 1, (int)Application.Current.MainWindow.Top);
                if (color != lastColor)
                {
                    var contrast = GetContrastingColor(color);
                    AttachColors(contrast, Settings.Default.IsTransparent && !isMouseHover ? TransparentColor(color, 0) : TransparentColor(color, 160), isWhiteTheme ? Colors.Black : Colors.White);
                    lastColor = color;
                }
            }));
        }

        private static Color GetContrastingColor(Color color)
        {
            var r = color.R > 0 ? 256 - color.R : 255;
            var g = color.G > 0 ? 256 - color.G : 255;
            var b = color.B > 0 ? 256 - color.B : 255;
            return Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
        }

        private void AttachColors(Color fontColor, Color backColor, Color changeColorButtonColor)
        {
            AttachColors(new SolidColorBrush(fontColor), new SolidColorBrush(backColor), new SolidColorBrush(changeColorButtonColor));
        }

        private void AttachColors(SolidColorBrush fontColor, SolidColorBrush backColor, SolidColorBrush changeColorButtonColor)
        {
            FontColor = fontColor;
            BackColor = backColor;
            colorSwitch.Background = changeColorButtonColor;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            Settings.Default.StartPosition = new System.Drawing.Point((int)Left, (int)Top);
            Settings.Default.IsWhiteTheme = isWhiteTheme;
            Settings.Default.RandomColor = isRandomColor;
            Settings.Default.IsTransparent = IsTransparent;
            Settings.Default.Save();
        }

        public DateTime CurrentTime
        {
            get => (DateTime)GetValue(CurrentTimeProperty);
            set => SetValue(CurrentTimeProperty, value);
        }

        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(MainWindow), new PropertyMetadata(DateTime.Now));

        public Brush FontColor
        {
            get => (Brush)GetValue(FontColorProperty);
            set => SetValue(FontColorProperty, value);
        }

        public static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register(nameof(FontColor), typeof(Brush), typeof(MainWindow));

        public Brush BackColor
        {
            get => (Brush)GetValue(BackColorProperty);
            set => SetValue(BackColorProperty, value);
        }

        public static readonly DependencyProperty BackColorProperty =
            DependencyProperty.Register(nameof(BackColor), typeof(Brush), typeof(MainWindow));

        public bool AutostartActive
        {
            get => (bool)GetValue(AutostartProperty);
            set
            {
                SetValue(AutostartProperty, value);
                if (value)
                    autostart.Add(System.Reflection.Assembly.GetExecutingAssembly().Location);
                else
                    autostart.Remove();
            }
        }

        public static readonly DependencyProperty AutostartProperty =
            DependencyProperty.Register(nameof(Autostart), typeof(bool), typeof(MainWindow));

        public Visibility ElementVisibility
        {
            get => (Visibility)GetValue(ElementVisibilityProperty);
            set => SetValue(ElementVisibilityProperty, value);
        }

        public static readonly DependencyProperty ElementVisibilityProperty =
            DependencyProperty.Register(nameof(ElementVisibility), typeof(Visibility), typeof(MainWindow));

        public bool IsTransparent
        {
            get => (bool)GetValue(IsTransparentProperty);
            set => SetValue(IsTransparentProperty, value);
        }

        public static readonly DependencyProperty IsTransparentProperty =
            DependencyProperty.Register(nameof(IsTransparent), typeof(bool), typeof(MainWindow));
    }
}
