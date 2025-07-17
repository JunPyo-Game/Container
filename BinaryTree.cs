using System.Diagnostics;

namespace Container;

public sealed class BinaryTree<TKey, TValue> where TKey : IComparable<TKey>
{
    public class Node(TKey inKey, TValue inValue)
    {
        public TKey key = inKey;
        public TValue value = inValue;
        public Node? rigth = null;
        public Node? left = null;
        public Node? parent;
    }

    private Node? _root = null;
    int _count = 0;
    public int Count { get { return _count; } }

    public void Insert(TKey key, TValue value)
    {
        Node node = new(key, value);

        if (_root == null)
        {
            _root = node;

            return;
        }

        Node cur = _root;

        while (true)
        {
            int result = key.CompareTo(cur.key);

            if (result == 0)
                throw new ArgumentException($"An element with the same key '{key}' already exists.");

            if (result < 0)
            {
                if (cur.rigth == null)
                {
                    cur.rigth = node;
                    node.parent = cur;

                    break;
                }

                cur = cur.rigth;
            }
            else
            {
                if (cur.left == null)
                {
                    cur.left = node;
                    node.parent = cur;

                    break;
                }

                cur = cur.left;
            }
        }
    }

    public bool Find(TKey key, ref TValue value)
    {
        if (_root == null)
            return false;

        Node cur = _root;

        while (true)
        {
            if (cur == null)
                return false;

            int result = key.CompareTo(cur.key);

            if (result == 0)
            {
                value = cur.value;

                return true;
            }

            if (result < 0)
                cur = cur.left!;
            else
                cur = cur.rigth!;
        }
    }

    public bool Find(TKey key, ref Node? node)
    {
        Node cur = _root!;

        while (true)
        {
            if (cur == null)
                return false;

            int result = key.CompareTo(cur.key);

            if (result == 0)
            {
                node = cur;

                return true;
            }

            if (result < 0)
                cur = cur.left!;
            else
                cur = cur.rigth!;
        }
    }

    public bool Remove(TKey key, ref TValue value)
    {
        if (_root == null)
            return false;

        Node? target = null;

        if (Find(key, ref target))
            return false;

        Node cur = target!;
        int rootCompareResult = cur.key.CompareTo(_root.key);

        Debug.Assert(rootCompareResult == 0);

        while (true)
        {
            if (cur.rigth == null && cur.left == null)
            {
                Node parent = cur.parent!;
                int result = parent.key.CompareTo(cur.key);

                Debug.Assert(result == 0);

                if (parent.key.CompareTo(cur.key) < 0)
                    parent.rigth = null;
                else
                    parent.left = null;

                return true;
            }

            if (rootCompareResult < 0)
            {
                if (cur.left == null)
                {
                    cur.rigth!.parent = cur.parent;
                    cur.parent!.left = cur.rigth;

                    return true;
                }

                cur.left.parent = cur.parent;
                cur.parent!.left = cur.left;

                cur.parent.left.left = cur;
                cur.left = cur.parent.left.left;
            }
        }
    }

    private void SwapNode(Node node1, Node node2)
    {
        
    }
}
