using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDBEditor.ViewModels;
using System.IO;

namespace TQDBEditor.Dialogs
{
    public partial class NewModDialogViewModel : ConfirmationDialogViewModelBase
    {
        public override string Title => "Create a new Mod";

        public override event Action<IDialogResult>? RequestClose;

        [ObservableProperty]
        private IEnumerable<string>? _existingMods;

        [ObservableProperty]
        private string _newModName = string.Empty;

        private void Submit()
        {
            RequestClose?.Invoke(new NewModDialogResult(NewModName));
        }

        public void MyKeyDownHandler(object? sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
                Submit();
        }

        public override bool CanConfirmDialog() => CheckText(NewModName);

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            ExistingMods = parameters.GetExistingMods();
        }

        public void OnTextInput(string? text)
        {
            if (CheckText(text))
            {
                NewModName = text!;
                Submit();
            }
        }

        public bool CheckText(string? text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return (!ExistingMods?.Contains(text) ?? true) && !text.Any(x => Path.GetInvalidFileNameChars().Contains(x));
        }

        public override IDialogParameters OnDialogConfirmed(EventArgs e)
        {
            var dialogParams = new DialogParameters();
            dialogParams.AddModName(NewModName);
            return dialogParams;
        }

        public override IDialogParameters? OnDialogCancelled(EventArgs e)
        {
            return null;
        }
    }
}
