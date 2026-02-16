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
    public AstroType type;
    [HideInInspector] public Transform transform;
    [Range(1f, 10f)] public float radius;
    [Range(15, 30)] public float gravity; 
    [Range(2f, 5f)] public float tangentialForce;
    [Range(0.5f, 1.5f)] public float radialDamping;
    public Vector3 velocity;
}
public enum AstroType
{
    None,
    Sun,
    Planet,
    Asteroid
};