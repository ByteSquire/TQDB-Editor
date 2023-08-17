using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using TQDB_Parser.DBR;

namespace TQDBEditor
{
    public class ObservableEntry : INotifyPropertyChanged
    {
        private readonly DBREntry _entry;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Value => _entry.Value;

        public ObservableEntry(DBREntry entry)
        {
            _entry = entry;
        }

        public void UpdateValue(string value)
        {
            _entry.UpdateValue(value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }

        public static explicit operator DBREntry(ObservableEntry entry) { return entry._entry; }
    }
}
