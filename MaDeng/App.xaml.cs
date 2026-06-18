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
                // 仅处理非致命异常，严重异常仍然终止
                if (args.Exception is not (OutOfMemoryException or StackOverflowException or AccessViolationException))
                {
                    args.Handled = true;
                }
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                Debug.WriteLine($"Domain unhandled exception: {args.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                Debug.WriteLine($"Unobserved task exception: {args.Exception}");
            };
        }
    }
}
