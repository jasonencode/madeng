using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace StatusLight
{
    public enum Status { Idle, Working, Waiting, Error, Completed }

    public partial class MainWindow : Window
    {
        private const int AnimationIntervalMs = 30;

        // Win32 API
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        private AppConfig _config = null!;
        private string? _terminalProgram;
        private int? _parentProcessId;

        private Status _currentStatus = Status.Idle;
        private int _marqueeIndex = 0;
        private bool _blinkState = true;
        private double _breathProgress = 0;
        private DispatcherTimer? _animationTimer;
        private HttpListener? _httpListener;
        private CancellationTokenSource? _cts;

        private readonly Color _greenOn = Color.FromRgb(0x22, 0xC5, 0x5E);
        private readonly Color _greenGlow = Color.FromRgb(0x16, 0xA3, 0x4A);
        private readonly Color _yellowOn = Color.FromRgb(0xFA, 0xCC, 0x15);
        private readonly Color _yellowGlow = Color.FromRgb(0xEA, 0xB3, 0x08);
        private readonly Color _redOn = Color.FromRgb(0xEF, 0x44, 0x44);
        private readonly Color _redGlow = Color.FromRgb(0xDC, 0x26, 0x26);
        private readonly Color _offColor = Color.FromRgb(0x1A, 0x1A, 0x2E);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _config = AppConfig.Load();
            BackgroundBrush.Opacity = _config.BackgroundOpacity;

            Width = 260;
            Height = 52;

            DetectTerminal();
            ResetPosition();
            StartHttpServer();
            StartAnimation();
            UpdateDisplay();
        }

        private void DetectTerminal()
        {
            _terminalProgram = Environment.GetEnvironmentVariable("TERM_PROGRAM");
            var ppid = Environment.GetEnvironmentVariable("PPID");

            if (int.TryParse(ppid, out var pid))
            {
                _parentProcessId = pid;
            }

            if (_parentProcessId == null)
            {
                try
                {
                    using var currentProcess = Process.GetCurrentProcess();
                    _parentProcessId = GetParentProcessId(currentProcess.Id);
                }
                catch { }
            }
        }

        private int? GetParentProcessId(int processId)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                var pbi = new PROCESS_BASIC_INFORMATION();
                int returnLength;

                if (NtQueryInformationProcess(process.Handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength) == 0)
                {
                    return (int)pbi.InheritedFromUniqueProcessId;
                }
            }
            catch { }
            return null;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION processInformation, int processInformationLength, out int returnLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
        }

        private void SwitchToTerminal()
        {
            try
            {
                if (_parentProcessId != null)
                {
                    try
                    {
                        var parentProcess = Process.GetProcessById(_parentProcessId.Value);
                        SetForegroundWindow(parentProcess.MainWindowHandle);
                        ShowWindow(parentProcess.MainWindowHandle, SW_RESTORE);
                        return;
                    }
                    catch { }
                }

                if (!string.IsNullOrEmpty(_terminalProgram))
                {
                    var processName = _terminalProgram.ToLower() switch
                    {
                        "vscode" => "Code",
                        "wt" => "WindowsTerminal",
                        "hyper" => "Hyper",
                        "jetbrains" => "idea64",
                        _ => null
                    };

                    if (processName != null)
                    {
                        var processes = Process.GetProcessesByName(processName);
                        if (processes.Length > 0)
                        {
                            SetForegroundWindow(processes[0].MainWindowHandle);
                            ShowWindow(processes[0].MainWindowHandle, SW_RESTORE);
                            return;
                        }
                    }
                }

                var cmdProcess = Process.GetProcessesByName("cmd")
                    .Concat(Process.GetProcessesByName("powershell"))
                    .Concat(Process.GetProcessesByName("WindowsTerminal"))
                    .FirstOrDefault();

                if (cmdProcess != null)
                {
                    SetForegroundWindow(cmdProcess.MainWindowHandle);
                    ShowWindow(cmdProcess.MainWindowHandle, SW_RESTORE);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法切换窗口: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ResetPosition()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            Left = (screenWidth - Width) / 2;
            Top = 0;
        }

        private void StartHttpServer()
        {
            _cts = new CancellationTokenSource();
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add($"http://127.0.0.1:{_config.Port}/");

            Task.Run(async () =>
            {
                try
                {
                    _httpListener.Start();
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        var context = await _httpListener.GetContextAsync();
                        _ = Task.Run(() => HandleRequest(context));
                    }
                }
                catch (ObjectDisposedException) { }
                catch (HttpListenerException) { }
                catch (Exception ex) { Debug.WriteLine($"HTTP server error: {ex.Message}"); }
            }, _cts.Token);
        }

        private void StopHttpServer()
        {
            _cts?.Cancel();
            try { _httpListener?.Stop(); } catch (ObjectDisposedException) { }
            try { _httpListener?.Close(); } catch (ObjectDisposedException) { }
        }

        private async void RestartHttpServer()
        {
            StopHttpServer();
            await Task.Delay(100);
            StartHttpServer();
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                    context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                    return;
                }

                if (context.Request.HttpMethod == "POST")
                {
                    using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                    var body = reader.ReadToEnd();
                    var data = JsonSerializer.Deserialize<JsonElement>(body);

                    if (data.TryGetProperty("status", out var statusProp))
                    {
                        var statusStr = statusProp.GetString()?.ToLower();
                        var status = statusStr switch
                        {
                            "idle" => Status.Idle,
                            "working" => Status.Working,
                            "waiting" => Status.Waiting,
                            "error" => Status.Error,
                            "completed" => Status.Completed,
                            _ => Status.Idle
                        };

                        // 从 hook 获取终端信息
                        if (data.TryGetProperty("terminal", out var terminalProp))
                        {
                            var terminal = terminalProp.GetString();
                            if (!string.IsNullOrEmpty(terminal))
                            {
                                _terminalProgram = terminal;
                            }
                        }

                        if (data.TryGetProperty("ppid", out var ppidProp))
                        {
                            var ppidStr = ppidProp.GetString();
                            if (int.TryParse(ppidStr, out var ppid) && ppid > 0)
                            {
                                _parentProcessId = ppid;
                            }
                        }

                        Dispatcher.Invoke(() => SetStatus(status));
                    }

                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    context.Response.StatusCode = 200;
                    var response = JsonSerializer.Serialize(new { ok = true });
                    var buffer = Encoding.UTF8.GetBytes(response);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"HandleRequest error: {ex.Message}"); }
            finally
            {
                try { context.Response.Close(); } catch { }
            }
        }

        private void SetStatus(Status status)
        {
            if (_currentStatus != status)
            {
                _currentStatus = status;
                _marqueeIndex = 0;
                _blinkState = true;
                _breathProgress = 0;
                StartAnimation();
                UpdateDisplay();
            }
        }

        private void StartAnimation()
        {
            _animationTimer?.Stop();

            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(AnimationIntervalMs);
            _animationTimer.Tick += (s, e) =>
            {
                switch (_currentStatus)
                {
                    case Status.Completed:
                        _breathProgress += (double)AnimationIntervalMs / _config.BreathCycleTime;
                        if (_breathProgress >= 1) _breathProgress = 0;
                        break;

                    case Status.Working:
                        _breathProgress += (double)AnimationIntervalMs / _config.MarqueeOnTime;
                        if (_breathProgress >= 1)
                        {
                            _breathProgress = 0;
                            _marqueeIndex = (_marqueeIndex + 1) % 3;
                        }
                        break;

                    case Status.Waiting:
                        _breathProgress += (double)AnimationIntervalMs / (_blinkState ? _config.BlinkOnTime : _config.BlinkOffTime);
                        if (_breathProgress >= 1)
                        {
                            _breathProgress = 0;
                            _blinkState = !_blinkState;
                        }
                        break;

                    case Status.Error:
                        _breathProgress += (double)AnimationIntervalMs / _config.BreathCycleTime;
                        if (_breathProgress >= 1) _breathProgress = 0;
                        break;
                }
                UpdateDisplay();
            };
            _animationTimer.Start();
        }

        private void UpdateDisplay()
        {
            switch (_currentStatus)
            {
                case Status.Idle:
                    SetLight(GreenInnerH, GreenOuterH, GreenGlowH, _greenOn, _greenGlow, true);
                    SetLight(YellowInnerH, YellowOuterH, YellowGlowH, _yellowOn, _yellowGlow, false);
                    SetLight(RedInnerH, RedOuterH, RedGlowH, _redOn, _redGlow, false);
                    StatusTextH.Text = "Ready";
                    break;

                case Status.Completed:
                    double breathIntensity = (Math.Sin(_breathProgress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
                    SetLightBreath(GreenInnerH, GreenOuterH, GreenGlowH, _greenOn, _greenGlow, breathIntensity);
                    SetLight(YellowInnerH, YellowOuterH, YellowGlowH, _yellowOn, _yellowGlow, false);
                    SetLight(RedInnerH, RedOuterH, RedGlowH, _redOn, _redGlow, false);
                    StatusTextH.Text = "Done";
                    break;

                case Status.Working:
                    // 跑马灯 + 呼吸效果
                    double marqueeIntensity = (Math.Sin(_breathProgress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
                    SetLightBreath(GreenInnerH, GreenOuterH, GreenGlowH, _greenOn, _greenGlow, _marqueeIndex == 0 ? marqueeIntensity : 0);
                    SetLightBreath(YellowInnerH, YellowOuterH, YellowGlowH, _yellowOn, _yellowGlow, _marqueeIndex == 1 ? marqueeIntensity : 0);
                    SetLightBreath(RedInnerH, RedOuterH, RedGlowH, _redOn, _redGlow, _marqueeIndex == 2 ? marqueeIntensity : 0);
                    StatusTextH.Text = "Working";
                    break;

                case Status.Waiting:
                    SetLight(GreenInnerH, GreenOuterH, GreenGlowH, _greenOn, _greenGlow, _blinkState);
                    SetLight(YellowInnerH, YellowOuterH, YellowGlowH, _yellowOn, _yellowGlow, _blinkState);
                    SetLight(RedInnerH, RedOuterH, RedGlowH, _redOn, _redGlow, _blinkState);
                    StatusTextH.Text = "Waiting";
                    break;

                case Status.Error:
                    double errorIntensity = (Math.Sin(_breathProgress * 2 * Math.PI - Math.PI / 2) + 1) / 2;
                    SetLight(GreenInnerH, GreenOuterH, GreenGlowH, _greenOn, _greenGlow, false);
                    SetLight(YellowInnerH, YellowOuterH, YellowGlowH, _yellowOn, _yellowGlow, false);
                    SetLightBreath(RedInnerH, RedOuterH, RedGlowH, _redOn, _redGlow, errorIntensity);
                    StatusTextH.Text = "Error";
                    break;
            }
        }

        private void SetLight(GradientStop inner, GradientStop outer,
            System.Windows.Media.Effects.DropShadowEffect glow,
            Color onColor, Color glowColor, bool isOn)
        {
            if (isOn)
            {
                inner.Color = onColor;
                outer.Color = glowColor;
                glow.Color = glowColor;
                glow.BlurRadius = 12;
                glow.Opacity = 0.6;
            }
            else
            {
                inner.Color = _offColor;
                outer.Color = _offColor;
                glow.BlurRadius = 0;
                glow.Opacity = 0;
            }
        }

        private void SetLightBreath(GradientStop inner, GradientStop outer,
            System.Windows.Media.Effects.DropShadowEffect glow,
            Color onColor, Color glowColor, double intensity)
        {
            double minBrightness = 0.1;
            double brightness = minBrightness + (1 - minBrightness) * intensity;

            byte r = (byte)(_offColor.R + (onColor.R - _offColor.R) * brightness);
            byte g = (byte)(_offColor.G + (onColor.G - _offColor.G) * brightness);
            byte b = (byte)(_offColor.B + (onColor.B - _offColor.B) * brightness);

            byte gr = (byte)(_offColor.R + (glowColor.R - _offColor.R) * brightness);
            byte gg = (byte)(_offColor.G + (glowColor.G - _offColor.G) * brightness);
            byte gb = (byte)(_offColor.B + (glowColor.B - _offColor.B) * brightness);

            inner.Color = Color.FromRgb(r, g, b);
            outer.Color = Color.FromRgb(gr, gg, gb);
            glow.Color = Color.FromRgb(gr, gg, gb);
            glow.BlurRadius = 4 + 16 * intensity;
            glow.Opacity = 0.1 + 0.7 * intensity;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DragMove();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SwitchToTerminal();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(
                _config.MarqueeOnTime, _config.MarqueeOffTime,
                _config.BlinkOnTime, _config.BlinkOffTime,
                _config.BackgroundOpacity, _config.Port);

            settings.Owner = this;
            if (settings.ShowDialog() == true)
            {
                _config.MarqueeOnTime = settings.MarqueeOn;
                _config.MarqueeOffTime = settings.MarqueeOff;
                _config.BlinkOnTime = settings.BlinkOn;
                _config.BlinkOffTime = settings.BlinkOff;
                _config.BackgroundOpacity = settings.BackgroundOpacity;

                BackgroundBrush.Opacity = _config.BackgroundOpacity;

                if (_config.Port != settings.Port)
                {
                    _config.Port = settings.Port;
                    RestartHttpServer();
                }

                _config.Save();
                StartAnimation();
            }
        }

        private void ResetStatus_Click(object sender, RoutedEventArgs e)
        {
            SetStatus(Status.Idle);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void ResetPosition_Click(object sender, RoutedEventArgs e)
        {
            ResetPosition();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            StopHttpServer();
            _animationTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
