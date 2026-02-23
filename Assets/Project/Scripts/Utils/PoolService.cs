using System.Collections.Generic;
using UnityEngine;

public class PoolService : MonoBehaviour
{
    private static PoolService _instance;
    public static PoolService Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("PoolService");
                _instance = go.AddComponent<PoolService>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, UnityObjectPool> _pools = new();

    public void CreatePool(string id, GameObject prefab, int initialSize)
    {
        if (_pools.ContainsKey(id)) return;

        var poolParent = new GameObject($"Pool_{id}").transform;
        poolParent.SetParent(transform);

        _pools[id] = new UnityObjectPool(prefab, initialSize, poolParent);
    }

    public GameObject Spawn(string id, Vector3 position, Quaternion rotation)
    {
        return _pools[id].Spawn(position, rotation);
    }

    public void Despawn(string id, GameObject obj)
    {
        _pools[id].Despawn(obj);
    }

    public void DespawnAll(string id)
    {
        _pools[id].DespawnAll();
    }
}