using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Container;

public sealed class DynamicArray<T> : IList<T>, IList, IReadOnlyList<T>
{
    private const int DEFAULT_CAPACITY = 4;
    private readonly static T[] _emptyArray = [];

    private T[] _items;
    private int _size = 0;

    public DynamicArray()
    {
        _items = _emptyArray;
    }

    public DynamicArray(int capacity)
    {
        _items = new T[capacity];
    }


    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException($"{nameof(index)} is out of Range");

            return _items[index];
        }

        set
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException($"{nameof(index)} is out of Range");

            _items[index] = value;
        }
    }

    object? IList.this[int index]
    {
        get
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException($"{nameof(index)} is out of Range");

            return _items[index];
        }

        set
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException($"{nameof(index)} is out of Range");

            if (value is not T)
                throw new InvalidCastException($"Unable to cast object of type '{value?.GetType().Name}' to type '{typeof(T).Name}'.");

            _items[index] = (T)value;
        }
    }

    public int Count { get { return _size; } }
    public int Capacity
    {
        get { return _items.Length; }

        set
        {
            if (value < _size)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Argument Out of Range");

            if (value != _size)
            {
                if (value > 0)
                {
                    T[] newItems = new T[value];
                    if (_size > 0)
                    {
                        Array.Copy(_items, newItems, _size);
                    }
                    _items = newItems;
                }
                else
                {
                    _items = _emptyArray;
                }
            }
              
        }
    }
    public bool IsReadOnly { get { return false; } }
    public bool IsFixedSize { get { return false; } }
    public bool IsSynchronized { get { return false; } }
    public object SyncRoot { get { return this; } }

    public void Add(T item)
    {
        Debug.Assert(_size <= _items.Length);

        if (_size == _items.Length)
            {
                Grow(_size + 1);
            }

        _items[_size++] = item;
    }

    public int Add(object? value)
    {
        Debug.Assert(_size <= _items.Length);

        if (value is not T)
            throw new InvalidCastException($"Unable to cast object of type '{value?.GetType().Name}' to type '{typeof(T).Name}'.");

        if (_items.Length == _size)
        {
            Grow(_size + 1);
        }

        _items[_size++] = (T)value;

        return _size - 1;
    }

    public void AddRange(IEnumerable<T> collection)
    {
        if (collection == null)
        {
            throw new NullReferenceException();
        }

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if (_items.Length < _size + count)
            {
                Grow(checked(_size + count));
            }

            c.CopyTo(_items, _size);
            _size += count;
        }
        else
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
        return new ReadOnlyCollection<T>(this);
    }

    public void Clear()
    {
        if (_size == 0)
            return;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, 0, _size);
        }

        _size = 0;
    }

    public bool Contains(T item)
    {
        return _size != 0 && Array.IndexOf(_items, item, 0, _size) >= 0;
    }

    public bool Contains(object? value)
    {
        if (value is not T)
            throw new InvalidCastException($"Unable to cast object of type '{value?.GetType().Name}' to type '{typeof(T).Name}'.");

        return _size != 0 && Array.IndexOf(_items, value, 0, _size) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex = 0)
    {
        // array가 null인지 검사
        ArgumentNullException.ThrowIfNull(array);

        // index가 적절한지 검사
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index is Out of Range");

        // array에 요소를 복사할 공간이 충분한지 검사
        if (array.Length - arrayIndex < _size)
            throw new ArgumentException("Invaild Arguments");

        Array.Copy(_items, 0, array, arrayIndex, _size);
    }

    public void CopyTo(Array array, int index)
    {
        // array가 null인지 검사
        ArgumentNullException.ThrowIfNull(array);

        // Array 타입과 T[] 타입이 일치하는 검사
        if (array.GetType() != _items.GetType())
            throw new ArrayTypeMismatchException();

        // array가 1차원 배열인지 검사
        if (array.Rank != 1)
            throw new ArgumentException("Invaild Arguments");

        // index가 적절한지 검사
        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is Out of Range");

        // array에 요소를 복사할 공간이 충분한지 검사
        if (array.Length - index < _size)
            throw new ArgumentException("Invaild Arguments");

        Array.Copy(_items, 0, array, index, _size);
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        // array가 null인지 검사
        ArgumentNullException.ThrowIfNull(array);

        // index가 적절한지 검사
        if (arrayIndex < 0 || arrayIndex > array.Length)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index is Out of Range");

        // array에 요소를 복사할 공간이 충분한지 검사
        if (array.Length - arrayIndex < _size)
            throw new ArgumentException("Invaild Arguments");

        Array.Copy(_items, index, array, arrayIndex, count);
    }

    public bool Exists(Predicate<T> match)
    {
        return FindIndex(match) != -1;
    }

    public T? Find(Predicate<T> match)
    {
        if (match == null)
            throw new NullReferenceException();

        for (int i = 0; i < _size; i++)
        {
            if (match(_items[i]))
            {
                return _items[i];
            }
        }

        return default(T);
    }

    public DynamicArray<T> FindAll(Predicate<T> match)
    {
        if (match == null)
            throw new NullReferenceException();

        DynamicArray<T> dArray = [];
        for (int i = 0; i < _size; i++)
        {
            if (match(_items[i]))
            {
                dArray.Add(_items[i]);
            }
        }

        return dArray;
    }

    public int FindIndex(Predicate<T> match)
    {
        return FindIndex(0, _size, match);
    }

    public int FindIndex(int startIndex, Predicate<T> match)
    {
        return FindIndex(startIndex, _size - startIndex, match);
    }

    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        if ((uint)startIndex > (uint)count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (count < 0 || startIndex > _size - count)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (match == null)
            throw new NullReferenceException();

        int endIndex = startIndex + count;
        for (int i = startIndex; i < endIndex; i++)
        {
            if (match(_items[i])) return i;
        }

        return -1;
    }

    public T? FindLast(Predicate<T> match)
    {
        if (match == null)
            throw new NullReferenceException();

        for (int i = _size - 1; i >= 0; i--)
        {
            if (match(_items[i]))
                return _items[i];
        }

        return default(T);
    }

    public int FindLastIndex(Predicate<T> match)
    {
        return FindLastIndex(_size - 1, _size, match);
    }

    public int FindLastIndex(int startIndex, Predicate<T> match)
    {
        return FindLastIndex(startIndex, _size, match);
    }

    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        if (match == null)
            throw new NullReferenceException();

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Argument Out Of Range");

        if (startIndex < 0 || startIndex < count + 1)
            throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "Argument Out Of Range");

        if (_size == 0)
            return -1;

        int endIndex = count - startIndex;
        for (int i = startIndex; i >= endIndex; i--)
        {
            if (match(_items[i])) return i;
        }

        return -1;
    }

    public void ForEach(Action<T> action)
    {
        if (action == null)
            throw new NullReferenceException();

        for (int i = 0; i < _size; i++)
        {
            action(_items[i]);
        }
    }

    public DynamicArray<T> GetRange(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Argument Out Of Range");

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Argument Out Of Range");

        if (_size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count), "Argument Out Of Range");

        DynamicArray<T> dArray = new(count);
        Array.Copy(_items, index, dArray._items, 0, count);
        dArray._size = count;

        return dArray;
    }

    public DynamicArray<T> Slice(int start, int length)
    {
        return GetRange(start, length);
    }
    
    public int IndexOf(T item)
    {
        return Array.IndexOf(_items, item, 0, _size);
    }

    public int IndexOf(object? value)
    {
        if (value is not T)
            throw new InvalidCastException($"Unable to cast object of type '{value?.GetType().Name}' to type '{typeof(T).Name}'.");

        return Array.IndexOf(_items, value, 0, _size);
    }

    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)} is Out of Range");

        if (_size == _items.Length)
        {
            Grow(_size + 1);
        }

        Array.Copy(_items, index, _items, index + 1, _size - index);
        _items[index] = item;
        _size++;
    }

    public void Insert(int index, object? value)
    {
        if (value is not T)
            throw new InvalidCastException($"Unable to cast object of type '{value?.GetType().Name}' to type '{typeof(T).Name}'.");

        if ((uint)index > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is Out of Range");

        if (_size == _items.Length)
        {
            Grow(_size + 1);
        }

        Array.Copy(_items, index, _items, index + 1, _size - index);
        _items[index] = (T)value;
        _size++;
    }

    public void InsertRange(int index, IEnumerable<T> collection)
    {
        if (collection == null)
            throw new NullReferenceException();

        if ((uint)index > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is Out of Range");

        if (collection is ICollection<T> c)
        {
            int count = c.Count;

            if (count > _items.Length - _size)
            {
                Grow(count + _size);
            }

            if (index < _size)
            {
                Array.Copy(_items, index, _items, index + count, _size - count);
            }

            c.CopyTo(_items, index);
            _size += count;
        }
        else
        {
            foreach (T item in collection)
            {
                Insert(index++, item);
            }
        }
    }

    public int LastIndexOf(T item)
    {
        if (_size == 0)
        {
            return -1;
        }
        else
        {
            return LastIndexOf(item, _size - 1, _size);
        }
    }

    public int LastIndexOf(T item, int index, int count)
    {
        if (count != 0 && index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is Out of Range");

        if (index >= _size)
            throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} is Out of Range");

        if (index != 0 && count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(count)} is Out of Range");

        if (count > index + 1)
            throw new ArgumentOutOfRangeException(nameof(count), $"{nameof(count)} is Out of Range");

        if (_size == 0)
                return -1;

        return Array.LastIndexOf(_items, item, index, count);
    }
    

    public bool Remove(T item)
    {
        int index = IndexOf(item);

        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    public void Remove(object? value)
    {
        int index = IndexOf(value);

        if (index < 0)
        {
            return;
        }

        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        if ((uint)index > (uint)_size)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)} is Out of Range");

        Array.Copy(_items, index + 1, _items, index, _size - index);
        _size--;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[_size] = default(T)!;
        }
    }

    public int RemoveAll(Predicate<T> match)
    {
        if (match == null)
            throw new NullReferenceException();

        int freeIdx = 0;
        while (freeIdx < _size && !match(_items[freeIdx]))
        {
            freeIdx++;
        }

        if (freeIdx >= _size)
        {
            return 0;
        }

        int curIdx = freeIdx + 1;
        while (curIdx < _size)
        {
            while (curIdx < _size && match(_items[curIdx]))
            {
                curIdx++;
            }

            if (curIdx < _size)
            {
                _items[freeIdx] = _items[curIdx];
                freeIdx++;
                curIdx++;
            }
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, freeIdx, _size - freeIdx);
        }

        int result = _size - freeIdx;
        _size = freeIdx;

        return result;
    }

    public void RemoveRange(int index, int count)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)} is Out of Range");

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"{nameof(count)} is Out of Range");

        if (_size - index < count)
            throw new ArgumentOutOfRangeException(nameof(count), count, $"{nameof(count)} is Out of Range");

        if (count == 0)
            return;

        _size -= count;
        if (index < _size)
        {
            Array.Copy(_items, count + index, _items, index, _size - index);
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            Array.Clear(_items, _size, count);
        }
    }

    private void Grow(int capacity)
    {
        // 기본 설정은 2배로 증가시킨다.
        int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : _items.Length * 2;

        // 2배로 증가한 값이 배열 최대 길이를 넘지 않도록 제한한다.
        if (newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;

        // 사용자가 지정한 값이 더 크다면 지정한 값으로 변경한다.
        if (newCapacity < capacity) newCapacity = capacity;

        // Setter로 배열 크기 변경
        Capacity = capacity;
    }
    
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator(DynamicArray<T> array) : IEnumerator<T>, IEnumerator
    {
        private DynamicArray<T> _array = array;
        private int _curIdx = 0;
        private T _curItem = default(T)!; 

        public T Current { get { return _curItem; }}

        object IEnumerator.Current { get { return Current!; } }

        public void Dispose() {}

        public bool MoveNext()
        {
            if ((uint)_curIdx >= _array._size)
            {
                _curItem = default(T)!;
                _curIdx = -1;

                return false;
            }

            _curItem = _array._items[_curIdx++];

            return true;
        }

        public void Reset()
        {
            _curIdx = 0;
            _curItem = default(T)!;
        }
    }
}
