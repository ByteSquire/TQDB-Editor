﻿using Prism.Ioc;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace TQDBEditor.Dialogs
{
    [Module(ModuleName = nameof(TQDBEditor.Dialogs), OnDemand = false)]
    public class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            ;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<NewModDialog, NewModDialogViewModel>(IDialogServiceExtensions.newMod);
            containerRegistry.RegisterDialog<InformationDialog, InformationDialogViewModel>(IDialogServiceExtensions.infoDialog);
            containerRegistry.RegisterDialogWindow<ConfirmationDialogWindow>(IDialogServiceExtensions.confirmationDialog);
            containerRegistry.RegisterDialogWindow<InformationDialogWindow>(IDialogServiceExtensions.informationDialog);
            containerRegistry.Register<IDialogService, ConfirmationDialogService>();
        }
    }
}
