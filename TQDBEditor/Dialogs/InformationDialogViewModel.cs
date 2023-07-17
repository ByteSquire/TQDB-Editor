using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Services.Dialogs;
using System;
using TQDBEditor.Dialogs.NewMod;
using TQDBEditor.ViewModels;

namespace TQDBEditor.Dialogs
{
    public partial class InformationDialogViewModel : ViewModelBase, IDialogAware
    {
        [ObservableProperty]
        private string? _text;

        [ObservableProperty]
        private string _title = "Information!";
        string IDialogAware.Title => Title;

        public event Action<IDialogResult>? RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Text = parameters.GetInfoText();
            Title = parameters.GetInfoTitle() ?? Title;
        }
    }
}
