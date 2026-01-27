using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;

    [Header("Orbit Reference")]
    private Transform _moonTransform;
    private IOrbitable _orbit;

    [Header("Orbit Settings")]
    [SerializeField] float _orbitRadius = 2f;

    [Header("Forces")]
    [SerializeField] float _gravityForce = 15f;          // Atracción central
    [SerializeField] float _tangentialForce = 8f;        // Influencia orbital
    [SerializeField] float _radialDamping = 4f;           // “Atmósfera”

    private Rigidbody _rb;
    private Vector3 _screenPosition;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void OnEnable()
    {
        _rb.useGravity = true;
        OnSpawn?.Invoke();
    }
    void FixedUpdate()
    {
        if (_moonTransform != null)
            ApplyOrbitalForces();
    }
    void LateUpdate()
    {
        _screenPosition = Camera.main.WorldToViewportPoint(transform.position);
        if(!_isInScreen) gameObject.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IOrbitable orbit)) EnterOrbit(orbit, other.transform);
    }
    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }
    void OnDisable()
    {
        if(_orbit != null) _orbit.ExitOrbit();
        _rb.linearVelocity = Vector3.zero;
        OnDespawn?.Invoke();
    }

    void EnterOrbit(IOrbitable orbit, Transform moon)
    {
        if(_orbit != null && _orbit!= orbit) _orbit.ExitOrbit();

        if (orbit.IsInDangerZone(transform.position))
        {
            gameObject.SetActive(false);
            return;
        }

        orbit.EnterOrbit();

        if(_rb.useGravity) _rb.useGravity = false;
        
        _orbit = orbit;
        _moonTransform = moon;
        OnOrbitEnter?.Invoke();
    }
    void ApplyOrbitalForces()
    {
        Vector3 toCenter = _moonTransform.position - transform.position;
        float distance = toCenter.magnitude;

        Vector3 centerDir = toCenter.normalized;

        float gravityStrength =
            _gravityForce * Mathf.Clamp01(_orbitRadius / distance);

        _rb.AddForce(centerDir * gravityStrength, ForceMode.Acceleration);

        Vector3 radialVelocity =
            Vector3.Project(_rb.linearVelocity, centerDir);

        _rb.AddForce(
            -radialVelocity * _radialDamping,
            ForceMode.Acceleration
        );

        Vector3 baseTangent =
            Vector3.Cross(centerDir, Vector3.forward).normalized;

        float directionSign = Mathf.Sign(
            Vector3.Dot(_rb.linearVelocity, baseTangent)
        );
        if (directionSign == 0)
            directionSign = 1f;

        Vector3 tangentDir = baseTangent * directionSign;

        float tangentialInfluence =
            Mathf.Clamp01(1f - distance / _orbitRadius);

        _rb.AddForce(
            tangentDir * tangentialInfluence * _tangentialForce,
            ForceMode.Acceleration
        );

        // Debug visual
        Debug.DrawLine(transform.position, _moonTransform.position, Color.yellow);
        Debug.DrawRay(transform.position, tangentDir, Color.cyan);
    }

    public void Loose()
    {
        if(_orbit == null) return;

        _orbit?.ExitOrbit();
        _orbit = null;
        _moonTransform = null;
        OnOrbitExit.Invoke();
    }
}
