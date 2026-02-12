using System;
using UnityEngine;

[Serializable]
public class RigidbodyOrbiter
{
    readonly Action _orbitEnter, _orbitExit;
    readonly Transform _transform;
    readonly Rigidbody _rb;

    private IOrbitable _astroOrbit, _sunOrbit;
    [SerializeField] Transform _astroTransform, _sunTransform;

    private float _orbitDirection = 1f;

    public RigidbodyOrbiter(Rigidbody rb, Transform transform, Action orbitEnter, Action orbitExit)
    {
        _rb = rb;
        _transform = transform;
        _orbitEnter = orbitEnter;
        _orbitExit = orbitExit;

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void FixedUpdate()
    {
        if (_astroTransform)
        {
            if (_astroOrbit != null)
                ApplyOrbitalForces(_astroTransform, _astroOrbit.Data);

            if (!_astroTransform.parent.gameObject.activeSelf)
                Loose();

            return;
        }

        if (_sunTransform)
        {
            if (_sunOrbit != null)
                ApplyOrbitalForces(_sunTransform, _sunOrbit.Data);

            if (!_sunTransform.parent.gameObject.activeSelf)
                Loose();
        }
    }

    public void EnterOrbit(IOrbitable orbit, Transform body)
    {
        switch (orbit.Data.astroType)
        {
            case AstroType.None:
                Debug.LogWarning("Astro type not registered");
                return;

            case AstroType.Sun:
                SetSunOrbit(orbit, body);
                break;

            default:
                SetAstroOrbit(orbit, body);
                break;
        }

        if (_rb.useGravity)
            _rb.useGravity = false;

        // -------- FIJAR DIRECCIÓN ORBITAL --------
        Vector3 toCenter = body.position - _transform.position;
        Vector3 centerDir = toCenter.normalized;
        Vector3 baseTangent = Vector3.Cross(centerDir, Vector3.forward).normalized;

        float dot = Vector3.Dot(_rb.linearVelocity, baseTangent);
        _orbitDirection = dot >= 0 ? 1f : -1f;
    }

    public void ApplyOrbitalForces(Transform body, OrbitData data)
    {
        Vector3 astroVelocity = data.velocity;
        Vector3 relativeVelocity = _rb.linearVelocity - astroVelocity;

        Vector3 toCenter = body.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.1f) return;

        Vector3 centerDir = toCenter.normalized;

#region Gravedad
        float gravityStrength = data.gravity / (distance * distance + .5f);
        _rb.AddForce(centerDir * gravityStrength, ForceMode.Acceleration);
#endregion
#region Damping Radial
        Vector3 radialVelocity = Vector3.Project(relativeVelocity, centerDir);
        _rb.AddForce(-radialVelocity * data.radialDamping, ForceMode.Acceleration);
#endregion
#region Fuerza Tangencial
        Vector3 baseTangent = Vector3.Cross(centerDir, Vector3.forward).normalized;
        Vector3 tangentDir = baseTangent * _orbitDirection;

        Vector3 tangentialVelocity = relativeVelocity - radialVelocity;

        // velocidad orbital correcta según radio
        float desiredSpeed = Mathf.Sqrt(data.gravity / distance);

        Vector3 desiredTangentialVelocity = tangentDir * desiredSpeed;

        Vector3 tangentialError = desiredTangentialVelocity - tangentialVelocity;

        float tangentialGain = 1.2f;

        _rb.AddForce(
            tangentialError * tangentialGain,
            ForceMode.Acceleration
        );
#endregion

        Debug.DrawLine(_transform.position, body.position, Color.yellow);
        Debug.DrawRay(_transform.position, tangentDir, Color.cyan);
    }

    public void SetSunOrbit(IOrbitable orbit, Transform body)
    {
        if (_sunOrbit != orbit)
        {
            _orbitEnter?.Invoke();
            _sunOrbit?.ExitOrbit();
        }

        orbit.EnterOrbit();

        _sunOrbit = orbit;
        _sunTransform = body;
    }

    public void SetAstroOrbit(IOrbitable orbit, Transform body)
    {
        if (_astroOrbit != orbit)
        {
            _orbitExit?.Invoke();
            _astroOrbit?.ExitOrbit();
        }

        orbit.EnterOrbit();

        _astroOrbit = orbit;
        _astroTransform = body;
    }

    public void Loose()
    {
        if (_sunOrbit == null && _astroOrbit == null)
            return;

        _orbitExit?.Invoke();

        _astroOrbit?.ExitOrbit();
        _sunOrbit?.ExitOrbit();

        _astroOrbit = null;
        _astroTransform = null;

        _sunOrbit = null;
        _sunTransform = null;
    }

    public void OnEnable()
    {
        _rb.useGravity = true;
    }

    public void OnDisable()
    {
        _astroOrbit?.ExitOrbit();
        _sunOrbit?.ExitOrbit();

        _rb.linearVelocity = Vector3.zero;

        _astroOrbit = null;
        _astroTransform = null;

        _sunOrbit = null;
        _sunTransform = null;
    }
}