using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System.Linq;

namespace TQDBEditor.Dialogs
{
    public class MyDialogService : DialogService
    {
        public MyDialogService(IContainerExtension containerExtension) : base(containerExtension) { }

        protected override void ShowDialogWindow(IDialogWindow dialogWindow, bool isModal, Window? owner = null)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
            {
                owner = lifetime.Windows.Where(x => x.IsActive).LastOrDefault();
            }
            base.ShowDialogWindow(dialogWindow, isModal, owner);
        }
    }
}
