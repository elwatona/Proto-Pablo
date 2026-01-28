using UnityEngine;

public class MoonController : MonoBehaviour
{
    [SerializeField] OrbitData _orbitData;
    [SerializeField] float _baseRadius = 1f;
    [SerializeField] float _rotationSpeed = 5f;

    [SerializeField] Transform _transform, _baseTransform, _orbitTransform;
    private IOrbitable _orbit;

    void Awake()
    {
        CacheReferences();
    }
    void OnValidate()
    {
        CacheReferences();
        UpdateBaseValues();
        UpdateOrbitValues();
    }
    void Update()
    {
        _transform.Rotate(Vector3.forward * _rotationSpeed * Time.deltaTime);
    }

    void CacheReferences()
    {
        if(!_transform) _transform = transform;
        if(!_baseTransform) _baseTransform = _transform.Find("Base");
        if(!_orbitTransform) _orbitTransform = _transform.Find("Orbit");
        if(_orbit == null) _orbit = _orbitTransform?.GetComponent<IOrbitable>();
    }
    void UpdateBaseValues()
    {
        float diameter = _baseRadius * 2f;
        _baseTransform.localScale = Vector3.one * diameter;
    }
    void UpdateOrbitValues()
    {
        float diameter = _orbitData.radius * 2f;
        _orbitTransform.localScale = Vector3.one * diameter;
        _orbit?.SetData(_orbitData);
    }
}
