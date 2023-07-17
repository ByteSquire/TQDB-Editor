using Avalonia.Controls;
using System;

namespace TQDBEditor.Controls
{
    public partial class ToolbarMenuItem : MenuItem
    {
        protected override Type StyleKeyOverride => typeof(MenuItem);
    }
}
