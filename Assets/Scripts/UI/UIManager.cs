using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _inspectorPanel;
    [SerializeField] GameObject _controlsPanel;
    [SerializeField] GameObject _debugPanel;
    [SerializeField] TextMeshProUGUI _debugText;
    [SerializeField] Transform _propertyContainer;
    [SerializeField] PropertyRow _rowPrefab;
    [SerializeField] GroupHeaderView _groupHeaderPrefab;
    [SerializeField] TextMeshProUGUI _version;

    private PanelController _panelController;
    private IEditable _currentTarget;

    void Awake()
    {
        _panelController = new PanelController(_propertyContainer, _rowPrefab, _groupHeaderPrefab);
        Astro.OnAstroClicked += SelectTarget;
        Orb.OnDebugUpdate += UpdateDebug;

        if (_version)
            _version.text = $"Version {Application.version} \n Unity {Application.unityVersion}";
    }
    void OnDestroy()
    {
        Astro.OnAstroClicked -= SelectTarget;
        Orb.OnDebugUpdate -= UpdateDebug;
    }
    void Start()
    {
        _inspectorPanel.SetActive(false);
    }

    public void SelectTarget(IEditable target)
    {
        _currentTarget?.Deselected();

        if (!_inspectorPanel.activeSelf)
            _inspectorPanel.SetActive(true);

        _currentTarget = target;
        _currentTarget.Selected();
        _panelController.Bind(target);
    }

    public void ClosePanel()
    {
        _currentTarget?.Deselected();
        _panelController.Clear();
        _inspectorPanel.SetActive(false);
        _currentTarget = null;
    }

    public void ToggleInfo() => _controlsPanel.SetActive(!_controlsPanel.activeSelf);
    public void ToggleDebug() => _debugPanel.SetActive(!_debugPanel.activeSelf);

    void UpdateDebug(float speed, EscapeMode escapeMode)
    {
        if (!_debugPanel.activeSelf)
            return;

        _debugText.text = $"Speed: {speed:0.##}";
    }

    public void DeleteTarget()
    {
        _panelController.Clear();
        _inspectorPanel.SetActive(false);
        _currentTarget?.Deselected();
        _currentTarget?.Deactivate();
        _currentTarget = null;
    }
}
