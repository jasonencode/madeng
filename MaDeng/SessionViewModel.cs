using System.ComponentModel;

namespace MaDeng
{
    public class SessionViewModel : INotifyPropertyChanged
    {
        private int _pid;
        private string _sessionId = "";
        private string _cwd = "";
        private string _status = "idle";
        private string _statusText = "Ready";
        private string _agentName = "Claude";

        public int Pid
        {
            get => _pid;
            set { if (_pid != value) { _pid = value; OnPropertyChanged(nameof(Pid)); } }
        }

        public string SessionId
        {
            get => _sessionId;
            set { if (_sessionId != value) { _sessionId = value; OnPropertyChanged(nameof(SessionId)); } }
        }

        public string Cwd
        {
            get => _cwd;
            set { if (_cwd != value) { _cwd = value; OnPropertyChanged(nameof(Cwd)); } }
        }

        public string AgentName
        {
            get => _agentName;
            set { if (_agentName != value) { _agentName = value; OnPropertyChanged(nameof(AgentName)); } }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusText = GetStatusText(value);
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            private set { if (_statusText != value) { _statusText = value; OnPropertyChanged(nameof(StatusText)); } }
        }

        public void UpdateFrom(SessionInfo session)
        {
            Pid = session.Pid;
            Cwd = session.Cwd;
            Status = session.Status;
            AgentName = session.AgentName;
        }

        public static SessionViewModel FromSession(SessionInfo session)
        {
            return new SessionViewModel
            {
                Pid = session.Pid,
                SessionId = session.SessionId,
                Cwd = session.Cwd,
                Status = session.Status,
                AgentName = session.AgentName
            };
        }

        private static string GetStatusText(string? status)
        {
            return status?.ToLower() switch
            {
                "idle" => "Ready",
                "working" => "Working",
                "busy" => "Working",
                "completed" => "Done",
                "waiting" => "Waiting",
                "error" => "Error",
                _ => status ?? "Unknown"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
