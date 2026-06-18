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
        private double _colorProgress;
        private bool _blinkState;

        // 复用的渲染对象，避免每帧 GC 分配
        private readonly SolidColorBrush _brush = new();
        private readonly DropShadowEffect _effect = new();

        // 缓存的配置值，避免每帧访问静态属性
        private double _breathCycleTime;
        private double _marqueeOnTime;
        private double _blinkOnTime;
        private double _blinkOffTime;

        // working 颜色循环周期（秒）
        private const double ColorCycleTimeSec = 3.0;

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(StatusLamp),
                new PropertyMetadata("idle", OnStatusChanged));

        static StatusLamp()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusLamp),
                new FrameworkPropertyMetadata(typeof(StatusLamp)));
        }

        public StatusLamp()
        {
            Unloaded += (_, _) => StopAnimation();
            _effect.ShadowDepth = ThemeConstants.ShadowDepth;
        }

        public string Status
        {
            get => (string)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusLamp lamp)
            {
                lamp._blinkState = true;
                lamp._breathProgress = 0;
                lamp._colorProgress = 0;

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

        /// <summary>
        /// 从 AppConfig 缓存配置值，仅在状态变化时调用
        /// </summary>
        private void CacheConfig()
        {
            var config = AppConfig.Instance;
            _breathCycleTime = config.BreathCycleTime / 1000.0;
            _marqueeOnTime = config.MarqueeOnTime / 1000.0;
            _blinkOnTime = config.BlinkOnTime / 1000.0;
            _blinkOffTime = config.BlinkOffTime / 1000.0;
        }

        private void StartAnimation()
        {
            _animationTimer?.Stop();
            CacheConfig();
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(ThemeConstants.FrameIntervalMs)
            };
            _animationTimer.Tick += (_, _) =>
            {
                const double dt = ThemeConstants.FrameIntervalMs / 1000.0;
                switch (Status)
                {
                    case "completed":
                        _breathProgress += dt / _breathCycleTime;
                        if (_breathProgress >= 1) _breathProgress = 0;
                        break;
                    case "working":
                    case "busy":
                        _colorProgress += dt / ColorCycleTimeSec;
                        if (_colorProgress >= 1) _colorProgress = 0;
                        _breathProgress += dt / _marqueeOnTime;
                        if (_breathProgress >= 1) _breathProgress = 0;
                        break;
                    case "waiting":
                    {
                        double cycleTime = _blinkState ? _blinkOnTime : _blinkOffTime;
                        _breathProgress += dt / cycleTime;
                        if (_breathProgress >= 1)
                        {
                            _breathProgress = 0;
                            _blinkState = !_blinkState;
                        }
                        break;
                    }
                    case "error":
                        _breathProgress += dt / _breathCycleTime;
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

        /// <summary>
        /// 计算呼吸强度：返回 0-1 的平滑正弦值
        /// </summary>
        private static double BreathIntensity(double progress)
        {
            return (Math.Sin(progress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
        }

        /// <summary>
        /// 根据循环进度 (0-1) 返回当前颜色，蓝→青→蓝
        /// </summary>
        private (Color onColor, Color glowColor) GetCycleColors(double progress)
        {
            double t = progress < 0.5 ? progress * 2 : 2 - progress * 2;

            if (t < 0.5)
            {
                // 蓝 → 青
                double p = t * 2;
                return (LerpColor(ThemeConstants.BlueOn, ThemeConstants.CyanOn, p),
                        LerpColor(ThemeConstants.BlueGlow, ThemeConstants.CyanGlow, p));
            }
            else
            {
                // 青 → 蓝
                double p = (t - 0.5) * 2;
                return (LerpColor(ThemeConstants.CyanOn, ThemeConstants.BlueOn, p),
                        LerpColor(ThemeConstants.CyanGlow, ThemeConstants.BlueGlow, p));
            }
        }

        private static Color LerpColor(Color a, Color b, double t)
        {
            byte r = (byte)(a.R + (b.R - a.R) * t);
            byte g = (byte)(a.G + (b.G - a.G) * t);
            byte bv = (byte)(a.B + (b.B - a.B) * t);
            return Color.FromRgb(r, g, bv);
        }

        private void UpdateDisplay()
        {
            if (_ellipse == null) return;

            switch (Status)
            {
                case "idle":
                    SetLightOn(ThemeConstants.GreenOn);
                    break;
                case "completed":
                {
                    double intensity = BreathIntensity(_breathProgress);
                    SetLightBreath(ThemeConstants.GreenOn, intensity);
                    break;
                }
                case "working":
                case "busy":
                {
                    var (onColor, _) = GetCycleColors(_colorProgress);
                    double intensity = BreathIntensity(_breathProgress);
                    SetLightBreath(onColor, intensity);
                    break;
                }
                case "waiting":
                    if (_blinkState)
                        SetLightOn(ThemeConstants.YellowOn);
                    else
                        SetLightOff();
                    break;
                case "error":
                {
                    double intensity = BreathIntensity(_breathProgress);
                    SetLightBreath(ThemeConstants.RedOn, intensity);
                    break;
                }
            }
        }

        private void SetLightOn(Color onColor)
        {
            if (_ellipse == null) return;

            _brush.Color = onColor;
            _ellipse.Fill = _brush;

            _effect.Color = onColor;
            _effect.BlurRadius = ThemeConstants.ShadowBlurRadius;
            _effect.Opacity = ThemeConstants.ShadowOpacity;
            _ellipse.Effect = _effect;
        }

        private void SetLightOff()
        {
            if (_ellipse == null) return;

            _brush.Color = ThemeConstants.OffColor;
            _ellipse.Fill = _brush;
            _ellipse.Effect = null;
        }

        private void SetLightBreath(Color onColor, double intensity)
        {
            if (_ellipse == null) return;

            double brightness = ThemeConstants.BreathMinBrightness
                + (1 - ThemeConstants.BreathMinBrightness) * intensity;

            byte r = (byte)(ThemeConstants.OffColor.R + (onColor.R - ThemeConstants.OffColor.R) * brightness);
            byte g = (byte)(ThemeConstants.OffColor.G + (onColor.G - ThemeConstants.OffColor.G) * brightness);
            byte b = (byte)(ThemeConstants.OffColor.B + (onColor.B - ThemeConstants.OffColor.B) * brightness);

            Color color = Color.FromRgb(r, g, b);

            _brush.Color = color;
            _ellipse.Fill = _brush;

            _effect.Color = color;
            _effect.BlurRadius = ThemeConstants.BreathMinBlur
                + (ThemeConstants.BreathMaxBlur - ThemeConstants.BreathMinBlur) * intensity;
            _effect.Opacity = ThemeConstants.BreathMinOpacity
                + (ThemeConstants.BreathMaxOpacity - ThemeConstants.BreathMinOpacity) * intensity;
            _ellipse.Effect = _effect;
        }
    }
}
