using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject _orbGameObject;
    private Transform _orbTransform;
    void Awake()
    {
        CacheReferences();    
    }

    void CacheReferences()
    {
        if(!_orbGameObject) _orbGameObject = transform.Find("Orb").gameObject;
        if(!_orbTransform) _orbTransform = _orbGameObject?.transform;
    }

    public void Respawn()
    {
        if(!_orbGameObject.activeSelf)
        _orbGameObject.transform.localPosition = Vector3.zero;
        _orbGameObject.SetActive(true);
    }
}
