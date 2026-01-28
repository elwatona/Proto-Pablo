using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;

    [Header("Orbit Reference")]
    private Transform _moonTransform;
    private IOrbitable _orbit;
    private Rigidbody _rb;
    private Vector3 _screenPosition;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;
    private float _collisionAngle;

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
        if (_moonTransform != null) ApplyOrbitalForces();
        if (_moonTransform && !_moonTransform.parent.gameObject.activeSelf) Loose();
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
        _orbit?.ExitOrbit();
        _rb.linearVelocity = Vector3.zero;
        _orbit = null;
        _moonTransform = null;
        OnDespawn?.Invoke();
    }

    void EnterOrbit(IOrbitable orbit, Transform moon)
    {
        if(_orbit != null && _orbit != orbit) _orbit.ExitOrbit();

        if (orbit.IsInDangerZone(transform.position))
        {
            gameObject.SetActive(false);
            return;
        }

        orbit.EnterOrbit();


        if(_rb.useGravity) _rb.useGravity = false;
        
        _orbit = orbit;
        _moonTransform = moon;

        GetCollisionAngle();
        
        OnOrbitEnter?.Invoke();
    }
    void GetCollisionAngle()
    {
        Vector3 collisionPosition = transform.position;
        Vector3 moonPosition = _moonTransform.position;
        
        Vector3 collisionDirection = _rb.linearVelocity.normalized * -1;
        
        Vector3 normalToCollision = (collisionPosition - moonPosition).normalized;

        _collisionAngle = Vector3.Angle(collisionDirection, normalToCollision);
    }
    void ApplyOrbitalForces()
    {
        Vector3 toCenter = _moonTransform.position - transform.position;
        float distance = toCenter.magnitude;

        Vector3 centerDir = toCenter.normalized;

        float gravityStrength = _orbit.Data.gravity * Mathf.Clamp01(_orbit.Data.radius / distance);

        _rb.AddForce(centerDir * gravityStrength, ForceMode.Acceleration); //Fuerza de gravedad

        Vector3 radialVelocity = Vector3.Project(_rb.linearVelocity, centerDir); //Transformacion a coordenadas polares

        _rb.AddForce( -radialVelocity * _orbit.Data.radialDamping, ForceMode.Acceleration);

        Vector3 baseTangent = Vector3.Cross(centerDir, Vector3.forward).normalized;

        float directionSign = Mathf.Sign(Vector3.Dot(_rb.linearVelocity, baseTangent));
        if (directionSign == 0) directionSign = 1f;

        Vector3 tangentDir = baseTangent * directionSign;

        float tangentialInfluence = Mathf.Clamp(1f - distance / _orbit.Data.radius, 0, _orbit.Data.radius);

        _rb.AddForce(
            tangentDir * tangentialInfluence * _orbit.Data.tangentialForce,
            ForceMode.Acceleration
        );
        _orbit.UpdateTangentialForce();

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
