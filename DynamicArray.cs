using System.Collections;

namespace Container;

public sealed class DynamicArray<T> : IList<T>
{
    // Constant
    private const int DefaultCapacity = 4;

    // Fields
    private static readonly T[] s_emptyArray = [];
    private T[] _items;
    private int _count = 0;

    // Constructor
    public DynamicArray()
    {
        _items = s_emptyArray;
    }

    public DynamicArray(int size)
    {
        if (size < 0)
            throw new Exception();

        if (size == 0)
            _items = s_emptyArray;

        else
            _items = new T[size];
    }


    // Property
    public int Capacity
    {
        get { return _items.Length; }

        set
        {
            if (value < _count)
                throw new Exception();

            if (value != _items.Length)
            {
                if (value > 0)
                {
                    T[] newItems = new T[value];

                    if (_count > 0)
                        Array.Copy(_items, newItems, _count);

                    _items = newItems;
                }

                else
                    _items = s_emptyArray;
            }
        }
    }

    public int Count { get { return _count; } }

    public bool IsReadOnly => throw new NotImplementedException();

    // Indexer
    public T this[int idx]
    {
        get
        {
            if (idx >= _count)
                throw new Exception();

            return _items[idx];
        }

        set
        {
            if (idx >= _count)
                throw new Exception();

            _items[idx] = value;
        }
    }

    // Public Method
    public void Add(T item)
    {
        // Grow Items Length
        if (_count >= _items.Length)
            Grow(_count + 1);

        // Add Item
        _items[_count++] = item;
    }

    public void Insert(int index, T item)
    {
        if (index > _count)
            throw new Exception();

        if (_count == _items.Length)
            Grow(_count + 1);

        if (index < _count)
            Array.Copy(_items, index, _items, index + 1, _count - index);

        _items[index] = item;
        _count++;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);

            return true;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        if (index >= _count)
            throw new Exception();

        _count--;

        if (index < _count)
            Array.Copy(_items, index + 1, _items, index, _count - index);
        
        // GC가 수집할 수 있도록 참조 해제
        _items[_count] = default(T)!;
    }

    public void Clear()
    {
        // 참조 타입의 경우 GC가 수집할 수 있도록 모든 참조를 해제
        if (_count > 0)
        {
            // Span을 사용하여 Array.Clear와 동일한 효과
            _items.AsSpan(0, _count).Clear();
            _count = 0;
        }
    }

    public int IndexOf(T item)
    {
        return Array.IndexOf(_items, item, 0, _count);
    }

    public int IndexOf(T item, int index)
    {
        if (index > _count)
            throw new Exception();

        return Array.IndexOf(_items, item, index, _count - index);
    }

    public int IndexOf(T item, int index, int count)
    {
        if (index > _count)
            throw new Exception();

        if (count < 0 || index > _count - count)
            throw new Exception();

        return Array.IndexOf(_items, item, index, count);
    }

    public bool Contains(T item)
    {
        return _count != 0 && IndexOf(item) >= 0;
    }

    public void TrimExcess()
    {
        int threshold = (int)(((double)_items.Length) * 0.9);

        if (_count < threshold)
            Capacity = _count;
    }


    // Private Method
    private void Grow(int capacity)
    {
        int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
        if (newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < capacity) newCapacity = capacity;

        // Setter make new Items
        Capacity = newCapacity;
    }


    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        public T Current => throw new NotImplementedException();

        object IEnumerator.Current => Current!;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}


