using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertyRow : MonoBehaviour
{
    [SerializeField] TMP_Text _label;
    [SerializeField] Slider _slider;
    [SerializeField] TMP_Text _value;

    private System.Action<float> _setter;

    public void Bind(PropertyDefinition property)
    {
        _label.text = property.name;
        _slider.minValue = property.min;
        _slider.maxValue = property.max;
        _slider.wholeNumbers = property.wholeNumbers;
        _slider.SetValueWithoutNotify(property.value);

        _value.text = FormatValue(property.value, property.wholeNumbers);

        _setter = property.setter;
        _slider.onValueChanged.AddListener(OnValueChanged);
    }

    public void Unbind()
    {
        _slider.onValueChanged.RemoveListener(OnValueChanged);
        _setter = null;
    }

    void OnValueChanged(float value)
    {
        _setter?.Invoke(value);
        _value.text = FormatValue(value, _slider.wholeNumbers);
    }

    static string FormatValue(float value, bool wholeNumbers)
    {
        return wholeNumbers ? value.ToString("0") : value.ToString("0.###");
    }
}
