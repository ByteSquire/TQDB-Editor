using Avalonia;
using Prism.DryIoc;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Prism.Ioc;
using Microsoft.Extensions.Logging;
using Avalonia.Logging;
using Microsoft.Extensions.Logging.TraceSource;
using System.Diagnostics;

namespace TQDB_Editor
{
    public partial class App : PrismApplication
    {
        private readonly ILogger? _logger;

        public App() : base()
        {
            var fileListener = new TextWriterTraceListener("app.log")
            {
                Filter = new EventTypeFilter(SourceLevels.All)
            };
            Trace.Listeners.Add(fileListener);
            var sourceSwitch = new SourceSwitch("sourceSwitch", "Error")
            {
                Level = SourceLevels.All
            };
            _logger = new TraceSourceLoggerProvider(sourceSwitch, fileListener).CreateLogger("TQDB_Editor");
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // Initializes Prism.Avalonia
            base.Initialize();
        }

        /// <summary>Register Services and Views.</summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Main shell window
            containerRegistry.Register<MainWindow>();

            if (_logger != null)
                containerRegistry.RegisterSingleton<ILogger>(() => _logger);
        }

        /// <summary>User interface entry point, called after Register and ConfigureModules.</summary>
        /// <returns>Startup View.</returns>
        protected override IAvaloniaObject CreateShell()
        {
            // Input your main shell window name
            return Container.Resolve<MainWindow>();
        }
    }
}
