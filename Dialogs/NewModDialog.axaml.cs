using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Prism.Services.Dialogs;
using System.Diagnostics;

namespace TQDBEditor.Dialogs
{
    public partial class NewModDialog : UserControl
    {
        private IBrush? defaultForeground;

        public NewModDialog()
        {
            InitializeComponent();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            defaultForeground ??= Input.Foreground;
            Input.Focus();
        }

        private void inputTxt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is NewModDialogViewModel viewModel)
                if (viewModel.CheckText(Input.Text))
                    viewModel.OnTextInput(Input.Text);
        }

        private void inputTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is NewModDialogViewModel viewModel)
            {
                if (!viewModel.CheckText(Input.Text))
                    Input.Foreground = new SolidColorBrush(Colors.Red);
                else
                    Input.Foreground = defaultForeground;
            }
        }

        private void existingLst_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                Input.Text = e.AddedItems[0]?.ToString();
        }
    }
}
