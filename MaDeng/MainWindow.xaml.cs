using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MaDeng
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        private string _statusText = "Ready";
        private string _status = "idle";
        private int _pid;

        public string SessionId { get; set; } = "";
        public string Cwd { get; set; } = "";

        public int Pid
        {
            get => _pid;
            set
            {
                if (_pid != value)
                {
                    _pid = value;
                    OnPropertyChanged(nameof(Pid));
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class MainWindow : Window
    {
        private readonly SessionWatcher _sessionWatcher;
        private readonly ObservableCollection<SessionViewModel> _sessions = new();

        public MainWindow()
        {
            InitializeComponent();

            _sessionWatcher = new SessionWatcher();
            _sessionWatcher.SessionsChanged += OnSessionsChanged;

            SessionsList.ItemsSource = _sessions;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetPosition();
            _sessionWatcher.Start();
        }

        private void OnSessionsChanged(List<SessionInfo> sessions)
        {
            Dispatcher.Invoke(() =>
            {
                // 更新或添加 sessions
                var existingIds = _sessions.Select(s => s.SessionId).ToHashSet();
                var newIds = sessions.Select(s => s.SessionId).ToHashSet();

                // 移除不存在的 sessions
                var toRemove = _sessions.Where(s => !newIds.Contains(s.SessionId)).ToList();
                foreach (var item in toRemove)
                {
                    _sessions.Remove(item);
                }

                // 更新或添加 sessions
                foreach (var session in sessions)
                {
                    var existing = _sessions.FirstOrDefault(s => s.SessionId == session.SessionId);
                    if (existing != null)
                    {
                        existing.Status = session.Status;
                        existing.StatusText = GetStatusText(session.Status);
                    }
                    else
                    {
                        _sessions.Add(new SessionViewModel
                        {
                            SessionId = session.SessionId,
                            Cwd = session.Cwd,
                            Pid = session.Pid,
                            Status = session.Status,
                            StatusText = GetStatusText(session.Status)
                        });
                    }
                }

                // 调整窗口高度
                UpdateWindowHeight();
            });
        }

        private string GetStatusText(string status)
        {
            return status?.ToLower() switch
            {
                "idle" => "Ready",
                "working" => "Working",
                "completed" => "Done",
                "waiting" => "Waiting",
                "error" => "Error",
                _ => "Ready"
            };
        }

        private void UpdateWindowHeight()
        {
            // 每个 session 行高约 36px，加上边距
            double rowHeight = 36;
            double minHeight = 52;
            double maxHeight = 400;

            double newHeight = Math.Max(minHeight, Math.Min(maxHeight, _sessions.Count * rowHeight + 16));
            Height = newHeight;
        }

        private void ResetPosition()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = (screenWidth - Width) / 2;
            Top = 0;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 实现设置窗口
        }

        private void ResetStatus_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 实现重置状态
        }

        private void ResetPosition_Click(object sender, RoutedEventArgs e)
        {
            ResetPosition();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 实现关于窗口
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _sessionWatcher.Dispose();
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            _sessionWatcher.Dispose();
            base.OnClosed(e);
        }
    }
}
