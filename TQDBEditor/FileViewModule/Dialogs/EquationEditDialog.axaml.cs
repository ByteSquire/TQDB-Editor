using Avalonia.Controls;
using Avalonia.Interactivity;
using TQDBEditor.FileViewModule.Dialogs.ViewModels;

namespace TQDBEditor.FileViewModule.Dialogs
{
    public partial class EquationEditDialog : UserControl
    {
        public EquationEditDialog()
        {
            InitializeComponent();
        }

        public void VariableSelected(object? sender, RoutedEventArgs eventArgs)
        {
            if (DataContext is EquationEditDialogViewModel viewModel && sender is Button btn)
            {
                var variable = btn.Content?.ToString();
                if (variable == null)
                    return;

                viewModel.AddVariable(EquationText.CaretIndex, variable);
                EquationText.CaretIndex += variable.Length;
                EquationText.Focus();
            }
        }
    }
}
