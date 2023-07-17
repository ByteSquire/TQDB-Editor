using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TQDBEditor.BasicToolbarModule.ViewModels;
using TQDBEditor.BasicToolbarModule.Views;
using TQDBEditor.Constants;

namespace TQDBEditor.BasicToolbarModule
{
    [Module(ModuleName = nameof(TQDBEditor.BasicToolbarModule))]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion<FileMenu>(RegionNames.ToolBarRegion);
            regionManager.RegisterViewWithRegion<EditMenu>(RegionNames.ToolBarRegion);
            regionManager.RegisterViewWithRegion<ModMenu>(RegionNames.ToolBarRegion);
            regionManager.RegisterViewWithRegion<BuildMenu>(RegionNames.ToolBarRegion);
            regionManager.RegisterViewWithRegion<DatabaseMenu>(RegionNames.ToolBarRegion);
            regionManager.RegisterViewWithRegion<ArchiveMenu>(RegionNames.ToolBarRegion);
            regionManager.RegisterViewWithRegion<ToolsMenu>(RegionNames.ToolBarRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<FileMenu, FileMenuViewModel>();
            containerRegistry.RegisterForNavigation<EditMenu, EditMenuViewModel>();
            containerRegistry.RegisterForNavigation<ModMenu, ModMenuViewModel>();
            containerRegistry.RegisterForNavigation<BuildMenu, BuildMenuViewModel>();
            containerRegistry.RegisterForNavigation<DatabaseMenu, DatabaseMenuViewModel>();
            containerRegistry.RegisterForNavigation<ArchiveMenu, ArchiveMenuViewModel>();
            containerRegistry.RegisterForNavigation<ToolsMenu, ToolsMenuViewModel>();
        }
    }
}
