using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Container;

public sealed class MyQueue<T>
    : IEnumerable<T>, ICollection, IReadOnlyCollection<T>
{
     const int DEFAULT_CAPACITY = 4;

    private T[] _items;
    public int _head = 0;
    public int _tail = 0;
    private int _count = 0;

    public MyQueue()
    {
        _items = [];
    }

    public MyQueue(int capacity)
    {
        _items = new T[capacity];
    }

#pragma warning disable IDE0305
    public MyQueue(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _items = collection.ToArray();
        _count = collection.Count();
    }
#pragma warning restore IDE0305

    public int Count { get { return _count; } }
    public bool IsSynchronized { get { return false; } }
    public object SyncRoot { get { return this; } }

    public bool Enqueue(T item)
    {
        Debug.Assert(_count <= _items.Length);

        if (_count == _items.Length)
        {
            Grow(_count + 1);
        }

        _items[_tail] = item;
        MoveNext(ref _tail);
        _count++;

        return true;
    }

    public T Dequeue()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        T item = _items[_head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[_head] = default(T)!;
        }

        MoveNext(ref _head);
         _count--;

        return item;
    }

    public bool TryDequeue(out T result)
    {
        if (_count == 0)
        {
            result = default(T)!;

            return false;
        }

        result = _items[_head];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _items[_head] = default(T)!;
        }

        MoveNext(ref _head);
         _count--;

        return true;
    }

    public T Peek()
    {
        if (_count == 0)
        {
            throw new InvalidOperationException("Queue is empty.");
        }

        return _items[_head];
    }

    public bool TryPeek(out T result)
    {
        if (_count == 0)
        {
            result = default(T)!;

            return false;
        }

        result = _items[_head];
     
        return true;
    }

    public void Clear()
    {
        if (_count != 0)
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                if (_head < _tail)
                {
                    Array.Clear(_items, _head, _count);
                }
                else
                {
                    Array.Clear(_items, _head, _items.Length - _head);
                    Array.Clear(_items, 0, _tail);
                }
            }

            _count = 0;
        }

        _head = 0;
        _tail = 0;
    }

    public bool Contains(T item)
    {
        if (_count == 0)
            return false;

        if (_head < _tail)
        {
            return Array.IndexOf(_items, item, _head, _count) >= 0;
        }
        else
        {
            return Array.IndexOf(_items, item, _head, _items.Length - _head) >= 0 ||
                   Array.IndexOf(_items, item, 0, _tail) >= 0;
        }
    }

    public T[] ToArray()
    {
        if (_count == 0)
            return Array.Empty<T>();

        T[] array = new T[_count];
        Copy(array, 0);

        return array;
    }

    public void CopyTo(T[] array, int index)
    {
        // array가 null인지 검사
        ArgumentNullException.ThrowIfNull(array);

        // index가 적절한지 검사
        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is Out of Range");

        // array에 요소를 복사할 공간이 충분한지 검사
        if (array.Length - index < _count)
            throw new ArgumentException("Invaild Arguments");

        if (_count == 0) return;

        Copy(array, index);
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

        if (_count == 0) return;

        Copy((T[])array, index);
    }

    public void TrimExcess()
    {
        if (_items.Length * 0.9 > _count)
        {
            SetCapacity(_count);
        }
    }

    private void Copy(T[] array, int index)
    {
        if (_head < _tail)
        {
            Array.Copy(_items, _head, array, index, _count);
        }
        else
        {
            Array.Copy(_items, _head, array, index, _items.Length - _head);
            Array.Copy(_items, 0, array, index + _items.Length - _head, _tail);
        }
    }

    
    // TODO : 나머지 연산자 vs 단순 비교문 성능 테스트하기
    private void MoveNext(ref int idx)
    {
        // idx = (idx + 1) % _items.Length;

        idx += 1;
        if (idx == _items.Length)
        {
            idx = 0;
        }
    }

    private void SetCapacity(int capacity)
    {
        Debug.Assert(capacity >= _count);

        T[] newItems = new T[capacity];

        if (_count > 0)
        {
            Copy(newItems, 0);
        }

        _items = newItems;
        _head = 0;
        _tail = _count == capacity ? 0 : _count;
    }

    private void Grow(int capacity)
    {
        int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : _items.Length * 2;
        if ((uint)newCapacity > Array.MaxLength) newCapacity = Array.MaxLength;
        if (newCapacity < capacity) newCapacity = capacity;

        SetCapacity(newCapacity);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    class Enumerator(MyQueue<T> queue) : IEnumerator<T>, IEnumerator
    {
        private MyQueue<T> _queue = queue;
        private int _curIdx = -1;
        private T? _curItem;

        public T Current
        {
            get
            {
                if (_curIdx < 0)
                    throw new InvalidOperationException(_curIdx == -1 ? "Enum Not Start" : "Enum Ended");

                return _curItem!;
            }
        }

        object IEnumerator.Current { get { return Current!; } }

        public void Dispose() { _curIdx = -2; }
        public void Reset()
        {
            _curIdx = -1;
            _curItem = default(T);
        }

        public bool MoveNext()
        {
            if (_curIdx == -2)
            {
                return false;
            }

            if (_curIdx == _queue._count)
            {
                _curIdx = -2;
                _curItem = default(T);

                return false;
            }

            _curIdx++;
            uint length = (uint)_queue._items.Length;
            uint idx = (uint)(_queue._head + _curIdx);

            if (idx >= length)
            {
                idx -= length;
            }

            _curItem = _queue._items[idx];

            return true;
        }
    }
}