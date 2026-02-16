using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Moon Pool Settings")]
    [SerializeField] GameObject _moonPrefab;
    [SerializeField] int _initialPoolSize;

    private GameObjectPool _pool;

    [Header("Settings")]
    [SerializeField] GameObject _orbGameObject;
    [SerializeField] Orb _orb;
    void Awake()
    {
        CacheReferences();    
    }

    void CacheReferences()
    {
        if(!_orbGameObject) _orbGameObject = transform.Find("Orb").gameObject;
        if(!_orb) _orb = _orbGameObject?.GetComponent<Orb>();
        if(_pool == null) _pool = new GameObjectPool(_moonPrefab, _initialPoolSize);
    }

    public void Aim(InputAction.CallbackContext context)
    {
        if (context.started)
            _orb.SetAiming(true);
        else if (context.canceled)
            _orb.SetAiming(false);
    }
    public void Loose(InputAction.CallbackContext context)
    {
        if(!context.started) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _orb.Loose(cursorWorldPosition);
    }
    public void Respawn()
    {
        if(!_orbGameObject.activeSelf)
        _orbGameObject.transform.localPosition = Vector3.zero;
        _orbGameObject.SetActive(true);
    }
    public void CreateMoon(InputAction.CallbackContext context)
    {
        if(!context.started) return;

        GameObject moon = _pool.Get();
        Vector3 desiredPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        desiredPosition.z = 0;
        moon.transform.position = desiredPosition;
    }
}
