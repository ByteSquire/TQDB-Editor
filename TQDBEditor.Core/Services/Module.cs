using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQDBEditor.Services
{
    [Module(ModuleName = nameof(TQDBEditor.Services))]
    public partial class Module : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider) { }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ConfigService.RegisterConfigService(containerRegistry);
        }
    }
}
