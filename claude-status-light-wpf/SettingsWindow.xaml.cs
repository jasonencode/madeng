using System.Windows;
using System.Windows.Controls;

namespace StatusLight
{
    public partial class SettingsWindow : Window
    {
        public int MarqueeOn { get; private set; }
        public int MarqueeOff { get; private set; }
        public int BlinkOn { get; private set; }
        public int BlinkOff { get; private set; }
        public double BackgroundOpacity { get; private set; }
        public int Port { get; private set; }

        public SettingsWindow(int marqueeOn, int marqueeOff, int blinkOn, int blinkOff, double opacity, int port)
        {
            InitializeComponent();
            
            MarqueeOnTime.Text = marqueeOn.ToString();
            MarqueeOffTime.Text = marqueeOff.ToString();
            BlinkOnTime.Text = blinkOn.ToString();
            BlinkOffTime.Text = blinkOff.ToString();
            
            OpacitySlider.Value = opacity;
            OpacityValue.Text = $"{(int)(opacity * 100)}%";
            
            HttpPort.Text = port.ToString();

            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OpacityValue.Text = $"{(int)(e.NewValue * 100)}%";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MarqueeOnTime.Text, out var marqueeOn) &&
                int.TryParse(MarqueeOffTime.Text, out var marqueeOff) &&
                int.TryParse(BlinkOnTime.Text, out var blinkOn) &&
                int.TryParse(BlinkOffTime.Text, out var blinkOff) &&
                int.TryParse(HttpPort.Text, out var port))
            {
                MarqueeOn = marqueeOn;
                MarqueeOff = marqueeOff;
                BlinkOn = blinkOn;
                BlinkOff = blinkOff;
                BackgroundOpacity = OpacitySlider.Value;
                Port = port;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("请输入有效的数字", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
