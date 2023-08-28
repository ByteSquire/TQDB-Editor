using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace TQDBEditor
{
    public static class ObservableExtensions
    {
        public static void AddPreserveNotify<T>(this ObservableCollection<T> collectionA, IEnumerable<T> collectionB)
        {
            var offset = collectionA.Count;
            collectionA.AddRange(collectionB);
            if (collectionB is INotifyCollectionChanged notifyB)
                notifyB.CollectionChanged += (_, e) => WireChangedEvent(collectionA, collectionB, offset, e);
        }

        private static void WireChangedEvent<T>(ObservableCollection<T> collectionA, IEnumerable<T> collectionB, int offset, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems is null)
                        return;
                    int i = args.NewStartingIndex > -1 ? args.NewStartingIndex : collectionB.Count();
                    collectionA.AddOrInsertRange(args.NewItems.Cast<T>(), i + offset);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oItem in args.OldItems!)
                        collectionA.Remove((T)oItem);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    collectionA.RemoveMany(collectionB);
                    collectionA.AddOrInsertRange(collectionB, offset);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    collectionA[collectionA.IndexOf((T)args.OldItems![0]!)] = (T)args.NewItems![0]!;
                    break;
                case NotifyCollectionChangedAction.Move:
                    collectionA.Move(args.OldStartingIndex + offset, args.NewStartingIndex + offset);
                    break;
                default:
                    break;
            }
        }
    }
}
