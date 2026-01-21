using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] float _orbitRadius = 2f;

    [Header("Danger Zones")]
    [SerializeField] List<DangerZone> _dangerZones = new List<DangerZone>();

    [Header("References")]
    [SerializeField] Renderer _orbitRenderer;
    [SerializeField] Transform _transform;
    void Start()
    {
        UpdateShaderData();
        UpdateTransformValues();
    }
    void OnValidate()
    {
        UpdateShaderData();
        UpdateTransformValues();
    }

    public bool IsOrbInDanger(Vector3 orbPosition)
    {
        // Posición relativa a la luna
        Vector3 localPos = orbPosition - transform.position;

        float r = localPos.magnitude;
        if (Mathf.Abs(r - _orbitRadius) > 0.2f)
            return false;

        // Coordenadas esféricas
        float phi = Mathf.Acos(localPos.y / r);           // 0..PI
        float theta = Mathf.Atan2(localPos.z, localPos.x); // -PI..PI
        if (theta < 0) theta += Mathf.PI * 2;

        foreach (var zone in _dangerZones)
        {
            if (theta >= zone.thetaMin && theta <= zone.thetaMax &&
                phi >= zone.phiMin && phi <= zone.phiMax)
            {
                return true;
            }
        }

        return false;
    }

    void UpdateShaderData()
    {
        if (_orbitRenderer == null)
            _orbitRenderer = transform.Find("Orbit").GetComponent<Renderer>();

        if (_orbitRenderer == null)
        {
            Debug.LogError("Reference has no renderer");
            return;
        }

        Material mat = Application.isPlaying
            ? _orbitRenderer.material
            : _orbitRenderer.sharedMaterial;

        if (mat == null)
        {
            Debug.LogError("Reference has no material");
            return;
        }

        if (_dangerZones.Count == 0)
        {
            mat.SetFloat("_ThetaMin", 0);
            mat.SetFloat("_ThetaMax", 0);
            mat.SetFloat("_PhiMin", 0);
            mat.SetFloat("_PhiMax", 0);
            return;
        }

        DangerZone z = _dangerZones[0];

        mat.SetFloat("_ThetaMin", z.thetaMin);
        mat.SetFloat("_ThetaMax", z.thetaMax);
        mat.SetFloat("_PhiMin", z.phiMin);
        mat.SetFloat("_PhiMax", z.phiMax);
    }

    void UpdateTransformValues()
    {
        if (_transform == null) _transform = transform.Find("Orbit");

        if (_transform == null) 
        {
            Debug.LogError("Reference has no transform");
            return;
        }

        float diameter = _orbitRadius * 2f;
        _transform.localScale = Vector3.one * diameter;
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
