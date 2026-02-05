using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;

    private Orbiter _orbiterController;
    private Rigidbody _rb;
    private Vector3 _screenPosition;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _orbiterController = new Orbiter(_rb, transform, OnOrbitEnter, OnOrbitExit);
    }
    void OnEnable()
    {
        _orbiterController.OnEnable();
        OnSpawn?.Invoke();
    }
    void FixedUpdate()
    {
        _orbiterController?.FixedUpdate();
    }
    void LateUpdate()
    {
        _screenPosition = Camera.main.WorldToViewportPoint(transform.position);
        if(!_isInScreen) gameObject.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IOrbitable orbit)) _orbiterController.EnterOrbit(orbit, other.transform);
    }
    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }
    void OnDisable()
    {
        _orbiterController.OnDisable();
        OnDespawn?.Invoke();
    }
    public void Loose()
    {
        _orbiterController.Loose();
    }
}
