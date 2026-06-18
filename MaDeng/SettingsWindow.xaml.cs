using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MaDeng
{
    public partial class SettingsWindow : Window
    {
        public int MarqueeOn { get; private set; }
        public int MarqueeOff { get; private set; }
        public int BlinkOn { get; private set; }
        public int BlinkOff { get; private set; }
        public double BackgroundOpacity { get; private set; }

        public SettingsWindow(int marqueeOn, int marqueeOff, int blinkOn, int blinkOff, double opacity)
        {
            InitializeComponent();

            MarqueeOnTime.Text = marqueeOn.ToString();
            MarqueeOffTime.Text = marqueeOff.ToString();
            BlinkOnTime.Text = blinkOn.ToString();
            BlinkOffTime.Text = blinkOff.ToString();

            OpacitySlider.Value = opacity;
            OpacityValue.Text = $"{(int)(opacity * 100)}%";

            OpacitySlider.ValueChanged += OpacitySlider_ValueChanged;

            // 只允许输入数字
            var numericHandler = new TextCompositionEventHandler(NumericOnly_PreviewTextInput);
            MarqueeOnTime.PreviewTextInput += numericHandler;
            MarqueeOffTime.PreviewTextInput += numericHandler;
            BlinkOnTime.PreviewTextInput += numericHandler;
            BlinkOffTime.PreviewTextInput += numericHandler;

            // 禁止粘贴非数字内容
            var pasteHandler = new DataObjectPastingEventHandler(NumericOnly_Pasting);
            MarqueeOnTime.AddHandler(DataObject.PastingEvent, pasteHandler);
            MarqueeOffTime.AddHandler(DataObject.PastingEvent, pasteHandler);
            BlinkOnTime.AddHandler(DataObject.PastingEvent, pasteHandler);
            BlinkOffTime.AddHandler(DataObject.PastingEvent, pasteHandler);
        }

        private static readonly Regex _numericRegex = new Regex("[^0-9]+");

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numericRegex.IsMatch(e.Text);
        }

        private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (_numericRegex.IsMatch(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
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
                int.TryParse(BlinkOffTime.Text, out var blinkOff))
            {
                MarqueeOn = marqueeOn;
                MarqueeOff = marqueeOff;
                BlinkOn = blinkOn;
                BlinkOff = blinkOff;
                BackgroundOpacity = OpacitySlider.Value;
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
