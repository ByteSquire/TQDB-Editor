﻿using System;
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

        IDialogParameters OnDialogConfirmed(EventArgs e);

        IDialogParameters? OnDialogCancelled(EventArgs e);
    }

    public abstract class ConfirmationDialogViewModelBase : ViewModelBase, IConfirmationDialogAware
    {
        public abstract string Title { get; }

        public abstract event Action<IDialogResult> RequestClose;

        public abstract bool CanConfirmDialog();
        public abstract IDialogParameters? OnDialogCancelled(EventArgs e);
        public abstract IDialogParameters OnDialogConfirmed(EventArgs e);
        public abstract void OnDialogOpened(IDialogParameters parameters);
    }

    public class ConfirmationDialogService : DialogService
    {
        public ConfirmationDialogService(IContainerExtension containerExtension) : base(containerExtension) { }

        protected override void ConfigureDialogWindowProperties(IDialogWindow window, Control dialogContent, IDialogAware viewModel)
        {
            base.ConfigureDialogWindowProperties(window, dialogContent, viewModel);
            if (window is IConfirmationDialogWindow confirmDialogWindow)
            {
                if (viewModel is IConfirmationDialogAware confirmDialogAware)
                {
                    confirmDialogWindow.Cancelled += (s, e) => SetResultAndClose(confirmDialogWindow, confirmDialogAware, e, true);
                    confirmDialogWindow.Confirmed += (s, e) => SetResultAndClose(confirmDialogWindow, confirmDialogAware, e, false);
                }
            }

            static void SetResultAndClose(IDialogWindow window, IConfirmationDialogAware aware, EventArgs e, bool cancelled)
            {
                if (!cancelled && !aware.CanConfirmDialog())
                    return;
                window.Result = new DialogResult(cancelled ? ButtonResult.Cancel : ButtonResult.OK, cancelled ? aware.OnDialogCancelled(e) : aware.OnDialogConfirmed(e));
                window.Close();
            }
        }
    }
}