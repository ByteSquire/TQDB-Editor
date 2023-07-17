using Avalonia.Controls;
using DynamicData;
using Prism.Regions;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System;
using Avalonia;
using System.Collections.Generic;
using Avalonia.Collections;
using TQDBEditor.BasicToolbarModule.ViewModels;
using TQDBEditor.ViewModels;
using Avalonia.Styling;
using Avalonia.VisualTree;
using TQDBEditor.AvaloniaProperties;
using ImTools;

namespace TQDBEditor.RegionAdapters
{
    public class ItemsControlRegionAdapterFixed : RegionAdapterBase<ItemsControl>
    {
        public ItemsControlRegionAdapterFixed(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory) { }

        protected override void Adapt(IRegion region, ItemsControl regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            // Avalonia needs the ItemsSource to implement IList, wrapping the IViewsCollection inside an AvaloniaList fixes this
            // Additionally, instead of replacing predefined items in the ItemsControl, they are prepended or appeneded based on the AttachedProperties.PrependProperty value
            var source = new AvaloniaListWrapperCollectionChanged<IViewsCollection, object?>(region.Views, (regionTarget.GetShouldPrepend(), regionTarget.Items));

            //var source = new Test(region.Views, regionTarget.Items.ToList());

            // Avalonia needs the Items collection to be empty for ItemsSource to work
            regionTarget.Items.Clear();
            regionTarget.ItemsSource = source;
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
