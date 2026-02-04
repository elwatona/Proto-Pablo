using UnityEngine;
public class BodyShader
{
    readonly Renderer _renderer;
    readonly MaterialPropertyBlock _propertyBlock;
    private Color _baseColor;

    readonly int _baseColorID = Shader.PropertyToID("_Base_Color");

    public BodyShader(Renderer renderer)
    {
        _renderer = renderer;
        _propertyBlock = new MaterialPropertyBlock();
        Apply();
    }
    public void Apply()
    {
        _renderer.GetPropertyBlock(_propertyBlock);

        _propertyBlock.SetColor(_baseColorID, _baseColor);

        _renderer.SetPropertyBlock(_propertyBlock);
    }
    public void SetColor(Color color)
    {
        _baseColor = color;
        Apply();
    }
}