using System.Collections.Generic;
using UnityEngine;

public class UnityObjectPool
{
    private readonly ObjectPool<GameObject> _pool;
    private readonly HashSet<GameObject> _activeObjects = new();
    private readonly Transform _parent;

    public UnityObjectPool(
        GameObject prefab,
        int initialSize,
        Transform parent = null)
    {
        _parent = parent;

        _pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var obj = Object.Instantiate(prefab, _parent);
                obj.SetActive(false);
                return obj;
            },
            onGet: obj =>
            {
                _activeObjects.Add(obj);
                obj.SetActive(true);
                obj.GetComponent<IPoolable>()?.OnSpawned();
            },
            onRelease: obj =>
            {
                _activeObjects.Remove(obj);
                obj.GetComponent<IPoolable>()?.OnDespawned();
                obj.SetActive(false);
            },
            initialSize: initialSize
        );
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        var obj = _pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public void Despawn(GameObject obj)
    {
        if (!_activeObjects.Contains(obj))
            return;

        _pool.Release(obj);
    }

    public void DespawnAll()
    {
        // Copy to avoid modification during iteration
        var objectsToRelease = new List<GameObject>(_activeObjects);

        foreach (var obj in objectsToRelease)
        {
            _pool.Release(obj);
        }
    }
}
