using Avalonia.Collections;
using DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using static ImTools.ImMap;

namespace TQDBEditor
{
    public class AvaloniaListWrapperCollectionChanged<T, U> : AvaloniaList<U> where T : INotifyCollectionChanged, IEnumerable<U>
    {
        private readonly IReadOnlyList<U> _prepends;
        private readonly IReadOnlyList<U> _appends;
        public AvaloniaListWrapperCollectionChanged(T baseCollection, params (bool prepend, IEnumerable<U>)[] additional) : base(baseCollection.Count() + additional.Sum(x => x.Item2.Count()))
        {
            _prepends = new List<U>(additional.Where(x => x.prepend).SelectMany(x => x.Item2));
            _appends = new List<U>(additional.Where(x => !x.prepend).SelectMany(x => x.Item2));
            AddRange(PreOrAppendCached(baseCollection));
            baseCollection.CollectionChanged += BaseCollection_CollectionChanged;
        }

        private IEnumerable<U> PreOrAppendCached(IEnumerable<U> original)
        {
            return _prepends.Concat(original).Concat(_appends);
        }

        private void BaseCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (args.NewItems is null)
                        return;
                    int i = args.NewStartingIndex > -1 ? args.NewStartingIndex : Count;
                    InsertRange(i, args.NewItems.Cast<U>());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oItem in args.OldItems!)
                        Remove((U)oItem);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    AddRange(PreOrAppendCached((IEnumerable<U>)sender!));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this[IndexOf((U)args.OldItems![0]!)] = (U)args.NewItems![0]!;
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                    break;
                default:
                    break;
            }
        }
    }
    #region Different approach
    /*
    public class EnumerableToListWrapper : IList, INotifyCollectionChanged
    {
        public object? this[int index] { get => _list[index]; set => _list[index] = value; }

        public bool IsFixedSize => _list.IsFixedSize;

        public bool IsReadOnly => _list.IsReadOnly;

        public int Count => _list.Count;

        public bool IsSynchronized => _list.IsSynchronized;

        public object SyncRoot => _list.SyncRoot;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly IList _list;

        public EnumerableToListWrapper(IEnumerable original)
        {
            if (original is null)
                throw new ArgumentNullException(nameof(original));
            if (original is IList list)
                _list = list;
            else
            {
                _list = new List<object?>();
                CopyToInner(original);
                if (original is INotifyCollectionChanged obs)
                    obs.CollectionChanged += InnerChanged;
            }

            if (original is INotifyCollectionChanged observable)
                observable.CollectionChanged += CollectionChanged;
        }

        private void CopyToInner(IEnumerable source)
        {
            foreach (var obj in source)
                _list.Add(obj);
        }

        private void InnerChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _list.Clear();
            CopyToInner((IEnumerable)sender!);
        }

        public int Add(object? value)
        {
            return _list.Add(value);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(object? value)
        {
            return _list.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(object? value)
        {
            return _list.IndexOf(value);
        }

        public void Insert(int index, object? value)
        {
            _list.Insert(index, value);
        }

        public void Remove(object? value)
        {
            _list.Remove(value);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
    }

    public class CollectionChangedAggregator : IList, INotifyCollectionChanged
    {
        public object? this[int index]
        {
            get
            {
                int i = 0;
                IList curr;
                while (index >= (curr = _lists[i++]).Count)
                {
                    index -= curr.Count;
                }
                if (i >= _lists.Length)
                    return new IndexOutOfRangeException();
                return curr[index];
            }
            set => Insert(index, value);
        }

        public bool IsFixedSize => false;

        public bool IsReadOnly => true;

        public int Count => _lists.Sum(x => x.Count);

        public bool IsSynchronized => false;

        private readonly static object _syncRoot = new();
        public object SyncRoot => _syncRoot;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly IList[] _lists;

        public CollectionChangedAggregator(params IEnumerable[] lists)
        {
            if (lists.Length == 0)
                throw new ArgumentException("Must pass at least one list!", nameof(lists));
            _lists = lists.Select(x => new EnumerableToListWrapper(x)).ToArray();
            foreach (var list in lists)
            {
                if (list is INotifyCollectionChanged observable)
                    observable.CollectionChanged += Observable_CollectionChanged;
            }
        }

        private void Observable_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //var listIndex = _lists.IndexOf(sender);
            //CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, e.NewStartingIndex));
            CollectionChanged?.Invoke(sender, e);
        }

        public int Add(object? value)
        {
            Insert(Count, value);
            return Count;
        }

        public void Clear()
        {
            throw new InvalidOperationException();
        }

        public bool Contains(object? value)
        {
            return IndexOf(value) >= 0;
        }

        public void CopyTo(Array array, int index)
        {
            int nIndex = index;
            foreach (var list in _lists)
            {
                list.CopyTo(array, nIndex);
                nIndex += list.Count;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _lists.SelectMany(x => x.Cast<object?>()).GetEnumerator();
        }

        public int IndexOf(object? value)
        {
            int offset = 0;
            foreach (var list in _lists)
            {
                int index = list.IndexOf(value);
                if (index >= 0)
                    return offset + index;
                offset += list.Count;
            }
            return -1;
        }

        public void Insert(int index, object? value)
        {
            throw new InvalidOperationException();
        }

        public void Remove(object? value)
        {
            RemoveAt(IndexOf(value));
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }
    }
    */
    #endregion
}
