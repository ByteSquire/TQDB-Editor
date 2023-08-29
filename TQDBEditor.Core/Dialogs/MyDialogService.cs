using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Linq;

namespace TQDBEditor.Dialogs
{
    public class MyDialogService : DialogService
    {
        public MyDialogService(IContainerExtension containerExtension) : base(containerExtension) { }

        protected override void ShowDialogWindow(IDialogWindow dialogWindow, bool isModal, Window? owner = null)
        {
            if (owner == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                owner = lifetime.Windows.Where(x => x.IsActive).LastOrDefault();
            }
            if (!isModal && owner != null)
            {
                EventHandler? closed = null;
                closed = (_, _) => { dialogWindow.Close(); owner.Closed -= closed; };
                owner.Closed += closed;
            }
            base.ShowDialogWindow(dialogWindow, isModal, owner);
        }
    }
}
