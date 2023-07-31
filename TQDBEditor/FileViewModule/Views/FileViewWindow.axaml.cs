using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using TQDB_Parser.Blocks;
using TQDB_Parser.DBR;
using TQDBEditor.ViewModels;

namespace TQDBEditor.FileViewModule.Views
{
    public partial class FileViewWindow : Window
    {
        private readonly GroupBlock _template;
        private readonly IEnumerable<DBRFile> _files;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public FileViewWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            // Dummy for design time preview
            InitializeComponent();
        }

        public FileViewWindow(GroupBlock template, IEnumerable<DBRFile> files) : this()
        {
            _template = template;
            _files = files;
            Views.SelectionChanged += Views_SelectionChanged;
            var fileNames = files.Select(x => x.FileName).ToList();
            if (fileNames.Count > 5)
            {
                fileNames.RemoveRange(5, fileNames.Count - 5);
                fileNames.Add("...");
            }
            Title = string.Join(",", fileNames);
        }

        private void Views_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is TabItem tabItem && tabItem.Content is ContentControl content)
            {
                if (content.DataContext is FileViewModelBase viewModel)
                {
                    viewModel.InitFiles(_template, _files);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var file in _files)
            {
                file.SaveFile();
            }
            base.OnClosed(e);
        }
    }
}
