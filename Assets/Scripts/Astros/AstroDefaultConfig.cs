using UnityEngine;

/// <summary>
/// Centralized default OrbitData and BodyData per AstroType for factory-created Astros.
/// </summary>
public static class AstroDefaultConfig
{
    public static void GetDefaults(AstroType type, out OrbitData orbitData, out BodyData bodyData)
    {
        orbitData = GetDefaultOrbitData(type);
        bodyData = GetDefaultBodyData(type);
    }

    public static OrbitData GetDefaultOrbitData(AstroType type)
    {
        var data = new OrbitData { type = type };
        switch (type)
        {
            case AstroType.Planet:
                data.radius = 5f;
                data.gravity = 25f;
                data.tangentialForce = 3f;
                data.radialDamping = 0.75f;
                break;
            case AstroType.Asteroid:
                data.radius = 2f;
                data.gravity = 18f;
                data.tangentialForce = 4f;
                data.radialDamping = 0.9f;
                break;
            case AstroType.Sun:
                data.radius = 6f;
                data.gravity = 28f;
                data.tangentialForce = 2.5f;
                data.radialDamping = 0.7f;
                break;
            default:
                data.radius = 3f;
                data.gravity = 22f;
                data.tangentialForce = 3.5f;
                data.radialDamping = 0.85f;
                break;
        }
        return data;
    }

    public static BodyData GetDefaultBodyData(AstroType type)
    {
        var data = new BodyData();
        switch (type)
        {
            case AstroType.Planet:
                data.radius = 0.5f;
                data.baseColor = new Color(0.3f, 0.6f, 0.9f);
                data.selectedColor = new Color(0.5f, 0.8f, 1f);
                break;
            case AstroType.Asteroid:
                data.radius = 0.25f;
                data.baseColor = new Color(0.5f, 0.45f, 0.4f);
                data.selectedColor = new Color(0.7f, 0.65f, 0.6f);
                break;
            case AstroType.Sun:
                data.radius = 2f;
                data.baseColor = new Color(1f, 0.9f, 0.5f);
                data.selectedColor = new Color(1f, 0.95f, 0.7f);
                break;
            default:
                data.radius = 1.5f;
                data.baseColor = new Color(0.7f, 0.7f, 0.75f);
                data.selectedColor = new Color(0.9f, 0.9f, 1f);
                break;
        }
        return data;
    }
}
