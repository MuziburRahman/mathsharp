using System;
using System.Collections;
using System.Collections.Generic;

namespace MathSharp.Additional
{
    public class SpecialList<T> : IList<T>
    {
        List<T> _list;

        public T this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int Count { get; private set; }

        public bool IsReadOnly { get; internal set; }


        public SpecialList()
        {
            _list = new List<T>();
        }





        public void Add(T item)
        {
            if (IsReadOnly)
                throw new Exception("Collection is readonly");
            _list.Add(item);
        }

        public bool TryAdd(T item)
        {
            if (IsReadOnly)
                return false;

            _list.Add(item);
            return true;
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (IsReadOnly)
                throw new Exception("Collection is readonly");
            return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
