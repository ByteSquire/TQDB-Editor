using DynamicData;
using System;
using System.Collections;
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
            {
                int i = collectionB.Count();
                notifyB.CollectionChanged += (_, e) => WireChangedEvent(collectionA, collectionB, offset, ref i, x => x, e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="collectionA"></param>
        /// <param name="collectionB"></param>
        /// <param name="mapping">Has to return the same object reference as the one present in collectionA if present</param>
        public static void AddPreserveNotifyMapping<T, U>(this ObservableCollection<T> collectionA, IEnumerable<U> collectionB, Func<U, T> mapping)
        {
            var offset = collectionA.Count;
            collectionA.AddRange(collectionB.Select(x => mapping(x)));
            if (collectionB is INotifyCollectionChanged notifyB)
            {
                int i = collectionB.Count();
                notifyB.CollectionChanged += (_, e) => WireChangedEvent(collectionA, collectionB, offset, ref i, mapping, e);
            }
        }

        private static void WireChangedEvent<T, U>(ObservableCollection<T> collectionA, IEnumerable<U> collectionB, int offset, ref int length, Func<U, T> mapping, NotifyCollectionChangedEventArgs args)
        {
            int i;
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems is null)
                        return;
                    i = args.NewStartingIndex > -1 ? args.NewStartingIndex : collectionB.Count();
                    collectionA.AddOrInsertRange(DoMap(args.NewItems), i + offset);
                    length += args.NewItems.Count;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oItem in args.OldItems!)
                        collectionA.Remove(mapping((U)oItem));
                    length -= args.OldItems.Count;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    for (i = length - 1; i >= 0; i--)
                    {
                        collectionA.RemoveAt(i + offset);
                    }
                    collectionA.AddOrInsertRange(DoMap(collectionB), offset);
                    length = collectionB.Count();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (args.OldItems is null || args.NewItems is null)
                        break;
                    for (i = 0; i < args.OldItems.Count; i++)
                    {
                        collectionA.Replace(mapping((U)args.OldItems[i]!), mapping((U)args.NewItems[i]!));
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    collectionA.Move(args.OldStartingIndex + offset, args.NewStartingIndex + offset);
                    break;
                default:
                    break;
            }

            IEnumerable<T> DoMap(IEnumerable original)
            {
                var genericOriginal = original.Cast<U>();
                foreach (var originalItem in genericOriginal)
                    yield return mapping(originalItem);
            }
        }
    }

    public abstract class ObservableCollectionWithMapping<FromType, ToType> : ObservableCollection<ToType> where FromType : notnull where ToType : notnull
    {
        protected readonly Dictionary<FromType, int> indexMap = new();

        protected void AddPreserveNotify(IEnumerable<FromType> input)
        {
            this.AddPreserveNotifyMapping(input, Mapping);
        }

        protected ToType Mapping(FromType input)
        {
            if (indexMap.TryGetValue(input, out int index))
                return this[index];
            else
                return Map(input);
        }

        protected abstract ToType Map(FromType input);
        protected abstract FromType MapBack(ToType input);

        protected override void SetItem(int index, ToType item)
        {
            base.SetItem(index, item);
            indexMap[MapBack(item)] = index;
        }

        protected override void InsertItem(int index, ToType item)
        {
            base.InsertItem(index, item);
            indexMap[MapBack(item)] = index;
        }

        protected override void RemoveItem(int index)
        {
            indexMap.Remove(MapBack(this[index]));
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            indexMap.Clear();
        }
    }

}
