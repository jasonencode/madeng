using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MaDeng
{
    public class SessionInfo
    {
        public int Pid { get; set; }
        public string SessionId { get; set; } = "";
        public string Cwd { get; set; } = "";
        public string Status { get; set; } = "";
        public long UpdatedAt { get; set; }
        public long StatusUpdatedAt { get; set; }
        public string Version { get; set; } = "";
        public string Kind { get; set; } = "";
        public string Entrypoint { get; set; } = "";
        public string AgentName { get; set; } = "";
    }

    public class SessionWatcher : IDisposable
    {
        private readonly string _sessionsDir;
        private readonly string _agentName;
        private FileSystemWatcher? _watcher;
        private CancellationTokenSource? _cts;
        private readonly Dictionary<string, SessionInfo> _sessions = new();
        private readonly object _lock = new();
        private bool _disposed;
        private DateTime _lastScan = DateTime.MinValue;
        private int _scanning; // 0 = idle, 1 = scanning

        public event Action<List<SessionInfo>>? SessionsChanged;

        public SessionWatcher()
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _sessionsDir = Path.Combine(homeDir, ".claude", "sessions");

            // 从父文件夹名推导 agent 名称: ~/.claude/ → "Claude"
            var parentName = Path.GetFileName(Path.GetDirectoryName(_sessionsDir)) ?? "";
            _agentName = string.IsNullOrEmpty(parentName) ? "Agent"
                : parentName.TrimStart('.', '_').ToLower() switch
                {
                    "claude" => "Claude",
                    _ => char.ToUpper(parentName[0]) + parentName[1..]
                };
        }

        public void Start()
        {
            if (!Directory.Exists(_sessionsDir))
            {
                Directory.CreateDirectory(_sessionsDir);
            }

            _cts = new CancellationTokenSource();

            // 初始扫描
            ScanSessions();

            // 监听文件变化
            _watcher = new FileSystemWatcher(_sessionsDir, "*.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += OnFileChanged;
            _watcher.Error += OnWatcherError;
        }

        public void Stop()
        {
            _cts?.Cancel();
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnFileChanged;
                _watcher.Created -= OnFileChanged;
                _watcher.Deleted -= OnFileChanged;
                _watcher.Renamed -= OnFileChanged;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                await Task.Delay(150, _cts?.Token ?? CancellationToken.None);
            }
            catch (OperationCanceledException) { return; }

            if (_disposed) return;

            var now = DateTime.UtcNow;
            lock (_lock)
            {
                if ((now - _lastScan).TotalMilliseconds < 100) return;
                _lastScan = now;
            }

            ScanSessions();
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"FileSystemWatcher error: {e.GetException().Message}");
            // 重新扫描一次以补偿丢失的事件
            if (!_disposed) ScanSessions();
        }

        private void ScanSessions()
        {
            if (_disposed) return;
            if (Interlocked.CompareExchange(ref _scanning, 1, 0) != 0) return;
            try
            {
                var files = Directory.GetFiles(_sessionsDir, "*.json");
                var currentSessions = new Dictionary<string, SessionInfo>();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var session = JsonSerializer.Deserialize<SessionInfo>(json, options);
                        if (session != null && !string.IsNullOrEmpty(session.SessionId))
                        {
                            session.AgentName = _agentName;
                            currentSessions[session.SessionId] = session;
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Session parse error: {ex.Message}"); }
                }

                List<SessionInfo> sessionsList;
                lock (_lock)
                {
                    // 更新 sessions 字典
                    _sessions.Clear();
                    foreach (var kvp in currentSessions)
                    {
                        _sessions[kvp.Key] = kvp.Value;
                    }
                    sessionsList = _sessions.Values.ToList();
                }

                SessionsChanged?.Invoke(sessionsList);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ScanSessions error: {ex.Message}"); }
            finally { Interlocked.Exchange(ref _scanning, 0); }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            _cts?.Dispose();
        }
    }
}
