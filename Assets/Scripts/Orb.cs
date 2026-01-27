using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    [Header("Orbit Reference")]
    private Transform _moonTransform;
    private IOrbitable _orbit;

    [Header("Orbit Settings")]
    [SerializeField] float _orbitRadius = 2f;

    [Header("Forces")]
    [SerializeField] float _gravityForce = 15f;          // Atracción central
    [SerializeField] float _tangentialForce = 8f;        // Influencia orbital
    [SerializeField] float _radialDamping = 4f;           // “Atmósfera”

    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (_moonTransform != null)
            ApplyOrbitalForces();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IOrbitable>(out IOrbitable orbit))
        {
            if(_orbit != null && _orbit!= orbit) _orbit.ExitOrbit();

            if (orbit.IsInDangerZone(transform.position))
            {
                // Impacto en zona peligrosa
                gameObject.SetActive(false);
                return;
            }

            orbit.EnterOrbit();
            _rb.useGravity = false;
            _orbit = orbit;
            _moonTransform = other.transform;
        }
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
        _orbit?.ExitOrbit();
        _orbit = null;
        _moonTransform = null;
    }
}
