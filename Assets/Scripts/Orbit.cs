using System;
using Unity.VisualScripting;
using UnityEngine;

public class Orbit : MonoBehaviour, IOrbitable
{
    [Header("Orbit Settings")]
    [SerializeField] DangerZone _dangerZone;
    private OrbitData _runtimeData;

    [Header("References")]
    [SerializeField] Renderer _orbitRenderer;
    [SerializeField] Transform _transform;
    private OrbitShaderController _shaderController;
    [SerializeField] float _collapseTimer;

    public OrbitData Data => _runtimeData;
    public float CollapseTimer => _collapseTimer;

    void Awake()
    {
        CacheReferences();
    }
    void Start()
    {
        _shaderController.Apply();
    }
    void OnValidate()
    {
        CacheReferences();
        _shaderController.SetData(_dangerZone);
    }
    void Update()
    {
        Debug.DrawRay(_transform.position, Vector3.up, Color.green, 1f);
        Debug.DrawRay(_transform.parent.position, Vector3.right, Color.blue, 1f);
    }
    void CacheReferences()
    {
        if (!_transform) _transform = transform;       
        if (!_orbitRenderer) _orbitRenderer = _transform.GetComponent<Renderer>();
        if (_shaderController == null) _shaderController = new OrbitShaderController(_orbitRenderer, _dangerZone);
    }
    public bool IsInDangerZone(Vector3 orbPosition)
    {
        Vector3 localPos = transform.parent.InverseTransformPoint(orbPosition);

        float r = localPos.magnitude;
        if (r < 0.0001f) return false;

        float phi = Mathf.Acos(Mathf.Clamp(localPos.y / r, -1f, 1f));
        float theta = Mathf.Atan2(localPos.z, localPos.x);
        if (theta < 0) theta += Mathf.PI * 2f;
        
        return theta >= _dangerZone.thetaMin && theta <= _dangerZone.thetaMax && phi   >= _dangerZone.phiMin   && phi   <= _dangerZone.phiMax;
    }


    bool IsAngleInRange(float angle, float min, float max)
    {
        if (min <= max)
            return angle >= min && angle <= max;

        return angle >= min || angle <= max;
    }


    public void EnterOrbit()
    {
        _collapseTimer = 0f;
        _shaderController.SetTetha(0,0);
        _shaderController.SetPhi(0,0);
    }
    public void ExitOrbit()
    {
        _shaderController.SetData(_dangerZone);
    }
    public void SetData(OrbitData data)
    {
        _runtimeData = data;
        _runtimeData.radialDamping = Mathf.Lerp(15, 1, _runtimeData.gravity/100);
    }

    public void UpdateTangentialForce()
    {
        _collapseTimer += Time.fixedDeltaTime;

        _runtimeData.tangentialForce = _collapseTimer >= (_runtimeData.radius * 2) ? -1 : 1;
        Debug.Log(_runtimeData.tangentialForce);
    }
}

[Serializable]
public struct DangerZone
{
    [Range(0, Mathf.PI * 2)]
    public float thetaMin;

    [Range(0, Mathf.PI * 2)]
    public float thetaMax;

    [Range(0, Mathf.PI)]
    public float phiMin;

    [Range(0, Mathf.PI)]
    public float phiMax;
}
