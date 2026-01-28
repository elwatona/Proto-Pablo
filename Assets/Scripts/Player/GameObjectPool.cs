using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _parent;
    private readonly List<GameObject> _pool = new List<GameObject>();

    public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            GameObject go = CreateInstance();
            go.SetActive(false);
            _pool.Add(go);
        }
    }

    private GameObject CreateInstance()
    {
        return Object.Instantiate(_prefab, _parent);
    }

    /// <summary>
    /// Obtiene un GameObject activo desde la pool
    /// </summary>
    public GameObject Get()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].activeSelf)
            {
                _pool[i].SetActive(true);
                return _pool[i];
            }
        }

        // Si no hay ninguno libre, crea uno nuevo
        GameObject go = CreateInstance();
        go.SetActive(true);
        _pool.Add(go);
        return go;
    }

    /// <summary>
    /// Devuelve un GameObject a la pool (lo desactiva)
    /// </summary>
    public void Release(GameObject go)
    {
        go.SetActive(false);
    }
}
