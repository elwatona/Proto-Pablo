using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoonController : MonoBehaviour, IPointerDownHandler, IMoonEditable, IDragHandler
{
    public static event Action<IMoonEditable> OnMoonClicked;
    
    [SerializeField] OrbitData _orbitData;
    [SerializeField] BaseData _baseData;
    [SerializeField] float _rotationSpeed = 5f;

    [SerializeField] Transform _transform, _baseTransform, _orbitTransform;
    private IOrbitable _orbit;
    private BaseShaderController _baseShaderController;
    private bool _isSelected;

    public MoonData Data 
    {
        get 
        {
            MoonData data = new MoonData()
            {
                orbitData = _orbitData,
                baseData = _baseData,
                rotationSpeed = _rotationSpeed
            };
            return data;
        }
    }

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
        if(_baseShaderController == null) _baseShaderController = new BaseShaderController(_baseTransform?.GetComponent<Renderer>());
    }
    void UpdateBaseValues()
    {
        float diameter = _baseData.radius * 2f;
        Color desiredColor = _isSelected ? _baseData.selectedColor : _baseData.baseColor;

        _baseTransform.localScale = Vector3.one * diameter;
        _baseShaderController?.SetColor(desiredColor);
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

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 desiredPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        _transform.position = desiredPosition;
    }

    public void SetBaseRadius(float value)
    {
        _baseData.radius = value;
        UpdateBaseValues();
    }

    public void SetOrbitRadius(float value)
    {
        _orbitData.radius = value;
        UpdateOrbitValues();
    }

    public void SetGravity(float value)
    {
        _orbitData.gravity = value;
        UpdateOrbitValues();
    }

    public void SetRotationSpeed(float value)
    {
        _rotationSpeed = value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) return;

        OnMoonClicked?.Invoke(this);
    }
}
[Serializable]
public struct BaseData
{
    [Range(0.5f, 7.5f)] public float radius;
    public Color baseColor;
    public Color selectedColor;
}
public struct MoonData
{
    public OrbitData orbitData;
    public BaseData baseData;
    public float rotationSpeed;

}
