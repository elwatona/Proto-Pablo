using TMPro;
using UnityEngine;

public class GroupHeaderView : MonoBehaviour
{
    [SerializeField] TMP_Text _label;

    public void SetTitle(string title)
    {
        if (_label != null)
            _label.text = title;
    }
}
