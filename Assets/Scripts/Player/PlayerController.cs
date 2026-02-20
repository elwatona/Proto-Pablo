using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Astro Creation")]
    [SerializeField] AstroFactory _astroFactory;

    [Header("Settings")]
    [SerializeField] GameObject _orbGameObject;
    [SerializeField] Orb _orb;
    [SerializeField] UIManager _uiManager;
    void Awake()
    {
        CacheReferences();
    }

    void CacheReferences()
    {
        if (!_orbGameObject) _orbGameObject = transform.Find("Orb").gameObject;
        if (!_orb) _orb = _orbGameObject?.GetComponent<Orb>();
    }

    public void SetSpawnPoint(InputAction.CallbackContext context)
    {
        if(_orbGameObject.activeSelf || !context.started) return;
        
        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        gameObject.transform.position = cursorWorldPosition;
    }
    public void Aim(InputAction.CallbackContext context)
    {
        if (context.started)
            _orb.SetAiming(true);
        else if (context.canceled)
            _orb.SetAiming(false);
    }
    public void Loose(InputAction.CallbackContext context)
    {
        if(!context.started) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0;

        _orb.Loose(cursorWorldPosition);
    }
    public void Respawn(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (HasModifiers(context)) _orbGameObject.SetActive(false);
        if (!_orbGameObject.activeSelf)
        {
            _orbGameObject.transform.localPosition = Vector3.zero;
            _orbGameObject.SetActive(true);
        }
    }

    /// <summary>True if the input that triggered the action had Shift, Ctrl or Alt pressed.</summary>
    private static bool HasModifiers(InputAction.CallbackContext context)
    {
        var keyboard = context.control?.device as Keyboard;
        if (keyboard == null)
            keyboard = Keyboard.current;
        if (keyboard == null)
            return false;
        return keyboard.altKey.isPressed;
    }
    public void CreateAstro(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (_astroFactory == null) return;

        AstroType type = GetAstroTypeFromBinding(context);
        if (type == AstroType.None) return;

        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorWorldPosition.z = 0f;

        _astroFactory.Create(type, cursorWorldPosition);
    }
    public void TogglePanel(InputAction.CallbackContext context)
    {
        if(!context.started) return;
        int panelIndex = GetPanelIndexFromBinding(context);
        if(panelIndex == -1) return;
        _uiManager.TogglePanel(panelIndex);
    }

    /// <summary>
    /// Obtiene el AstroType según el control que disparó la acción (p. ej. tecla 1=Planet, 2=Asteroid, 3=Sun).
    /// </summary>
    private static AstroType GetAstroTypeFromBinding(InputAction.CallbackContext context)
    {
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "1" => AstroType.Planet,
            "2" => AstroType.Asteroid,
            "3" => AstroType.Sun,
            _ => AstroType.Planet
        };
    }
    private static int GetPanelIndexFromBinding(InputAction.CallbackContext context)
    {
        string displayName = context.control?.displayName ?? "";
        return displayName switch
        {
            "F2" => 0,
            "F3" => 1,
            _ => -1
        };
    }
}
