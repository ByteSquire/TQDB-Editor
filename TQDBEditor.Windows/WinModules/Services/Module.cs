using DryIoc;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using TQDBEditor.Views;

namespace TQDBEditor.Windows.WinModules.Services
{
    [Module(ModuleName = "TQDBEditor.WinModules.Services")]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(SetupWinTaskbarProgressHandler(containerRegistry.GetContainer()));
        }

        private static WinTaskbarProgressHandler SetupWinTaskbarProgressHandler(IResolver resolver)
        {
            return new WinTaskbarProgressHandler(resolver.Resolve<MainWindow>(), resolver.Resolve<IEventAggregator>());
        }
    }
}
