using System.Windows.Media;

namespace MaDeng
{
    /// <summary>
    /// 集中管理主题颜色和动画参数常量
    /// </summary>
    public static class ThemeConstants
    {
        // ========== 背景色 ==========
        public static readonly Color BackgroundColor = Color.FromRgb(0x1E, 0x1E, 0x2E);

        // ========== 灯颜色 ==========
        public static readonly Color GreenOn = Color.FromRgb(0x22, 0xC5, 0x5E);
        public static readonly Color GreenGlow = Color.FromRgb(0x16, 0xA3, 0x4A);
        public static readonly Color YellowOn = Color.FromRgb(0xFA, 0xCC, 0x15);
        public static readonly Color YellowGlow = Color.FromRgb(0xEA, 0xB3, 0x08);
        public static readonly Color RedOn = Color.FromRgb(0xEF, 0x44, 0x44);
        public static readonly Color RedGlow = Color.FromRgb(0xDC, 0x26, 0x26);
        public static readonly Color BlueOn = Color.FromRgb(0x0E, 0xA5, 0xE9);   // 科技蓝
        public static readonly Color BlueGlow = Color.FromRgb(0x02, 0x84, 0xC7); // 科技蓝发光
        public static readonly Color CyanOn = Color.FromRgb(0x06, 0xB6, 0xD4);   // 青色
        public static readonly Color CyanGlow = Color.FromRgb(0x08, 0x91, 0xB2); // 青色发光
        public static readonly Color OffColor = Color.FromRgb(0x1A, 0x1A, 0x2E);

        // ========== 阴影参数 ==========
        public const double ShadowBlurRadius = 12;
        public const double ShadowOpacity = 0.6;
        public const double ShadowDepth = 0;
        public const double BreathMinBlur = 4;
        public const double BreathMaxBlur = 20;
        public const double BreathMinOpacity = 0.1;
        public const double BreathMaxOpacity = 0.8;

        // ========== 动画参数 ==========
        /// <summary>动画帧间隔（毫秒），约60fps</summary>
        public const double FrameIntervalMs = 16;
        /// <summary>呼吸亮度最小值</summary>
        public const double BreathMinBrightness = 0.1;
    }
}
