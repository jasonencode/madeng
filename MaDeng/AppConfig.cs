using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MaDeng
{
    public class AppConfig
    {
        // 默认值常量
        private const int DefaultMarqueeOnTime = 500;
        private const int DefaultMarqueeOffTime = 200;
        private const int DefaultBlinkOnTime = 600;
        private const int DefaultBlinkOffTime = 400;
        private const int DefaultBreathCycleTime = 3000;
        private const double DefaultBackgroundOpacity = 0.6;

        // 有效范围
        private const int MinTimingValue = 50;   // 最小 50ms，防止除零和过快动画
        private const int MaxTimingValue = 10000; // 最大 10s

        public int MarqueeOnTime { get; set; } = DefaultMarqueeOnTime;
        public int MarqueeOffTime { get; set; } = DefaultMarqueeOffTime;
        public int BlinkOnTime { get; set; } = DefaultBlinkOnTime;
        public int BlinkOffTime { get; set; } = DefaultBlinkOffTime;
        public int BreathCycleTime { get; set; } = DefaultBreathCycleTime;
        public double BackgroundOpacity { get; set; } = DefaultBackgroundOpacity;

        private static readonly string ConfigPath = GetConfigPath();
        public static AppConfig Instance { get; private set; } = new AppConfig();

        private static string GetConfigPath()
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            var dir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(dir, "settings.json");
        }

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                    config.Validate();
                    Instance = config;
                    return config;
                }
            }
            catch (Exception ex) { Debug.WriteLine($"AppConfig.Load error: {ex.Message}"); }
            var fallback = new AppConfig();
            Instance = fallback;
            return fallback;
        }

        public void Save()
        {
            try
            {
                Validate();
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
                Instance = this;
            }
            catch (Exception ex) { Debug.WriteLine($"AppConfig.Save error: {ex.Message}"); }
        }

        /// <summary>
        /// 校验所有值在有效范围内，超出范围的重置为默认值
        /// </summary>
        private void Validate()
        {
            MarqueeOnTime = ClampTiming(MarqueeOnTime, DefaultMarqueeOnTime);
            MarqueeOffTime = ClampTiming(MarqueeOffTime, DefaultMarqueeOffTime);
            BlinkOnTime = ClampTiming(BlinkOnTime, DefaultBlinkOnTime);
            BlinkOffTime = ClampTiming(BlinkOffTime, DefaultBlinkOffTime);
            BreathCycleTime = ClampTiming(BreathCycleTime, DefaultBreathCycleTime);
            BackgroundOpacity = Math.Clamp(BackgroundOpacity, 0.05, 1.0);
        }

        private static int ClampTiming(int value, int defaultValue)
        {
            return (value >= MinTimingValue && value <= MaxTimingValue) ? value : defaultValue;
        }
    }
}
