using TMPro;
using UnityEngine;
public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _inspectorPanelGameObject, _controlsGameObject;
    [SerializeField] TextMeshProUGUI _version;
    [SerializeField] PanelData _panelData;
    private PanelController _panelController;
    private IEditable _currentMoon;

    void Awake()
    {
        CacheReferences();
        Astro.OnAstroClicked += MoonClicked;
        _version.text = $"Version {Application.version} \n Unity {Application.unityVersion}";
    }
    private void OnDestroy()
    {
        Astro.OnAstroClicked -= MoonClicked;
    }
    void Start()
    {
        _inspectorPanelGameObject.SetActive(false);
    }

    void CacheReferences()
    {
        if(_panelController == null) _panelController = new PanelController(_panelData);
        if(!_inspectorPanelGameObject) _inspectorPanelGameObject = transform.Find("Inspector").gameObject;
        if(!_controlsGameObject) _controlsGameObject = transform.Find("Controls").gameObject;
        if(!_version) _version = transform.Find("Controls").GetComponent<TextMeshProUGUI>();
    }
    void MoonClicked(IEditable moon)
    {
        if(_currentMoon != null) _currentMoon.Deselected();
        if(!_inspectorPanelGameObject.activeSelf) _inspectorPanelGameObject.SetActive(true);

        _currentMoon = moon;
        _currentMoon.Selected();
        _panelController.SetMoon(moon);
    }

    public void ClosePanel() => _currentMoon?.Deselected();
    public void ToggleInfo() => _controlsGameObject.SetActive(!_controlsGameObject.activeSelf);
    public void DeleteMoon()
    {
        _inspectorPanelGameObject.SetActive(false);
        _currentMoon.Deselected();
        _currentMoon.Deactivate();
    }
}