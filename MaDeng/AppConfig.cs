using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MaDeng
{
    public class AppConfig
    {
        public int MarqueeOnTime { get; set; } = 500;
        public int MarqueeOffTime { get; set; } = 200;
        public int BlinkOnTime { get; set; } = 600;
        public int BlinkOffTime { get; set; } = 400;
        public int BreathCycleTime { get; set; } = 3000;
        public double BackgroundOpacity { get; set; } = 0.6;

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
                    Instance = config;
                    return config;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"AppConfig.Load error: {ex.Message}"); }
            var fallback = new AppConfig();
            Instance = fallback;
            return fallback;
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"AppConfig.Save error: {ex.Message}"); }
        }
    }
}
