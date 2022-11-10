using Godot;
using System;
using TQDBEditor.Common;

namespace TQDBEditor
{
    public partial class ViewTabs : TabContainer
    {
        private PCKHandler pckHandler;

        public override void _Ready()
        {
            var config = this.GetEditorConfig();
            if (config is null)
                return;

            config.TrulyReady += Init;
            if (config.ValidateConfig())
                Init();
        }

        private void Init()
        {
            pckHandler = this.GetPCKHandler();

            var views = pckHandler.GetViews();

            foreach (var viewTooltip in views)
            {
                foreach (var view in viewTooltip.Value)
                {
                    var child = view.Instantiate();
                    var nameExtension = string.Format(" ({0})", viewTooltip.Key);
                    child.Name += nameExtension;
                    AddChild(child);
                }
            }
        }
    }
}