using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TQDBEditor.ViewModels;

namespace TQDBEditor.ClassicViewModule.ViewModels
{
    public partial class ClassicViewViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _greeting = "Hello!";

        public ClassicViewViewModel(IContainerProvider container)
        {
            ;
        }

        public override string ToString()
        {
            return "Classic (ArtManager)";
        }
    }
}
