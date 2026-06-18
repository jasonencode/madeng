using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MaDeng
{
    public partial class MainWindow : Window
    {
        private readonly SessionWatcher _sessionWatcher;
        private readonly ObservableCollection<SessionViewModel> _sessions = new();
        private AppConfig _config = null!;

        public MainWindow()
        {
            InitializeComponent();
            SessionsList.ItemsSource = _sessions;

            _sessionWatcher = new SessionWatcher();
            _sessionWatcher.SessionsChanged += OnSessionsChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _config = AppConfig.Load();
            ApplyBackgroundOpacity(_config.BackgroundOpacity);
            ResetPosition();
            _sessionWatcher.Start();
        }

        private void OnSessionsChanged(List<SessionInfo> sessions)
        {
            Dispatcher.BeginInvoke(() => UpdateSessions(sessions));
        }

        private void UpdateSessions(List<SessionInfo> sessions)
        {
            var currentIds = sessions.Select(s => s.SessionId).ToHashSet();

            // 移除不存在的 sessions
            var toRemove = _sessions.Where(s => !currentIds.Contains(s.SessionId)).ToList();
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
                    existing.UpdateFrom(session);
                }
                else
                {
                    _sessions.Add(SessionViewModel.FromSession(session));
                }
            }
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
            var settings = new SettingsWindow(
                _config.MarqueeOnTime, _config.MarqueeOffTime,
                _config.BlinkOnTime, _config.BlinkOffTime,
                _config.BackgroundOpacity);

            settings.Owner = this;
            if (settings.ShowDialog() == true)
            {
                _config.MarqueeOnTime = settings.MarqueeOn;
                _config.MarqueeOffTime = settings.MarqueeOff;
                _config.BlinkOnTime = settings.BlinkOn;
                _config.BlinkOffTime = settings.BlinkOff;
                _config.BackgroundOpacity = settings.BackgroundOpacity;

                ApplyBackgroundOpacity(_config.BackgroundOpacity);
                _config.Save();
            }
        }

        private void ResetPosition_Click(object sender, RoutedEventArgs e)
        {
            ResetPosition();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ApplyBackgroundOpacity(double opacity)
        {
            var alpha = (byte)(opacity * 255);
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(alpha, 0x1E, 0x1E, 0x2E));
        }

        private void OpenCwd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SessionViewModel vm && !string.IsNullOrEmpty(vm.Cwd))
            {
                try
                {
                    Process.Start("explorer.exe", vm.Cwd);
                }
                catch { }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _sessionWatcher.Dispose();
            base.OnClosed(e);
        }
    }
}
