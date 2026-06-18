using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MaDeng
{
    public partial class SettingsWindow : Window
    {
        private const int MinTimingValue = 50;
        private const int MaxTimingValue = 10000;

        public int MarqueeOn { get; private set; }
        public int MarqueeOff { get; private set; }
        public int BlinkOn { get; private set; }
        public int BlinkOff { get; private set; }
        public double BackgroundOpacity { get; private set; }

        public SettingsWindow(AppConfig config)
        {
            InitializeComponent();

            MarqueeOnTime.Text = config.MarqueeOnTime.ToString();
            MarqueeOffTime.Text = config.MarqueeOffTime.ToString();
            BlinkOnTime.Text = config.BlinkOnTime.ToString();
            BlinkOffTime.Text = config.BlinkOffTime.ToString();

            OpacitySlider.Value = config.BackgroundOpacity;
            OpacityValue.Text = $"{(int)(config.BackgroundOpacity * 100)}%";

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

        private static readonly Regex _numericRegex = new("[^0-9]+", RegexOptions.Compiled);

        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _numericRegex.IsMatch(e.Text);
        }

        private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string?)e.DataObject.GetData(typeof(string));
                if (text == null || _numericRegex.IsMatch(text))
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
            if (!TryParseTiming(MarqueeOnTime.Text, "每个灯亮时间", out var marqueeOn)) return;
            if (!TryParseTiming(MarqueeOffTime.Text, "循环间隔", out var marqueeOff)) return;
            if (!TryParseTiming(BlinkOnTime.Text, "亮灯时间", out var blinkOn)) return;
            if (!TryParseTiming(BlinkOffTime.Text, "灭灯时间", out var blinkOff)) return;

            MarqueeOn = marqueeOn;
            MarqueeOff = marqueeOff;
            BlinkOn = blinkOn;
            BlinkOff = blinkOff;
            BackgroundOpacity = OpacitySlider.Value;
            DialogResult = true;
        }

        private bool TryParseTiming(string? text, string fieldName, out int value)
        {
            if (!int.TryParse(text, out value) || value < MinTimingValue || value > MaxTimingValue)
            {
                MessageBox.Show(
                    $"「{fieldName}」请输入 {MinTimingValue} ~ {MaxTimingValue} 之间的数字",
                    "输入无效", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
