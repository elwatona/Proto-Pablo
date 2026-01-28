using TMPro;
using System;
using UnityEngine.UI;
public class PanelController
{
    readonly Slider _baseRadiusInput, _orbitRadiusInput, _gravityInput, _rotationInput;
    readonly TMP_Text _baseRadiusText, _orbitRadiusText, _gravityText, _rotationText;
    IMoonEditable _moon;
    public PanelController(PanelData data)
    {
        _baseRadiusInput = data.baseRadiusInput;
        _orbitRadiusInput = data.orbitRadiusInput;
        _gravityInput = data.gravityInput;
        _rotationInput = data.rotationInput;

        _baseRadiusText = data.baseRadiusText;
        _orbitRadiusText = data.orbitRadiusText;
        _gravityText = data.gravityText;
        _rotationText = data.rotationText;

        BindEvents();
    }
    void BindEvents()
    {
        _baseRadiusInput.onValueChanged.AddListener(OnBaseRadiusChanged);
        _orbitRadiusInput.onValueChanged.AddListener(OnOrbitRadiusChanged);
        _gravityInput.onValueChanged.AddListener(OnGravityChanged);
        _rotationInput.onValueChanged.AddListener(OnRotationChanged);
    }
    public void SetMoon(IMoonEditable moon)
    {
        _moon = moon;
        UpdateSliders(moon.Data);
        UpdateTexts();
    }
    public void UpdateTexts()
    {
        _baseRadiusText.text = _baseRadiusInput.value.ToString("0.###");
        _orbitRadiusText.text = _orbitRadiusInput.value.ToString("0.###");
        _gravityText.text = _gravityInput.value.ToString("0.###");
        _rotationText.text = _rotationInput.value.ToString("0.###");
    }
    void UpdateSliders(MoonData moonData)
    {
        _baseRadiusInput.SetValueWithoutNotify(moonData.baseData.radius);
        _orbitRadiusInput.SetValueWithoutNotify(moonData.orbitData.radius);
        _gravityInput.SetValueWithoutNotify(moonData.orbitData.gravity);
        _rotationInput.SetValueWithoutNotify(moonData.rotationSpeed);
    }
    void OnBaseRadiusChanged(float value)
    {
        _moon.SetBaseRadius(value);
        _baseRadiusText.text = value.ToString("0.###");
    }

    void OnOrbitRadiusChanged(float value)
    {
        _moon.SetOrbitRadius(value);
        _orbitRadiusText.text = value.ToString("0.###");
    }

    void OnGravityChanged(float value)
    {
        _moon.SetGravity(value);
        _gravityText.text = value.ToString("0.###");
    }

    void OnRotationChanged(float value)
    {
        _moon.SetRotationSpeed(value);
        _rotationText.text = value.ToString("0.###");
    }
}
[Serializable]
public struct PanelData
{
    public Slider baseRadiusInput, orbitRadiusInput, gravityInput, rotationInput;
    public TMP_Text baseRadiusText, orbitRadiusText, gravityText, rotationText;
}