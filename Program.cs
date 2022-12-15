using Avalonia;
using Avalonia.Logging;
using System;
using System.Diagnostics;

namespace TQDB_Editor
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                Logger.Sink?.Log(LogEventLevel.Fatal, string.Empty, e.Source, "Uncaught exception, unrecoverable error in {Source}", e.Source);
            }
            finally
            {
                Trace.Flush();
            }
        }

        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }
}
