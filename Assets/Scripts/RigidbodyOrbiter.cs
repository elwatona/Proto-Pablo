using System;
using System.Collections.Generic;
using UnityEngine;

public enum EscapeMode
{
    Cursor,
    Velocity
}

[Serializable]
public class RigidbodyOrbiter
{
    readonly Action _orbitEnter, _orbitExit;
    readonly Transform _transform;
    readonly Rigidbody _rb;

    private readonly List<IOrbitable> _gravitySources = new();
    private IOrbitable _capturedOrbit;
    private IOrbitable _lastReleasedOrbit;
    private float _graceTimer;
    private OrbiterSettings _settings;

    // Detach spin tracking
    private bool _isDetaching;
    private float _accumulatedAngle;
    private float _previousAngle;

    // Trajectory prediction cache
    private readonly Vector3[] _trajectoryPoints = new Vector3[2];

    public RigidbodyOrbiter(Rigidbody rb, Transform transform, Action orbitEnter, Action orbitExit, OrbiterSettings settings)
    {
        _rb = rb;
        _transform = transform;
        _orbitEnter = orbitEnter;
        _orbitExit = orbitExit;
        _settings = settings;

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // ---------------------------------------------------------
    // Lifecycle
    // ---------------------------------------------------------

    public void FixedUpdate()
    {
        if (_graceTimer > 0f)
        {
            _graceTimer -= Time.fixedDeltaTime;
            if (_graceTimer <= 0f)
                _lastReleasedOrbit = null;
        }

        ApplyGravitySources();

        if (_capturedOrbit != null)
        {
            ApplyStabilization(_capturedOrbit.Data);

            if (_isDetaching)
                UpdateDetach(_capturedOrbit.Data);
        }
    }

    public void OnEnable()
    {
        _capturedOrbit = null;
        _lastReleasedOrbit = null;
        _graceTimer = 0f;

        _isDetaching = false;
        _accumulatedAngle = 0f;
        _previousAngle = 0f;

        _gravitySources.Clear();
        _rb.linearVelocity = Vector3.zero;
        _rb.useGravity = true;
    }

    public void OnDisable()
    {
        _isDetaching = false;
        ReleaseCapturedOrbit();

        _rb.linearVelocity = Vector3.zero;
        _gravitySources.Clear();
    }

    // ---------------------------------------------------------
    // Gravity source tracking (called by Orb via triggers)
    // ---------------------------------------------------------

    public void AddGravitySource(IOrbitable source)
    {
        if (source.Data.type == AstroType.None)
        {
            Debug.LogWarning("Astro type not registered");
            return;
        }

        if (!_gravitySources.Contains(source))
            _gravitySources.Add(source);

        if (_rb.useGravity)
            _rb.useGravity = false;

        // Re-entering the captured orbit's zone cancels detach
        if (source == _capturedOrbit && _isDetaching)
        {
            _isDetaching = false;
            return;
        }

        GrabOrbit(source);
    }

    public void RemoveGravitySource(IOrbitable source)
    {
        // If this is the captured orbit, begin detach countdown instead of releasing.
        // Keep the source in the gravity list so forces continue during the final spins.
        if (_capturedOrbit == source)
        {
            if (!_isDetaching)
                BeginDetach();
            return;
        }

        _gravitySources.Remove(source);

        if (_gravitySources.Count == 0)
            _rb.useGravity = true;
    }

    // ---------------------------------------------------------
    // Phase 1 — Free-fall gravity from every nearby source
    // ---------------------------------------------------------

    void ApplyGravitySources()
    {
        for (int i = _gravitySources.Count - 1; i >= 0; i--)
        {
            IOrbitable source = _gravitySources[i];

            if (!IsValidSource(source))
            {
                if (_capturedOrbit == source)
                {
                    _isDetaching = false;
                    ReleaseCapturedOrbit();
                }

                _gravitySources.RemoveAt(i);
                continue;
            }

            // Skip gravity from the recently released orbit during grace period
            if (source == _lastReleasedOrbit && _graceTimer > 0f)
                continue;

            ApplyGravity(source.Data);
        }

        if (_gravitySources.Count == 0 && !_rb.useGravity)
            _rb.useGravity = true;
    }

    void ApplyGravity(OrbitData data)
    {
        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        float gravityMagnitude = data.gravity / (distance * distance);
        _rb.AddForce(toCenter.normalized * gravityMagnitude, ForceMode.Acceleration);

        Debug.DrawLine(_transform.position, data.transform.position, Color.grey);
    }

    // ---------------------------------------------------------
    // Phase 2 — Stabilization forces (only on the captured orbit)
    // ---------------------------------------------------------

    void ApplyStabilization(OrbitData data)
    {
        Vector3 relativeVelocity = _rb.linearVelocity - data.velocity;
        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 centerDir = toCenter.normalized;

        // Decompose velocity into radial and tangential components
        Vector3 radialVelocity = Vector3.Project(relativeVelocity, centerDir);
        Vector3 tangentialVelocity = relativeVelocity - radialVelocity;
        float tangentialSpeed = tangentialVelocity.magnitude;

        Vector3 tangentDir = tangentialSpeed > 0.001f
            ? tangentialVelocity.normalized
            : Vector3.Cross(centerDir, Vector3.forward).normalized;

        // -------------------
        // RADIUS CORRECTION
        // -------------------
        float radiusError = distance - data.radius;
        float correctionForce = radiusError * _settings.radiusCorrection;
        _rb.AddForce(centerDir * correctionForce, ForceMode.Acceleration);

        // -------------------
        // TANGENTIAL MAINTENANCE
        // -------------------
        float idealSpeed = Mathf.Sqrt(data.gravity / Mathf.Max(distance, 0.1f));
        float targetSpeed = Mathf.Min(idealSpeed, _settings.maxSpeed);
        float speedError = targetSpeed - tangentialSpeed;
        float tangentialAssist = speedError * data.tangentialForce * _settings.stabilization;
        _rb.AddForce(tangentDir * tangentialAssist, ForceMode.Acceleration);

        // -------------------
        // SPEED LIMITING
        // -------------------
        if (tangentialSpeed > _settings.maxSpeed)
        {
            float excess = tangentialSpeed - _settings.maxSpeed;
            _rb.AddForce(-tangentDir * excess * _settings.speedDamping, ForceMode.Acceleration);
        }

        // -------------------
        // RADIAL DAMPING
        // -------------------
        float normalizedDeviation = Mathf.Abs(radiusError) / Mathf.Max(data.radius, 0.1f);
        float dampingStrength = data.radialDamping * (1f + normalizedDeviation * _settings.stabilization * 2f);
        _rb.AddForce(-radialVelocity * dampingStrength, ForceMode.Acceleration);

        Debug.DrawLine(_transform.position, data.transform.position, Color.yellow);
    }

    // ---------------------------------------------------------
    // Grab — orbit captures the orb on trigger enter
    // ---------------------------------------------------------

    void GrabOrbit(IOrbitable orbit)
    {
        if (_capturedOrbit == orbit)
            return;

        if (orbit == _lastReleasedOrbit && _graceTimer > 0f)
            return;

        if (_capturedOrbit != null)
        {
            _isDetaching = false;
            _capturedOrbit.ExitOrbit();
            _orbitExit?.Invoke();
        }

        _capturedOrbit = orbit;
        orbit.EnterOrbit();

        _orbitEnter?.Invoke();

        // Snap velocity toward circular orbit
        OrbitData data = orbit.Data;
        Vector3 toCenter = data.transform.position - _transform.position;
        float distance = toCenter.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 centerDir = toCenter.normalized;
        Vector3 tangentDir = Vector3.Cross(centerDir, Vector3.forward).normalized;

        float effectiveDistance = Mathf.Lerp(distance, data.radius, _settings.stabilization);
        float orbitalSpeed = Mathf.Min(Mathf.Sqrt(data.gravity / effectiveDistance), _settings.maxSpeed);

        Vector3 idealVelocity = tangentDir * orbitalSpeed + data.velocity;
        float blendFactor = 0.5f + _settings.stabilization * 0.5f;
        _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, idealVelocity, blendFactor);
    }

    // ---------------------------------------------------------
    // Detach — spin countdown before releasing from orbit
    // ---------------------------------------------------------

    void BeginDetach()
    {
        _isDetaching = true;
        _accumulatedAngle = 0f;

        Vector3 toOrb = _transform.position - _capturedOrbit.Data.transform.position;
        _previousAngle = Mathf.Atan2(toOrb.y, toOrb.x);
    }

    void UpdateDetach(OrbitData data)
    {
        Vector3 toOrb = _transform.position - data.transform.position;
        float currentAngle = Mathf.Atan2(toOrb.y, toOrb.x);

        float delta = currentAngle - _previousAngle;

        // Wrap to [-PI, PI] to handle the ±180° boundary
        if (delta > Mathf.PI) delta -= 2f * Mathf.PI;
        if (delta < -Mathf.PI) delta += 2f * Mathf.PI;

        _accumulatedAngle += Mathf.Abs(delta);
        _previousAngle = currentAngle;

        if (_accumulatedAngle >= _settings.detachSpins * Mathf.PI * 2f)
            CompleteDetach();
    }

    void CompleteDetach()
    {
        _isDetaching = false;

        IOrbitable detachedOrbit = _capturedOrbit;
        ReleaseCapturedOrbit();

        _gravitySources.Remove(detachedOrbit);

        if (_gravitySources.Count == 0)
            _rb.useGravity = true;
    }

    // ---------------------------------------------------------
    // Release
    // ---------------------------------------------------------

    void ReleaseCapturedOrbit()
    {
        if (_capturedOrbit == null)
            return;

        _lastReleasedOrbit = _capturedOrbit;
        _graceTimer = 1f;

        _capturedOrbit.ExitOrbit();
        _capturedOrbit = null;
        _isDetaching = false;

        _orbitExit?.Invoke();
    }

    // ---------------------------------------------------------
    // Public API
    // ---------------------------------------------------------

    public void Loose(Vector3 cursorWorldPosition)
    {
        if (_capturedOrbit == null)
            return;

        float currentSpeed = _rb.linearVelocity.magnitude;

        Vector3 escapeDirection = _settings.escapeMode switch
        {
            EscapeMode.Velocity => _rb.linearVelocity.normalized,
            _ => (cursorWorldPosition - _transform.position).normalized,
        };

        _isDetaching = false;
        ReleaseCapturedOrbit();

        _rb.linearVelocity = escapeDirection * currentSpeed;
        _rb.AddForce(escapeDirection * _settings.escapeForce, ForceMode.VelocityChange);
    }

    /// <summary>Applies a fixed speed toward cursor (e.g. first loose after respawn, when velocity is 0 and there is no orbit).</summary>
    public void LooseWithFixedSpeed(Vector3 cursorWorldPosition, float speed)
    {
        Vector3 toCursor = cursorWorldPosition - _transform.position;
        Vector3 direction = toCursor.sqrMagnitude > 0.0001f ? toCursor.normalized : Vector3.right;

        if (_capturedOrbit != null)
        {
            _isDetaching = false;
            ReleaseCapturedOrbit();
        }

        _rb.linearVelocity = direction * speed;
    }

    public int PredictTrajectory(Vector3 cursorWorldPosition)
    {
        Vector3 origin = _transform.position;
        Vector3 direction = cursorWorldPosition - origin;
        float lineLength = direction.magnitude;

        _trajectoryPoints[0] = origin;
        _trajectoryPoints[1] = cursorWorldPosition;

        if (lineLength < 0.001f)
            return 1;

        // Check if the line segment intersects any non-attached orbit
        Vector3 normalized = direction / lineLength;

        for (int i = 0; i < _gravitySources.Count; i++)
        {
            IOrbitable source = _gravitySources[i];
            if (source == _capturedOrbit || !IsValidSource(source))
                continue;

            OrbitData data = source.Data;
            Vector3 toCenter = data.transform.position - origin;
            float projection = Vector3.Dot(toCenter, normalized);

            if (projection < 0f || projection > lineLength)
                continue;

            Vector3 closestPoint = origin + normalized * projection;
            float distanceToAxis = Vector3.Distance(closestPoint, data.transform.position);

            if (distanceToAxis > data.radius)
                continue;

            // Line enters this orbit — find the entry point along the segment
            float offset = Mathf.Sqrt(data.radius * data.radius - distanceToAxis * distanceToAxis);
            float entryDistance = projection - offset;

            if (entryDistance > 0f && entryDistance < lineLength)
            {
                Vector3 entryPoint = origin + normalized * entryDistance;

                // Keep the closest intersection
                if (Vector3.Distance(origin, entryPoint) < Vector3.Distance(origin, _trajectoryPoints[1]))
                    _trajectoryPoints[1] = entryPoint;
            }
        }

        return 2;
    }

    public Vector3[] TrajectoryPoints => _trajectoryPoints;
    public float Speed => _rb.linearVelocity.magnitude;
    public EscapeMode EscapeMode => _settings.escapeMode;

    public void UpdateSettings(OrbiterSettings settings)
    {
        _settings = settings;
    }

    // ---------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------

    static bool IsValidSource(IOrbitable source)
    {
        if (source is UnityEngine.Object obj && !obj)
            return false;

        Transform t = source.Data.transform;
        return t && t.parent && t.parent.gameObject.activeSelf;
    }
}

[Serializable]
public struct OrbiterSettings
{
    [Header("Orbit")]
    [Range(5f, 20f)] public float maxSpeed;
    [Range(0f, 1f)] public float stabilization;
    [Range(0f, 10f)] public float radiusCorrection;
    [Range(0f, 5f)] public float speedDamping;

    [Header("Escape")]
    public EscapeMode escapeMode;
    [Range(0f, 30f)] public float escapeForce;

    [Header("Detach")]
    [Range(1, 5)] public int detachSpins;

    public static OrbiterSettings Default => new()
    {
        maxSpeed = 10f,
        stabilization = 0.5f,
        radiusCorrection = 3f,
        speedDamping = 2f,
        escapeForce = 5f,
        detachSpins = 1,
    };
}
