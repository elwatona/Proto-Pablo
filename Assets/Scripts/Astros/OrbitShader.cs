using UnityEngine;
public class OrbitShader
{
    readonly Renderer _renderer;
    readonly MaterialPropertyBlock _propertyBlock;
    private DangerZone _data;

#region ShaderIDs
    readonly int _thetaMinID = Shader.PropertyToID("_ThetaMin");
    readonly int _thetaMaxID = Shader.PropertyToID("_ThetaMax"); 
    readonly int _phiMinID = Shader.PropertyToID("_PhiMin");
    readonly int _phiMaxID = Shader.PropertyToID("_PhiMax");
#endregion

    public OrbitShader(Renderer renderer, DangerZone data)
    {
        _renderer = renderer;
        _data = data;
        _propertyBlock = new MaterialPropertyBlock();
        Apply();
    }
    public void Apply()
    {
        _renderer?.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetFloat(_thetaMinID, _data.thetaMin);
        _propertyBlock.SetFloat(_thetaMaxID, _data.thetaMax);
        _propertyBlock.SetFloat(_phiMinID, _data.phiMin);
        _propertyBlock.SetFloat(_phiMaxID, _data.phiMax);

        _renderer?.SetPropertyBlock(_propertyBlock);
    }
    public void SetData(DangerZone data)
    {
        _data = data;
        Apply();
    }
    public void SetTetha(float min, float max)
    {
        _data.thetaMin = min;
        _data.thetaMax = max;
        Apply();
    }
    public void SetPhi(float min, float max)
    {
        _data.phiMin = min;
        _data.phiMax = max;
        Apply();
    }
}