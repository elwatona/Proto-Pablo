using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Astro : MonoBehaviour, IPointerDownHandler, IEditable, IDragHandler
{
    public static event Action<IEditable> OnAstroClicked;

    [Header("Settings")]
    [SerializeField] OrbitData _orbitData;
    [SerializeField] BodyData _bodyData;
    [SerializeField] float _rotationSpeed = 5f;

    [SerializeField] Transform _transform, _baseTransform, _orbitTransform;
    private IOrbitable _orbit;
    private BodyShader _baseShader;
    private TransformOrbiter _orbiter;
    private bool _isSelected;

    public string DisplayName => _orbitData.type.ToString();
    public float BodyRadius => _bodyData.radius;

    void Awake()
    {
        CacheReferences();
    }
    void OnEnable()
    {
        Apply();
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
        if(_baseShader == null) _baseShader = new BodyShader(_baseTransform?.GetComponent<Renderer>());
        if(_orbiter == null) _orbiter = GetComponent<TransformOrbiter>();
    }
    void UpdateBaseValues()
    {
        float diameter = _bodyData.radius * 2f;
        Color desiredColor = _isSelected ? _bodyData.selectedColor : _bodyData.baseColor;

        _baseTransform.localScale = Vector3.one * diameter;
        _baseShader?.SetColor(desiredColor);
    }
    void UpdateOrbitValues()
    {
        float diameter = _orbitData.radius * 2f;

        _orbitTransform.localScale = Vector3.one * diameter;
        _orbit?.SetData(_orbitData);
    }
    void Apply()
    {
        UpdateBaseValues();
        UpdateOrbitValues();
    }

    public List<PropertyDefinition> GetProperties()
    {
        var properties = new List<PropertyDefinition>
        {
            new("Body Radius", 0.5f, 7.5f, _bodyData.radius, value =>
            {
                _bodyData.radius = value;
                UpdateBaseValues();
            }, group: "Body"),
            new("Orbit Radius", 1f, 10f, _orbitData.radius, value =>
            {
                _orbitData.radius = value;
                UpdateOrbitValues();
            }, group: "Orbit"),
            new("Gravity", 15f, 30f, _orbitData.gravity, value =>
            {
                _orbitData.gravity = value;
                UpdateOrbitValues();
            }, group: "Orbit"),
            new("Tangential Force", 2f, 5f, _orbitData.tangentialForce, value =>
            {
                _orbitData.tangentialForce = value;
                UpdateOrbitValues();
            }, group: "Orbit"),
            new("Radial Damping", 0.5f, 1.5f, _orbitData.radialDamping, value =>
            {
                _orbitData.radialDamping = value;
                UpdateOrbitValues();
            }, group: "Orbit"),
            new("Rotation Speed", 0f, 20f, _rotationSpeed, value =>
            {
                _rotationSpeed = value;
            }, group: "Astro"),
        };

        return properties;
    }

    public void Selected()
    {
        _isSelected = true;
        Apply();
    }

    public void Deselected()
    {
        _isSelected = false;
        Apply();
    }

    /// <summary>
    /// Sets orbit and body data (e.g. when spawning from pool) and refreshes visuals. Type is defined by the prefab, not here.
    /// </summary>
    public void Initialize(OrbitData orbitData, BodyData bodyData)
    {
        _orbitData = orbitData;
        _bodyData = bodyData;
        Apply();
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 desiredPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        _transform.position = desiredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;

        OnAstroClicked?.Invoke(this);
    }
}
[Serializable]
public struct BodyData
{
    [Range(0.5f, 7.5f)] public float radius;
    public Color baseColor;
    public Color selectedColor;
}
