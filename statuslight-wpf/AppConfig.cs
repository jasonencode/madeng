using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace StatusLight
{
    public class AppConfig
    {
        public int MarqueeOnTime { get; set; } = 500;
        public int MarqueeOffTime { get; set; } = 200;
        public int BlinkOnTime { get; set; } = 600;
        public int BlinkOffTime { get; set; } = 400;
        public int BreathCycleTime { get; set; } = 3000;
        public int Port { get; set; } = 51234;
        public double BackgroundOpacity { get; set; } = 0.6;

        private static readonly string ConfigPath = GetConfigPath();

        private static string GetConfigPath()
        {
            // 获取exe所在目录
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            var exeDir = Path.GetDirectoryName(exePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(exeDir, "settings.json");
        }

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch { }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }
    }
}
