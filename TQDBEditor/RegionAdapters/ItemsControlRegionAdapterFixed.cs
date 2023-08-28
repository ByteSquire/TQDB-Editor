using Avalonia.Controls;
using DynamicData;
using Prism.Regions;
using System;
using TQDBEditor.AvaloniaProperties;
using System.Collections.ObjectModel;

namespace TQDBEditor.RegionAdapters
{
    public class ItemsControlRegionAdapterFixed : RegionAdapterBase<ItemsControl>
    {
        public ItemsControlRegionAdapterFixed(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory) { }

        protected override void Adapt(IRegion region, ItemsControl regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            // Avalonia needs the ItemsSource to implement IList, wrapping the IViewsCollection inside an ObservableCollection fixes this
            // Additionally, instead of replacing predefined items in the ItemsControl, they are prepended or appeneded based on the AttachedProperties.PrependProperty value
            var source = new ObservableCollection<object?>();
            var doPrepend = regionTarget.GetShouldPrepend();
            if (doPrepend)
                source.AddRange(regionTarget.Items);
            source.AddPreserveNotify(region.Views);
            if (!doPrepend)
                source.AddRange(regionTarget.Items);

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
