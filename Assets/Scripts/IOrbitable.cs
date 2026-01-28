using System;
using UnityEngine;
public interface IOrbitable
{
    public OrbitData Data { get;}
    public float CollapseTimer { get;}
    public bool IsInDangerZone(Vector3 orbPosition);
    public void EnterOrbit();
    public void ExitOrbit();
    public void SetData(OrbitData data);
    public void UpdateTangentialForce();
}
[Serializable]
public struct OrbitData
{
    [Range(1f, 10f)] public float radius;
    [Range(1f, 100)] public float gravity; 
    [Range(-1f, 1f), HideInInspector] public float tangentialForce;
    [Range(0f, 10), HideInInspector] public float radialDamping;
}