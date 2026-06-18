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
    }

    public class SessionWatcher : IDisposable
    {
        private readonly string _sessionsDir;
        private FileSystemWatcher? _watcher;
        private CancellationTokenSource? _cts;
        private readonly Dictionary<string, SessionInfo> _sessions = new();
        private readonly object _lock = new();

        public event Action<List<SessionInfo>>? SessionsChanged;

        public SessionWatcher()
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _sessionsDir = Path.Combine(homeDir, ".claude", "sessions");
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
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(100, _cts?.Token ?? CancellationToken.None).ContinueWith(_ => ScanSessions());
        }

        private void ScanSessions()
        {
            try
            {
                var files = Directory.GetFiles(_sessionsDir, "*.json");
                var currentSessions = new Dictionary<string, SessionInfo>();

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var session = JsonSerializer.Deserialize<SessionInfo>(json);
                        if (session != null && !string.IsNullOrEmpty(session.SessionId))
                        {
                            currentSessions[session.SessionId] = session;
                        }
                    }
                    catch { }
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
            catch { }
        }

        public List<SessionInfo> GetSessions()
        {
            lock (_lock)
            {
                return _sessions.Values.ToList();
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
