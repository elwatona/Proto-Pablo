using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour, IOrbitable
{
    [Header("Orbit Settings")]
    [SerializeField] float _orbitRadius = 2f;
    [SerializeField] float _rotationSpeed = 1.5f;

    [Header("Danger Zones")]
    [SerializeField] List<DangerZone> _dangerZones = new List<DangerZone>();
    private bool _isOccupied;

    [Header("References")]
    [SerializeField] Renderer _orbitRenderer;
    [SerializeField] Transform _transform;
    private Material _material
    {
        get
        {
            if (_orbitRenderer == null)
            {
                Debug.LogError("Reference has no renderer");
                return null;
            }
            Material material = Application.isPlaying
                ? _orbitRenderer.material
                : _orbitRenderer.sharedMaterial;
            return material;
        }
    }
    void Start()
    {
        UpdateShaderData();
        UpdateTransformValues();
    }
    void OnValidate()
    {
        CacheReferences();
        UpdateShaderData();
        UpdateTransformValues();
    }
    void Update()
    {
        _transform.parent.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
    }

    public bool CollidesInDangerZone(Vector3 orbPosition)
    {
        Vector3 localPos = _transform.parent.InverseTransformPoint(orbPosition);

        float r = localPos.magnitude;
        if (Mathf.Abs(r - _orbitRadius) > 0.2f)
            return false;

        float phi = Mathf.Acos(localPos.y / r);
        float theta = Mathf.Atan2(localPos.z, localPos.x);
        if (theta < 0) theta += Mathf.PI * 2;

        foreach (DangerZone zone in _dangerZones)
        {
            if (theta >= zone.thetaMin && theta <= zone.thetaMax &&
                phi >= zone.phiMin && phi <= zone.phiMax)
            {
                return true;
            }
        }

        return false;
    }

    void CacheReferences()
    {
        if (_orbitRenderer == null)
            _orbitRenderer = transform.GetComponent<Renderer>();
        if (_transform == null) 
            _transform = transform;
            
    }
    void UpdateShaderData()
    {
        if (_dangerZones.Count == 0 || _isOccupied)
        {
            SetShaderValues(0,0,0,0);
            return;
        }

        DangerZone z = _dangerZones[0];
        SetShaderValues(z.thetaMin, z.thetaMax, z.phiMin, z.phiMax);
    }
    void SetShaderValues(float thetaMin, float thetaMax, float phiMin, float phiMax)
    {
        if(_material == null)
        {
            Debug.LogError("Reference has no material");
            return;
        }

        _material.SetFloat("_ThetaMin", thetaMin);
        _material.SetFloat("_ThetaMax", thetaMax);
        _material.SetFloat("_PhiMin", phiMin);
        _material.SetFloat("_PhiMax", phiMax);
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
        _isOccupied = true;
        UpdateShaderData();
    }

    public void ExitOrbit()
    {
        _isOccupied = false;
        UpdateShaderData();
    }
}

[System.Serializable]
public class DangerZone
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
