using UnityEngine;
public interface IOrbitable
{
    public bool IsInDangerZone(Vector3 orbPosition);
    public void EnterOrbit();
    public void ExitOrbit();
}