using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Orb : MonoBehaviour
{
    public static event Action OnOrbitEnter, OnOrbitExit, OnSpawn, OnDespawn;
    public static event Action<float, EscapeMode> OnDebugUpdate;

    [SerializeField] RigidbodyOrbiter _orbiterController;
    [SerializeField] OrbiterSettings _orbiterSettings = OrbiterSettings.Default;
    [SerializeField] LineRenderer _trajectoryRenderer;
    [SerializeField] LineRenderer _directionRenderer;
    private LineRendererController _lineRendererController;
    private Rigidbody _rb;
    private Vector3 _screenPosition;
    private bool _isAiming;
    private bool _isInScreen => _screenPosition.x > 0 & _screenPosition.x < 1 & _screenPosition.y > 0 & _screenPosition.y < 1;

    public EscapeMode EscapeMode => _orbiterSettings.escapeMode;
    public float EscapeForce => _orbiterSettings.escapeForce;

    public void SetEscapeMode(EscapeMode mode)
    {
        _orbiterSettings.escapeMode = mode;
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    public void SetEscapeForce(float force)
    {
        _orbiterSettings.escapeForce = Mathf.Clamp(force, 0f, 30f);
        _orbiterController.UpdateSettings(_orbiterSettings);
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _orbiterController = new RigidbodyOrbiter(_rb, transform, OnOrbitEnter, OnOrbitExit, _orbiterSettings);
        _lineRendererController = new LineRendererController(_trajectoryRenderer, _directionRenderer, _orbiterController, transform.localScale.x);
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

        if (_isAiming)
        {
            Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cursorWorldPosition.z = 0;

            _lineRendererController.UpdateTrajectory(cursorWorldPosition);
        }

        _lineRendererController.UpdateDirection(transform.position, _rb.linearVelocity);

        OnDebugUpdate?.Invoke(_orbiterController.Speed, _orbiterController.EscapeMode);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IOrbitable orbit))
            _orbiterController.AddGravitySource(orbit);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IOrbitable orbit))
            _orbiterController.RemoveGravitySource(orbit);
    }
    void OnValidate()
    {
        _orbiterController?.UpdateSettings(_orbiterSettings);
    }
    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }
    void OnDisable()
    {
        SetAiming(false);
        _orbiterController.OnDisable();
        OnDespawn?.Invoke();
    }
    public void SetAiming(bool active)
    {
        _isAiming = active;
        _lineRendererController.SetAiming(active && _orbiterSettings.escapeMode == EscapeMode.Cursor);
    }
    public void Loose(Vector3 cursorWorldPosition)
    {
        _orbiterController.Loose(cursorWorldPosition);
    }
}
