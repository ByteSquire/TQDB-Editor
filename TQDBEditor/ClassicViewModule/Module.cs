using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TQDBEditor.Constants;
using TQDBEditor.ClassicViewModule.ViewModels;
using TQDBEditor.ClassicViewModule.Views;

namespace TQDBEditor.ClassicViewModule
{
    [Module(ModuleName = nameof(TQDBEditor.ClassicViewModule))]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion<ClassicView>(RegionNames.ViewsRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ClassicView, ClassicViewViewModel>();
        }
    }
}
