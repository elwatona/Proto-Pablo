using UnityEngine;

/// <summary>
/// Factory responsible for creating and initializing Astro instances (e.g. from pool + prefab).
/// </summary>
public interface IAstroFactory
{
    /// <summary>
    /// Creates an Astro of the given type at the specified position, optionally under a parent.
    /// Uses default OrbitData/BodyData for the type. Returns the Astro component (or null if creation failed).
    /// </summary>
    Astro Create(AstroType type, Vector3 position, Transform parent = null);

    /// <summary>
    /// Creates an Astro with explicit orbit and body data at the given position.
    /// When orbitData or bodyData is null, defaults for the given type are used.
    /// </summary>
    Astro Create(AstroType type, Vector3 position, OrbitData? orbitData, BodyData? bodyData, Transform parent = null);
}
