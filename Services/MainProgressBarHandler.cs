using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQDBEditor.Services
{
    public interface IMainProgressBarHandler
    {
        void SetProgress(int value, int? maxValue);
    }

#if WINDOWS7_0_OR_GREATER
    public class MainProgressBarHandler : IMainProgressBarHandler
    {
        public void SetProgress(int value, int? maxValue)
        {
            try
            {
                Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.SetProgressValue(value, maxValue);
            }
            catch (InvalidOperationException)
            {

            }
        }
    }
#endif
}
