using Avalonia;
using Prism.Modularity;
using System;

namespace TQDBEditor
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            EditorProgram.Main(BuildAvaloniaApp(), args, ConfigureModuleCatalog);
        }

        private static void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<Windows.WinModules.Services.Module>();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => EditorProgram.BuildAvaloniaApp()
                .UsePlatformDetect()
                .UseSkia()
                ;
    }
}
