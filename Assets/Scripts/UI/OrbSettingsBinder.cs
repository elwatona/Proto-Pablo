using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OrbSettingsBinder : MonoBehaviour
{
    [SerializeField] Orb _orb;
    [SerializeField] TMP_Dropdown _escapeModeDropdown;
    [SerializeField] Slider _escapeForceSlider;
    [SerializeField] TMP_Text _escapeForceLabel;
    [SerializeField] TMP_Text _orbStatusLabel;

    void OnEnable()
    {
        if (_orb == null) return;

        if (_escapeModeDropdown != null)
        {
            _escapeModeDropdown.SetValueWithoutNotify((int)_orb.EscapeMode);
            _escapeModeDropdown.onValueChanged.AddListener(OnEscapeModeChanged);
        }

        if (_escapeForceSlider != null)
        {
            _escapeForceSlider.SetValueWithoutNotify(_orb.EscapeForce);
            _escapeForceSlider.onValueChanged.AddListener(OnEscapeForceChanged);
            RefreshEscapeForceLabel(_orb.EscapeForce);
        }
        Orb.OnSpawn += UpdateOrbStatusLabel;
        Orb.OnDespawn += UpdateOrbStatusLabel;
    }

    void OnDisable()
    {
        if (_escapeModeDropdown != null)
            _escapeModeDropdown.onValueChanged.RemoveListener(OnEscapeModeChanged);
        if (_escapeForceSlider != null)
            _escapeForceSlider.onValueChanged.RemoveListener(OnEscapeForceChanged);
        Orb.OnSpawn -= UpdateOrbStatusLabel;
        Orb.OnDespawn -= UpdateOrbStatusLabel;
    }
    void Start()
    {
        UpdateOrbStatusLabel();
    }
    void OnEscapeModeChanged(int index)
    {
        if (_orb != null)
            _orb.SetEscapeMode((EscapeMode)index);
    }

    void OnEscapeForceChanged(float value)
    {
        if (_orb != null)
        {
            _orb.SetEscapeForce(value);
            RefreshEscapeForceLabel(value);
        }
    }

    void RefreshEscapeForceLabel(float value)
    {
        if (_escapeForceLabel != null)
            _escapeForceLabel.text = value.ToString("0.##");
    }
    void UpdateOrbStatusLabel()
    {
        if (_orbStatusLabel == null) return;
        _orbStatusLabel.text = GetOrbStatusText(_orb.gameObject.activeSelf);
    }
    string GetOrbStatusText(bool isActive)
    {
        return isActive ? "<color=green>Alive" : "<color=red>Dead";
    }
}
