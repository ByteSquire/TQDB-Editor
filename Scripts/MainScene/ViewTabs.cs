using Godot;
using System;
using System.Linq;
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
            var orderedKeys = views.Keys.ToList();
            orderedKeys.Sort((a, b) =>
            {
                var ret = a.CompareTo(b);
                if (ret == 0)
                    return ret;

                if (a == "ArtManager")
                    return -1;
                if (b == "ArtManager")
                    return 1;

                return ret;
            });
            foreach (var key in orderedKeys)
            {
                foreach (var view in views[key])
                {
                    var child = view.Instantiate();
                    var nameExtension = string.Format(" ({0})", key);
                    child.Name += nameExtension;
                    AddChild(child);
                }
            }
        }
    }
}