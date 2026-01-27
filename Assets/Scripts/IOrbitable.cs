using UnityEngine;
public interface IOrbitable
{
    public bool CollidesInDangerZone(Vector3 orbPosition);
    public void EnterOrbit();
    public void ExitOrbit();
}