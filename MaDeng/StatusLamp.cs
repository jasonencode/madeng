using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MaDeng
{
    [TemplatePart(Name = "PART_Ellipse", Type = typeof(Ellipse))]
    public class StatusLamp : Control
    {
        private Ellipse? _ellipse;
        private DispatcherTimer? _animationTimer;
        private double _breathProgress;
        private int _marqueeIndex;
        private bool _blinkState;

        private static readonly Color GreenOn = Color.FromRgb(0x22, 0xC5, 0x5E);
        private static readonly Color YellowOn = Color.FromRgb(0xFA, 0xCC, 0x15);
        private static readonly Color RedOn = Color.FromRgb(0xEF, 0x44, 0x44);
        private static readonly Color OffColor = Color.FromRgb(0x1A, 0x1A, 0x2E);

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(StatusLamp),
                new PropertyMetadata("idle", OnStatusChanged));

        public static readonly DependencyProperty LampIndexProperty =
            DependencyProperty.Register("LampIndex", typeof(int), typeof(StatusLamp),
                new PropertyMetadata(0));

        static StatusLamp()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusLamp),
                new FrameworkPropertyMetadata(typeof(StatusLamp)));
        }

        public StatusLamp()
        {
            Unloaded += (_, _) => StopAnimation();
        }

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public int LampIndex
        {
            get => (int)GetValue(LampIndexProperty);
            set => SetValue(LampIndexProperty, value);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusLamp lamp)
            {
                lamp._marqueeIndex = 0;
                lamp._blinkState = true;
                lamp._breathProgress = 0;

                if (lamp.Status == "idle")
                {
                    lamp.StopAnimation();
                }
                else
                {
                    lamp.StartAnimation();
                }
                lamp.UpdateDisplay();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _ellipse = GetTemplateChild("PART_Ellipse") as Ellipse;

            if (Status != "idle")
            {
                StartAnimation();
            }
            UpdateDisplay();
        }

        private void StartAnimation()
        {
            _animationTimer?.Stop();
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
            _animationTimer.Tick += (s, e) =>
            {
                var config = AppConfig.Instance;
                switch (Status)
                {
                    case "completed":
                        _breathProgress += 0.016 / (config.BreathCycleTime / 1000.0);
                        if (_breathProgress >= 1) _breathProgress = 0;
                        break;
                    case "working":
                    case "busy":
                        _breathProgress += 0.016 / (config.MarqueeOnTime / 1000.0);
                        if (_breathProgress >= 1)
                        {
                            _breathProgress = 0;
                            _marqueeIndex = (_marqueeIndex + 1) % 3;
                        }
                        break;
                    case "waiting":
                        _breathProgress += 0.016 / ((_blinkState ? config.BlinkOnTime : config.BlinkOffTime) / 1000.0);
                        if (_breathProgress >= 1)
                        {
                            _breathProgress = 0;
                            _blinkState = !_blinkState;
                        }
                        break;
                    case "error":
                        _breathProgress += 0.016 / (AppConfig.Instance.BreathCycleTime / 1000.0);
                        if (_breathProgress >= 1) _breathProgress = 0;
                        break;
                }
                UpdateDisplay();
            };
            _animationTimer.Start();
        }

        private void StopAnimation()
        {
            _animationTimer?.Stop();
            _animationTimer = null;
        }

        private void UpdateDisplay()
        {
            if (_ellipse == null) return;

            switch (Status)
            {
                case "idle":
                    SetLight(LampIndex == 0);
                    break;
                case "completed":
                    if (LampIndex == 0)
                    {
                        double intensity = (Math.Sin(_breathProgress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
                        SetLightBreath(GreenOn, intensity);
                    }
                    else
                    {
                        SetLight(false);
                    }
                    break;
                case "working":
                case "busy":
                    double marqueeIntensity = (Math.Sin(_breathProgress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
                    Color marqueeColor = LampIndex switch
                    {
                        0 => GreenOn,
                        1 => YellowOn,
                        2 => RedOn,
                        _ => GreenOn
                    };
                    SetLightBreath(marqueeColor, _marqueeIndex == LampIndex ? marqueeIntensity : 0);
                    break;
                case "waiting":
                    SetLight(_blinkState);
                    break;
                case "error":
                    if (LampIndex == 2)
                    {
                        double errorIntensity = (Math.Sin(_breathProgress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
                        SetLightBreath(RedOn, errorIntensity);
                    }
                    else
                    {
                        SetLight(false);
                    }
                    break;
            }
        }

        private void SetLight(bool isOn)
        {
            if (_ellipse == null) return;

            if (isOn)
            {
                Color onColor = LampIndex switch
                {
                    0 => GreenOn,
                    1 => YellowOn,
                    2 => RedOn,
                    _ => GreenOn
                };
                _ellipse.Fill = new SolidColorBrush(onColor);
                _ellipse.Effect = new DropShadowEffect
                {
                    Color = onColor,
                    BlurRadius = 12,
                    Opacity = 0.6,
                    ShadowDepth = 0
                };
            }
            else
            {
                _ellipse.Fill = new SolidColorBrush(OffColor);
                _ellipse.Effect = null;
            }
        }

        private void SetLightBreath(Color onColor, double intensity)
        {
            if (_ellipse == null) return;

            double minBrightness = 0.1;
            double brightness = minBrightness + (1 - minBrightness) * intensity;

            byte r = (byte)(OffColor.R + (onColor.R - OffColor.R) * brightness);
            byte g = (byte)(OffColor.G + (onColor.G - OffColor.G) * brightness);
            byte b = (byte)(OffColor.B + (onColor.B - OffColor.B) * brightness);

            Color color = Color.FromRgb(r, g, b);
            _ellipse.Fill = new SolidColorBrush(color);
            _ellipse.Effect = new DropShadowEffect
            {
                Color = color,
                BlurRadius = 4 + 16 * intensity,
                Opacity = 0.1 + 0.7 * intensity,
                ShadowDepth = 0
            };
        }
    }
}
