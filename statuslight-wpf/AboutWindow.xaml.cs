using System.Windows;

namespace StatusLight
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            SetHooksText();
        }

        private void SetHooksText()
        {
            HooksTextBox.Text = @"{
  ""hooks"": {
    ""SessionStart"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""idle\""}'""
      }]
    }],
    ""UserPromptSubmit"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""working\""}'""
      }]
    }],
    ""Stop"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""completed\""}'""
      }]
    }],
    ""StopFailure"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""error\""}'""
      }]
    }],
    ""PermissionRequest"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""waiting\""}'""
      }]
    }],
    ""PostToolUse"": [{
      ""matcher"": """",
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""working\""}'""
      }]
    }],
    ""PostToolUseFailure"": [{
      ""matcher"": """",
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""error\""}'""
      }]
    }],
    ""PermissionDenied"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""idle\""}'""
      }]
    }],
    ""SessionEnd"": [{
      ""hooks"": [{
        ""type"": ""command"",
        ""command"": ""curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\""status\"": \""idle\""}'""
      }]
    }]
  }
}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
