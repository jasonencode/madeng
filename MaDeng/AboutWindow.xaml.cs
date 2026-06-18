using System.Reflection;
using System.Windows;

namespace MaDeng
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionText.Text = $"v{version?.Major}.{version?.Minor}.{version?.Build}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
