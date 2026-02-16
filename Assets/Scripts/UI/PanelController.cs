using System.Collections.Generic;
using UnityEngine;

public class PanelController
{
    readonly Transform _container;
    readonly PropertyRow _rowPrefab;
    readonly GroupHeaderView _groupHeaderPrefab;
    readonly List<GameObject> _instantiated = new();

    public PanelController(Transform container, PropertyRow rowPrefab, GroupHeaderView groupHeaderPrefab = null)
    {
        _container = container;
        _rowPrefab = rowPrefab;
        _groupHeaderPrefab = groupHeaderPrefab;
    }

    public void Bind(IEditable target)
    {
        Clear();

        List<PropertyDefinition> properties = target.GetProperties();
        string lastGroup = null;

        foreach (PropertyDefinition property in properties)
        {
            if (!string.IsNullOrEmpty(property.group) && property.group != lastGroup)
            {
                lastGroup = property.group;
                if (_groupHeaderPrefab != null)
                {
                    GroupHeaderView header = Object.Instantiate(_groupHeaderPrefab, _container);
                    header.SetTitle(property.group);
                    header.gameObject.SetActive(true);
                    _instantiated.Add(header.gameObject);
                }
            }

            PropertyRow row = Object.Instantiate(_rowPrefab, _container);
            row.Bind(property);
            row.gameObject.SetActive(true);
            _instantiated.Add(row.gameObject);
        }
    }

    public void Clear()
    {
        foreach (GameObject go in _instantiated)
        {
            if (go.TryGetComponent(out PropertyRow row))
                row.Unbind();
            Object.Destroy(go);
        }

        _instantiated.Clear();
    }
}
