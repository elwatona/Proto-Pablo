using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _inspectorPanel;
    [SerializeField] Transform _propertyContainer;
    [SerializeField] GameObject _orbiterPanel;
    [SerializeField] Transform _orbiterPropertyContainer;
    [SerializeField] OrbiterTargetsView _orbiterTargetsView;
    [SerializeField] GameObject _controlsPanel;
    [SerializeField] GameObject _debugPanel;
    [SerializeField] TextMeshProUGUI _debugText;
    [SerializeField] PropertyRow _rowPrefab;
    [SerializeField] GroupHeaderView _groupHeaderPrefab;
    [SerializeField] TextMeshProUGUI _version;

    private PanelController _panelController;
    private PanelController _orbiterPanelController;
    private IEditable _currentTarget;
    private bool _isPickingTargetForOrbiter;

    void Awake()
    {
        _panelController = new PanelController(_propertyContainer, _rowPrefab, _groupHeaderPrefab);
        _orbiterPanelController = new PanelController(_orbiterPropertyContainer, _rowPrefab, _groupHeaderPrefab);
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
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(false);
    }

    public void SelectTarget(IEditable target)
    {
        if (_isPickingTargetForOrbiter)
        {
            var orbiter = (_currentTarget as Component)?.GetComponent<TransformOrbiter>();
            if (orbiter != null && target is Component comp)
            {
                orbiter.AddTarget(comp.transform);
                _orbiterTargetsView?.Refresh();
            }
            _isPickingTargetForOrbiter = false;
            return;
        }

        _currentTarget?.Deselected();

        if (!_inspectorPanel.activeSelf)
            _inspectorPanel.SetActive(true);

        _currentTarget = target;
        _currentTarget.Selected();
        _panelController.Bind(target);

        var targetOrbiter = (target as Component)?.GetComponent<TransformOrbiter>();
        if (targetOrbiter != null)
        {
            _orbiterPanelController.Bind(targetOrbiter.GetProperties());
            _orbiterTargetsView?.Bind(targetOrbiter);
            if (_orbiterPanel != null)
                _orbiterPanel.SetActive(true);
        }
        else
        {
            _orbiterPanelController.Clear();
            _orbiterTargetsView?.Clear();
            if (_orbiterPanel != null)
                _orbiterPanel.SetActive(false);
        }
    }

    public void StartPickingTargetForOrbiter()
    {
        var orbiter = (_currentTarget as Component)?.GetComponent<TransformOrbiter>();
        _isPickingTargetForOrbiter = orbiter != null;
    }

    public void CancelPickingTargetForOrbiter()
    {
        _isPickingTargetForOrbiter = false;
    }

    public void ClosePanel()
    {
        _isPickingTargetForOrbiter = false;
        _currentTarget?.Deselected();
        _panelController.Clear();
        _orbiterPanelController.Clear();
        _orbiterTargetsView?.Clear();
        _inspectorPanel.SetActive(false);
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(false);
        _currentTarget = null;
    }
    public void TogglePanel(int index)
    {
        switch (index)
        {
            case 0: _controlsPanel.SetActive(!_controlsPanel.activeSelf); break;
            case 1: _debugPanel.SetActive(!_debugPanel.activeSelf); break;
        };
    }

    void UpdateDebug(float speed, EscapeMode escapeMode)
    {
        if (!_debugPanel.activeSelf)
            return;

        _debugText.text = $"Speed: {speed:0.##}";
    }

    public void DeleteTarget()
    {
        _isPickingTargetForOrbiter = false;
        _panelController.Clear();
        _orbiterPanelController.Clear();
        _orbiterTargetsView?.Clear();
        _inspectorPanel.SetActive(false);
        if (_orbiterPanel != null)
            _orbiterPanel.SetActive(false);
        _currentTarget?.Deselected();
        _currentTarget?.Deactivate();
        _currentTarget = null;
    }
}
