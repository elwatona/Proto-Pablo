using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetRowView : MonoBehaviour
{
    [SerializeField] Button _removeButton;

    private int _index;
    private Action<int> _onRemoveRequested;

    public void Setup(int index, Action<int> onRemoveRequested)
    {
        _index = index;
        _onRemoveRequested = onRemoveRequested;

        if (_removeButton == null)
            _removeButton = GetComponentInChildren<Button>();

        if (_removeButton != null)
        {
            _removeButton.onClick.RemoveAllListeners();
            _removeButton.onClick.AddListener(InvokeRemove);
        }
    }

    void InvokeRemove()
    {
        _onRemoveRequested?.Invoke(_index);
    }
}
