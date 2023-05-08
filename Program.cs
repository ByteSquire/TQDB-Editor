using Avalonia;
using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TQDBEditor.Constants;

namespace TQDBEditor
{
    internal class Program
    {
        private const long LOG_FILE_MAX_LENGTH = 1000000;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                try
                {
                    var logFileName = nameof(TQDBEditor) + DateTime.Now.ToString("dd.MM.yy");
                    var logFileNameExFormat = logFileName + ".{0}.log";
                    logFileName += ".log";
                    int i = 1;
                    while (File.Exists(logFileName))
                    {
                        if (new FileInfo(logFileName).Length > LOG_FILE_MAX_LENGTH)
                            logFileName = string.Format(logFileNameExFormat, i++);
                        else
                            break;
                    }
                    var fileListener = new TextWriterTraceListener(logFileName)
                    {
                        Filter = new EventTypeFilter(SourceLevels.All),
                    };
                    Trace.Listeners.Add(fileListener);
                    Trace.WriteLine(DateTime.Now, "Startup");
                    AppDomain currentDomain = AppDomain.CurrentDomain;
                    currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
                    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                }
                catch (Exception e)
                {
                    LogCritical(e);
                }
                finally
                {
                    Trace.Flush();
                }
            }
            catch
            {
                throw;
            }
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            LogCritical(e);
        }

        static void LogCritical(Exception e)
        {
            if (Logger.Sink != null)
                Logger.Sink.Log(LogEventLevel.Fatal, MyLogAreas.Critial, e.Source, "Uncaught exception, unrecoverable error in {Source}:\n{Exception}", e.Source, e);
            else
                Trace.WriteLine(e, "Uncaught Exception");
            if (Environment.ExitCode == 0)
                Environment.ExitCode = 10;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions { AllowEglInitialization = true })
                .LogToTrace(LogEventLevel.Warning, MyLogAreas.AvaloniaLogAreas.Where(x => x != LogArea.Binding).Concat(MyLogAreas.All).ToArray())
                //.UseManagedSystemDialogs()
                .UseSkia();
        //.UseReactiveUI();
    }
}
