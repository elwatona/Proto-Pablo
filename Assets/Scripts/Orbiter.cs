using System;
using UnityEngine;
public class Orbiter
{
    readonly Action _orbitEnter, _orbitExit;
    readonly Transform _transform;
    readonly Rigidbody _rb;

    private IOrbitable _astroOrbit, _sunOrbit;
    private Transform _astroTransform, _sunTransform;
    private float _collisionAngle;
    public Orbiter(Rigidbody rb, Transform transform)
    {
        _rb = rb;
        _transform = transform;
    }
    public Orbiter(Rigidbody rb, Transform transform, Action orbitEnter, Action orbitExit)
    {
        _rb = rb;
        _transform = transform;
        _orbitEnter = orbitEnter;
        _orbitExit = orbitExit;
    }

    public void FixedUpdate()
    {
        if(_astroTransform)
        {
            if (_astroOrbit != null) ApplyOrbitalForces(_astroTransform, _astroOrbit.Data);
            if (!_astroTransform.parent.gameObject.activeSelf) Loose();
            return;
        }
        if(_sunTransform)
        {
            if (_sunOrbit != null) ApplyOrbitalForces(_sunTransform, _sunOrbit.Data);
            if (!_sunTransform.parent.gameObject.activeSelf) Loose();
        }
    }
    public void EnterOrbit(IOrbitable orbit, Transform body)
    {
        switch(orbit.Data.astroType)
        {
            case AstroType.None: Debug.LogWarning("Astro type not registered"); return;
            case AstroType.Sun: SetSunOrbit(orbit, body); break;
            default: SetAstroOrbit(orbit, body); break;
        }

        if(_rb.useGravity) _rb.useGravity = false;

        GetCollisionAngle(body);
    }
    public void ApplyOrbitalForces(Transform body, OrbitData data)
    {
        Vector3 toCenter = body.position - _transform.position;
        float distance = toCenter.magnitude;

        Vector3 centerDir = toCenter.normalized;

        float gravityStrength = data.gravity * Mathf.Clamp01(data.radius / distance);

        _rb.AddForce(centerDir * gravityStrength, ForceMode.Acceleration); //Fuerza de gravedad

        Vector3 radialVelocity = Vector3.Project(_rb.linearVelocity, centerDir); //Transformacion a coordenadas polares

        _rb.AddForce( -radialVelocity * data.radialDamping, ForceMode.Acceleration);

        Vector3 baseTangent = Vector3.Cross(centerDir, Vector3.forward).normalized;

        float directionSign = Mathf.Sign(Vector3.Dot(_rb.linearVelocity, baseTangent));
        if (directionSign == 0) directionSign = 1f;

        Vector3 tangentDir = baseTangent * directionSign;

        float tangentialInfluence = Mathf.Clamp(1f - distance / data.radius, 0, data.radius);

        _rb.AddForce(
            tangentDir * tangentialInfluence * data.tangentialForce,
            ForceMode.Acceleration
        );

        // _orbit.UpdateTangentialForce(); Actualiza el movimiento tangencial, permitiendo el ser repelido o impulsado al centro

        // Debug visual
        Debug.DrawLine(_transform.position, body.position, Color.yellow);
        Debug.DrawRay(_transform.position, tangentDir, Color.cyan);
    }
    public void SetSunOrbit(IOrbitable orbit, Transform body)
    {
        if(_sunOrbit != orbit)
        {
            _orbitEnter?.Invoke();
            
            if(_sunOrbit != null) _sunOrbit.ExitOrbit();
        }

        orbit.EnterOrbit();

        _sunOrbit = orbit;
        _sunTransform = body;
    }
    public void SetAstroOrbit(IOrbitable orbit, Transform body)
    {
        if(_astroOrbit != orbit)
        {
            _orbitExit?.Invoke();
            
            if(_astroOrbit != null) _astroOrbit.ExitOrbit();
        }

        orbit.EnterOrbit();
        
        _astroOrbit = orbit;
        _astroTransform = body;
    }
    public void GetCollisionAngle(Transform body)
    {
        Vector3 collisionPosition = _transform.position;
        Vector3 astroPosition = body.position;
        
        Vector3 collisionDirection = _rb.linearVelocity.normalized * -1;
        
        Vector3 normalToCollision = (collisionPosition - astroPosition).normalized;

        _collisionAngle = Vector3.Angle(collisionDirection, normalToCollision);
    }
    public void Loose()
    {
        if(_sunOrbit == null & _astroOrbit == null) return;

        _orbitExit?.Invoke();

        if(_astroOrbit == null)
        {
            _sunOrbit?.ExitOrbit();
            _sunOrbit = null;
            _sunTransform = null;
            return;
        }

        _astroOrbit?.ExitOrbit();
        _astroOrbit = null;
        _astroTransform = null;
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

        _sunTransform = null;
        _sunOrbit = null;
    }
}