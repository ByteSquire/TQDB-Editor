using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;

namespace TQDBEditor.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
    }

    public abstract partial class FileViewModelBase : ViewModelBase
    {
        public abstract void InitFiles(GroupBlock template, IEnumerable<DBRFile> files);
    }
}
