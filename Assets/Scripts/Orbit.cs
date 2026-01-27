using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour, IOrbitable
{
    [Header("Orbit Settings")]
    [SerializeField] float _orbitRadius = 2f;
    [SerializeField] float _rotationSpeed = 1.5f;
    [SerializeField] DangerZone _dangerZone;

    [Header("References")]
    [SerializeField] Renderer _orbitRenderer;
    [SerializeField] Transform _transform;
    private OrbitShaderController _shaderController;
    void Awake()
    {
        CacheReferences();
    }
    void Start()
    {
        _shaderController.Apply();
        UpdateTransformValues();
    }
    void OnValidate()
    {
        CacheReferences();
        _shaderController.SetData(_dangerZone);
        UpdateTransformValues();
    }
    void Update()
    {
        _transform.parent.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
    }

    public bool IsInDangerZone(Vector3 orbPosition)
    {
        Vector3 localPos = _transform.parent.InverseTransformPoint(orbPosition);

        float r = localPos.magnitude;
        if (Mathf.Abs(r - _orbitRadius) > 0.2f)
            return false;

        float phi = Mathf.Acos(localPos.y / r);
        float theta = Mathf.Atan2(localPos.z, localPos.x);
        if (theta < 0) theta += Mathf.PI * 2;

            if (theta >= _dangerZone.thetaMin && theta <= _dangerZone.thetaMax &&
                phi >= _dangerZone.phiMin && phi <= _dangerZone.phiMax)
            {
                return true;
            }

        return false;
    }

    void CacheReferences()
    {
        if (!_transform) _transform = transform;       
        if (!_orbitRenderer) _orbitRenderer = _transform.GetComponent<Renderer>();
        if(_shaderController == null) _shaderController = new OrbitShaderController(_orbitRenderer, _dangerZone);
    }

    void UpdateTransformValues()
    {
        if (_transform == null) 
        {
            Debug.LogError("Reference has no transform");
            return;
        }

        float diameter = _orbitRadius * 2f;
        _transform.localScale = Vector3.one * diameter;
    }

    public void EnterOrbit()
    {
        _shaderController.SetTetha(0,0);
        _shaderController.SetPhi(0,0);
    }

    public void ExitOrbit()
    {
        _shaderController.SetData(_dangerZone);
    }
}

[System.Serializable]
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
