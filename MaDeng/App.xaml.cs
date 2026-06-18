using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MaDeng
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (s, args) =>
            {
                Debug.WriteLine($"Unhandled exception: {args.Exception}");
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Debug.WriteLine($"Domain unhandled exception: {args.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                Debug.WriteLine($"Unobserved task exception: {args.Exception}");
                args.SetObserved();
            };
        }
    }
}
