using System.Collections;
using System.Diagnostics;

namespace Container;

public sealed class MyStack<T> 
    : IEnumerable<T>, ICollection, IReadOnlyCollection<T>
{
    const int DEFAULT_CAPACITY = 4;

    private T[] _items;
    private int _count = 0;

    public MyStack()
    {
        _items = [];
    }

    public MyStack(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _items = new T[capacity];
    }

#pragma warning disable IDE0305
    public MyStack(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _items = collection.ToArray();
        _count = collection.Count();
    }
#pragma warning restore IDE0305

    public int Count { get { return _count; } }
    public bool IsSynchronized { get { return false; } }
    public object SyncRoot { get { return this; } }

    public bool Cotains(T item)
    {
        return _count != 0 && Array.LastIndexOf(_items, item, _count - 1) != -1;
    }

    public void Push(T item)
    {
        // _count 값이 Length보다 커질 수는 없다.
        Debug.Assert(_count <= _items.Length);

        if (_count == _items.Length)
            Grow(_count + 1);

        _items[_count++] = item;
    }

    public T Pop()
    {
        int count = _count - 1;

        if ((uint)count >= (uint)_items.Length)
            throw new InvalidOperationException("Stack is empty.");

        T item = _items[count];
        // if (_isRefType) _items[count] = default(T)!;
        _items[count] = default(T)!;
        
        _count = count;

        return item;
    }

    public bool TryPop(out T item)
    {
        int count = _count - 1;

        if ((uint)count >= (uint)_items.Length)
        {
            item = default(T)!;

            return false;
        }

        item = _items[count];
        // if (_isRefType) _items[count] = default(T)!;
        _items[count] = default(T)!;
        _count = count;

        return true;
    }

    public T Peek()
    {
        int count = _count - 1;

        if ((uint)count >= (uint)_items.Length)
            throw new InvalidOperationException("Stack is empty.");

        return _items[count];
    }

    public bool TryPeek(out T item)
    {
        int count = _count - 1;

        if ((uint)count >= (uint)_items.Length)
        {
            item = default(T)!;

            return false;
        }

        item = _items[count];

        return true;
    }

    public void Clear()
    {
        Array.Clear(_items, 0, _count);
        // _items.AsSpan(0, _count).Clear();
        _count = 0;
    }

    public void CopyTo(T[] array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is Out of Range");

        if (array.Length - index < _count)
            throw new ArgumentException("Invaild Arguments");

        int scrIdx = 0;
        int destIdx = index + _count - 1;

        while (scrIdx < _count)
            array[destIdx--] = _items[scrIdx++];
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
        if (array.Length - index < _count)
            throw new ArgumentException("Invaild Arguments");

        Array.Copy(_items, 0, array, index, _count);
        Array.Reverse(array, index, _count);
    }

    public T[] ToArray()
    {
        if (_count == 0)
            return [];

        T[] array = new T[_count];

        for (int i = 0; i < _count; i++)
            array[i] = _items[_count - 1 - i];

        return array;
    }

    public void TrimExcess()
    {
        if (_count < (int)(_items.Length * 0.9))
            Array.Resize(ref _items, _count);
    }

    private void Grow(int capacity)
    {
        // Length보다 더 작은 값을 capacity로 들어오면 비정상
        Debug.Assert(_items.Length < capacity);

        int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : _items.Length * 2;

        // newCapacity 값이 오버로드되더라도 검사가 정상적으로 작동되게 만든다.
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < capacity) newCapacity = capacity;

        Array.Resize(ref _items, newCapacity);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    class Enumerator(MyStack<T> stack) : IEnumerator<T>, System.Collections.IEnumerator
    {
        private MyStack<T> _stack = stack;
        private int _curIdx = -2;
        private T? _curItem = default(T);

        public T Current
        {
            get
            {   
                // 비정상적인 인덱스 접근
                if (_curIdx < 0)
                {  
                    // _curIdx가 음수일때, -1, -2외의 값이 들어올 수 없다.
                    Debug.Assert(_curIdx == -1 || _curIdx == -2);

                    throw new InvalidOperationException(_curIdx == -2 ? "Enum Not Start" : "Enum Ended");
                }

                return _curItem!;
            }
        }

        object IEnumerator.Current { get { return Current!; } }

        public void Dispose() { _curIdx = -1; }
        public void Reset()
        {
            _curIdx = -2;
            _curItem = default(T);
        }

        public bool MoveNext()
        {
            if (_curIdx == -1)
                return false;

            if (_curIdx == -2)
                _curIdx = _stack.Count;

            bool result = --_curIdx >= 0;
            _curItem = result ? _stack._items[_curIdx] : default(T);

            return result;
        }
    }
}

