using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using TQDBEditor.Constants;
using TQDBEditor.FileViewModule.Views;
using TQDBEditor.FileViewModule.ViewModels;
using TQDBEditor.FileViewModule.Dialogs;
using TQDBEditor.Dialogs;

namespace TQDBEditor.FileViewModule
{
    [Module(ModuleName = nameof(TQDBEditor.FileViewModule))]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _ = containerProvider.Resolve<DBRAccessHandler>();
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion<ClassicFileView>(RegionNames.FileViewsRegion);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<DBRAccessHandler>();
            containerRegistry.RegisterForNavigation<ClassicFileView, ClassicFileViewViewModel>();

            containerRegistry.RegisterDialog<ArrayEditDialog, ArrayEditDialogViewModel>(IDialogServiceExtensions.arrayEdit);
            containerRegistry.RegisterDialog<EquationEditDialog, EquationEditDialogViewModel>(IDialogServiceExtensions.equationEdit);
            containerRegistry.RegisterDialog<DBFilePicker, DBFilePickerViewModel>(IDialogServiceExtensions.databaseFilePicker);
        }
    }
}
