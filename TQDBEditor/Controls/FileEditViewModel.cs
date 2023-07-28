using CommunityToolkit.Mvvm.ComponentModel;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDB_Parser.DBR;
using TQDBEditor.Dialogs;
using TQDBEditor.ViewModels;

namespace TQDBEditor.Controls.ViewModels
{
    public partial class FileEditViewModel : ViewModelBase
    {
        private readonly DBREntry _dbrEntry;
        private readonly string? _modDir;
        private readonly IDialogService _dialogService;
        public FileEditViewModel(DBREntry dbrEntry, string? modDir, IDialogService dialogService)
        {
            _dbrEntry = dbrEntry;
            _modDir = modDir;
            _dialogService = dialogService;
            _value = _dbrEntry.Value;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Value))
            {
                _dbrEntry.UpdateValue(Value ?? string.Empty);
            }
        }

        [ObservableProperty]
        private string? _value;

        public void OnClick()
        {
            if (_modDir == null)
                return;

            _dialogService.ShowDBFilePicker(x => Value = x);
        }
    }
}
