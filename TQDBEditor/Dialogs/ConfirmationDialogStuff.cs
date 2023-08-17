using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Prism.Common;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using TQDBEditor.ViewModels;

namespace TQDBEditor.Dialogs
{
    public interface IConfirmationDialogWindow : IDialogWindow
    {
        event EventHandler? Confirmed;
        event EventHandler? Cancelled;
    }

    public interface IConfirmationDialogAware : IDialogAware
    {
        bool IDialogAware.CanCloseDialog() => true;

        void IDialogAware.OnDialogClosed() { }

        bool CanConfirmDialog();

        IDialogParameters? OnDialogConfirmed(EventArgs e);

        IDialogParameters? OnDialogCancelled(EventArgs e);
    }

    public abstract class ConfirmationDialogViewModelBase : ViewModelBase, IConfirmationDialogAware
    {
        public abstract string Title { get; }

        public abstract event Action<IDialogResult>? RequestClose;

        public abstract bool CanConfirmDialog();
        public abstract IDialogParameters? OnDialogCancelled(EventArgs e);
        public abstract IDialogParameters? OnDialogConfirmed(EventArgs e);
        public abstract void OnDialogOpened(IDialogParameters parameters);
    }

    public class ConfirmationDialogService : MyDialogService
    {
        public ConfirmationDialogService(IContainerExtension containerExtension) : base(containerExtension) { }

        protected override void ConfigureDialogWindowProperties(IDialogWindow dialogWindow, Control dialogContent, IDialogAware viewModel)
        {
            base.ConfigureDialogWindowProperties(dialogWindow, dialogContent, viewModel);
            if (dialogWindow is IConfirmationDialogWindow confirmDialogWindow &&
                viewModel is IConfirmationDialogAware confirmDialogAware)
            {
                EventHandler? closedHandler = null, cancelledHandler = null, confirmedHandler = null;
                confirmedHandler = delegate (object? sender, EventArgs e)
                {
                    if (!confirmDialogAware.CanConfirmDialog())
                        return;
                    CloseDialog(ButtonResult.OK, confirmDialogAware.OnDialogConfirmed(e));
                };
                cancelledHandler = delegate (object? sender, EventArgs e)
                {
                    CloseDialog(ButtonResult.Cancel, confirmDialogAware.OnDialogCancelled(e));
                };
                closedHandler = delegate
                {
                    confirmDialogWindow.Confirmed -= confirmedHandler;
                    confirmDialogWindow.Cancelled -= cancelledHandler;
                    dialogWindow.Closed -= closedHandler;
                };
                confirmDialogWindow.Confirmed += confirmedHandler;
                confirmDialogWindow.Cancelled += cancelledHandler;
                dialogWindow.Closed += closedHandler;
            }

            void CloseDialog(ButtonResult buttonResult, IDialogParameters? parameters)
            {
                dialogWindow.Result = new DialogResult(buttonResult, parameters);
                dialogWindow.Close();
            }
        }
    }
}
