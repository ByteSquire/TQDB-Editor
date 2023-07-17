using Avalonia.Controls;
using Prism.DryIoc.Properties;
using Prism.Regions;
using System.Collections.Specialized;
using System.Linq;
using System;
using System.Drawing;
using System.Collections;
using Avalonia.Collections;
using Avalonia;
using DynamicData;

namespace TQDBEditor.RegionAdapters
{
    public class TabControlRegionAdapter : RegionAdapterBase<TabControl>
    {
        public TabControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, TabControl regionTarget)
        {
            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            bool contentIsSet = regionTarget.SelectedItem != null;
            contentIsSet = contentIsSet || regionTarget[ContentControl.ContentProperty] != null;

            if (contentIsSet)
                throw new InvalidOperationException("TabControl already has Content selected!");

            region.ActiveViews.CollectionChanged += delegate
            {
                if (region.ActiveViews.Any())
                    regionTarget.SelectedIndex = region.Views.IndexOf(region.ActiveViews.FirstOrDefault());
            };

            //regionTarget.ItemsSource ??= new AvaloniaList<object>();

            region.Views.CollectionChanged +=
                (sender, e) =>
                {
                    var itemsList = (regionTarget.Items! as IList)!;
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            //if (!region.ActiveViews.Any())
                            //    region.Activate(e.NewItems![0]);
                            AddTabItemsToList(itemsList, e.NewItems!);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            RemoveTabItemsFromList(itemsList, e.OldItems!);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                        case NotifyCollectionChangedAction.Move:
                        case NotifyCollectionChangedAction.Reset:
                            if (e.OldItems != null)
                                RemoveTabItemsFromList(itemsList, e.OldItems);
                            if (e.NewItems != null)
                                AddTabItemsToList(itemsList, e.NewItems);
                            break;
                        default:
                            break;
                    }
                };

            regionTarget.SelectionChanged +=
                (sender, e) =>
                {
                    if (sender != e.Source)
                        return;
                    if (e.RemovedItems.Count > 0)
                        foreach (TabItem item in e.RemovedItems)
                            region.Deactivate(item.Content);
                    if (e.AddedItems.Count > 0)
                        foreach (TabItem item in e.AddedItems)
                            region.Activate(item.Content);
                };

            static void AddTabItemsToList(IList list, IList added)
            {
                foreach (var view in added)
                {
                    var tabItem = CreateTabItem(view);
                    list.Add(tabItem);
                }
            }

            static TabItem CreateTabItem(object? view)
            {
                return new TabItem { Content = view, Header = (view as Visual)?.DataContext?.ToString() };
            }

            static void RemoveTabItemsFromList(IList list, IList removed)
            {
                foreach (var view in removed)
                {
                    var tabItem = list.OfType<TabItem>().FirstOrDefault(ti => ti!.Content == view, null);
                    if (tabItem != null)
                    {
                        if (tabItem.IsSelected)
                        {
                            var nextTabItem = list.OfType<TabItem>().FirstOrDefault(ti => ti != tabItem, null);
                            if (nextTabItem != null)
                            {
                                nextTabItem.IsSelected = true;
                            }
                        }

                        list.Remove(tabItem);
                    }
                }
            }
        }

        protected override IRegion CreateRegion()
        {
            return new SingleActiveRegion();
        }
    }
}
