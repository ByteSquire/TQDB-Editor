using Avalonia;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.TraceSource;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.Linq;
using TQDBEditor.ViewModels;
using TQDBEditor.Views;
using TQDBEditor.Constants;
using Avalonia.Controls;
using TQDBEditor.RegionAdapters;
using DryIoc;
using System.Reflection;
using System.IO;
using Avalonia.Logging;
using TQDBEditor.Services;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Styling;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Prism.Events;
using Avalonia.Platform.Storage;
using Avalonia.Input.Platform;
using Avalonia.Platform;

namespace TQDBEditor
{
    public partial class App : PrismApplication
    {
        public static bool IsSingleViewLifetime =>
        Environment.GetCommandLineArgs()
            .Any(a => a == "--fbdev" || a == "--drm");

        public static SynchronizationContext? MainThreadContext { get; private set; }

        private bool _areModulesInitialized = false;

        private readonly ILoggerProvider _loggerProvider;
        public App() : base()
        {
            var sourceSwitch = new SourceSwitch("sourceSwitch")
            {
                Level =
#if DEBUG
                SourceLevels.All
#else
                SourceLevels.Information
#endif
            };
            _loggerProvider = new TraceSourceLoggerProvider(sourceSwitch, EditorProgram.LogFileListener);
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            //base.ConfigureRegionAdapterMappings(regionAdapterMappings);

            regionAdapterMappings.RegisterMapping<ContentControl, ContentControlRegionAdapter>();
            regionAdapterMappings.RegisterMapping<ItemsControl, ItemsControlRegionAdapterFixed>();
            regionAdapterMappings.RegisterMapping<TabControl, TabControlRegionAdapter>();
        }

        public override void Initialize()
        {
            MainThreadContext = SynchronizationContext.Current;
            AvaloniaXamlLoader.Load(this);
            base.Initialize();              // <-- Required
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register Services
            //containerRegistry.Register<IRestService, RestService>();
            if (_loggerProvider != null)
                containerRegistry.RegisterInstance(_loggerProvider);

            //// Views - Singleton
            var mainWindow = new MainWindow();
            containerRegistry.RegisterInstance(mainWindow);

            //// Avalonia services
            containerRegistry.RegisterInstance(mainWindow.StorageProvider);
            containerRegistry.RegisterInstance(mainWindow.Clipboard);
            containerRegistry.RegisterInstance(mainWindow.PlatformSettings);
            //// Views - Region Navigation
            //containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
            //containerRegistry.RegisterForNavigation<SidebarView, SidebarViewModel>();
        }

        protected override DryIoc.Rules CreateContainerRules()
        {
            return base.CreateContainerRules();
        }

        protected override AvaloniaObject CreateShell()
        {
            InitializeModules();
            //if (IsSingleViewLifetime)
            //return Container.Resolve<MainControl>(); // For Linux Framebuffer or DRM
            //else
            return Container.Resolve<MainWindow>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            // Register core modules
            moduleCatalog.AddModule<Services.Module>();

            // Register modules
            moduleCatalog.AddModule<ClassicViewModule.Module>();
            moduleCatalog.AddModule<Dialogs.Module>();
            moduleCatalog.AddModule<BasicToolbarModule.Module>();
            moduleCatalog.AddModule<FileViewModule.Module>();
            EditorProgram.ConfigureAdditionalModules(moduleCatalog);

            Container.Resolve<IModuleManager>().LoadModuleCompleted += App_LoadModuleCompleted;
        }

        protected override void InitializeModules()
        {
            if (_areModulesInitialized)
                return;
            _areModulesInitialized = true;
            try
            {
                base.InitializeModules();
            }
            catch (ModularityException ex)
            {
                Logger.Sink?.Log(LogEventLevel.Warning, MyLogAreas.Initialization, ex.Source, "Failed to load module {ModuleName}, Reason:\n{Exception}", ex.ModuleName, ex);
            }
        }

        private void App_LoadModuleCompleted(object? sender, LoadModuleCompletedEventArgs e)
        {
            if (e.Error != null && !e.IsErrorHandled)
            {
                Logger.Sink?.Log(LogEventLevel.Warning, MyLogAreas.Initialization, sender, "Failed to load module {ModuleName}, Reason:\n{Exception}", e.ModuleInfo.ModuleName, e.Error);
                e.IsErrorHandled = true;
            }
            else
                Logger.Sink?.Log(LogEventLevel.Information, MyLogAreas.Initialization, sender, "Module {ModuleName} loaded successfully!", e.ModuleInfo.ModuleName);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            try
            {
                if (!Directory.Exists(Modules.ModulesPath))
                    Directory.CreateDirectory(Modules.ModulesPath);
                return new MyDirectoryModuleCatalog() { ModulePath = Modules.ModulesPath };
            }
            catch (Exception ex)
            {
                Logger.Sink?.Log(LogEventLevel.Error, MyLogAreas.Initialization, ex.Source, "Error setting up the DirectoryModuleCatalog: {Exception}", ex);
                return base.CreateModuleCatalog();
            }
        }

        /// <summary>Called after <seealso cref="Initialize"/>.</summary>
        protected override void OnInitialized()
        {
            // Register initial Views to Region.
            var regionManager = Container.Resolve<IRegionManager>();

            var dialogService = Container.Resolve<IDialogService>();
            //regionManager.RegisterViewWithRegion(RegionNames.ContentRegion, typeof(DashboardView));
            //regionManager.RegisterViewWithRegion(RegionNames.SidebarRegion, typeof(SidebarView));
        }

        class MyDirectoryModuleCatalog : DirectoryModuleCatalog
        {
            public override IModuleCatalog AddModule(IModuleInfo moduleInfo)
            {
                var type = Type.GetType(moduleInfo.ModuleType);
                string moduleName = type!.Name;
                var dependsOn = new List<string>();
                bool onDemand = false;
                var moduleAttribute =
                    CustomAttributeData.GetCustomAttributes(type).FirstOrDefault(
                        cad => cad.Constructor.DeclaringType?.FullName == typeof(ModuleAttribute).FullName);

                if (moduleAttribute != null)
                {
                    foreach (CustomAttributeNamedArgument argument in moduleAttribute.NamedArguments)
                    {
                        string argumentName = argument.MemberInfo.Name;
                        switch (argumentName)
                        {
                            case "ModuleName":
                                moduleName = (string?)argument.TypedValue.Value ?? moduleName;
                                break;

                            case "OnDemand":
                                onDemand = (bool?)argument.TypedValue.Value ?? onDemand;
                                break;

                            case "StartupLoaded":
                                onDemand = !((bool?)argument.TypedValue.Value) ?? onDemand;
                                break;
                        }
                    }
                }

                var moduleDependencyAttributes =
                    CustomAttributeData.GetCustomAttributes(type).Where(
                        cad => cad.Constructor.DeclaringType?.FullName == typeof(ModuleDependencyAttribute).FullName);

                foreach (CustomAttributeData cad in moduleDependencyAttributes)
                {
                    string? value;
                    if ((value = (string?)cad.ConstructorArguments[0].Value) != null)
                        dependsOn.Add(value);
                }

                var moduleInfo1 = new ModuleInfo(moduleName, type.AssemblyQualifiedName)
                {
                    InitializationMode = onDemand ? InitializationMode.OnDemand : InitializationMode.WhenAvailable,
                    Ref = type.Assembly.Location,
                };
                moduleInfo1.DependsOn.AddRange(dependsOn);

                return base.AddModule(moduleInfo1);
            }
        }
    }
}
