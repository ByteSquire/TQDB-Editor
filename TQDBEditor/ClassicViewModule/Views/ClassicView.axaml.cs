using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using System;
using System.Diagnostics.CodeAnalysis;
using TQDBEditor.ClassicViewModule.ViewModels;

namespace TQDBEditor.ClassicViewModule.Views
{
    public partial class ClassicView : UserControl
    {
        private IFocusManager? _focusManager = null;

        public ClassicView()
        {
            InitializeComponent();
        }

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            if (TryFindParentOfType<TopLevel>(this, out var top))
                _focusManager = top.FocusManager;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (DataContext is ClassicViewViewModel viewModel)
            {
                viewModel.FileTable = FileTable;
            }
        }

        private static bool TryFindParentOfType<T>(StyledElement styledElement, [NotNullWhen(true)] out T? parent) where T : class
        {
            var tmpParent = styledElement.Parent;
            parent = null;
            while (tmpParent != null)
            {
                if (tmpParent is T typedParent)
                {
                    parent = typedParent;
                    break;
                }
                tmpParent = tmpParent.Parent;
            }
            return parent != null;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (e.Source is StyledElement styled && TryFindParentOfType<TreeDataGridCell>(styled, out _))
                return;
            FileTable.RowSelection?.BeginBatchUpdate();
            for (int i = 0; i < (FileTable.Rows?.Count ?? 0); i++)
                FileTable.RowSelection?.Deselect(i);
            FileTable.RowSelection?.EndBatchUpdate();
            _focusManager?.ClearFocus();
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (DataContext is ClassicViewViewModel viewModel)
            {
                e.Handled = true;
                switch (e.Key)
                {
                    case Key.Enter:
                        viewModel.BeginOpening();
                        break;
                    case Key.Escape:
                        FileTable.RowSelection?.BeginBatchUpdate();
                        for (int i = 0; i < (FileTable.Rows?.Count ?? 0); i++)
                            FileTable.RowSelection?.Deselect(i);
                        FileTable.RowSelection?.EndBatchUpdate();
                        break;
                    default:
                        e.Handled = false;
                        break;
                }
            }
        }
    }
}
