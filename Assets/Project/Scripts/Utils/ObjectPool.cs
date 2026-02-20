using System;
using System.Collections.Generic;

public class ObjectPool<T>
{
    private readonly Stack<T> _pool;
    private readonly Func<T> _createFunc;
    private readonly Action<T> _onGet;
    private readonly Action<T> _onRelease;

    public ObjectPool(Func<T> createFunc, Action<T> onGet = null, Action<T> onRelease = null, int initialSize = 0)
    {
        _pool = new Stack<T>(initialSize);
        _createFunc = createFunc;
        _onGet = onGet;
        _onRelease = onRelease;

        for (int i = 0; i < initialSize; i++)
        {
            _pool.Push(_createFunc());
        }
    }

    public T Get()
    {
        T item = _pool.Count > 0 ? _pool.Pop() : _createFunc();
        _onGet?.Invoke(item);
        return item;
    }

    public void Release(T item)
    {
        _onRelease?.Invoke(item);
        _pool.Push(item);
    }
}
