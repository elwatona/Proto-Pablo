using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrbiterTargetsView : MonoBehaviour
{
    [SerializeField] Transform _container;
    [SerializeField] GameObject _rowPrefab;
    [SerializeField] GroupHeaderView _groupHeaderPrefab;

    private TransformOrbiter _orbiter;
    private readonly List<GameObject> _rows = new List<GameObject>();

    public void Bind(TransformOrbiter orbiter)
    {
        _orbiter = orbiter;
        Refresh();
    }

    public void Refresh()
    {
        foreach (GameObject go in _rows)
            Object.Destroy(go);
        _rows.Clear();

        if (_orbiter == null || _container == null || _rowPrefab == null) return;

        _orbiter.EnsureTargetsClean();

        bool worldPositionStays = false;

        if (_groupHeaderPrefab != null)
        {
            GroupHeaderView header = Object.Instantiate(_groupHeaderPrefab, _container, worldPositionStays);
            header.SetTitle("Targets");
            header.gameObject.SetActive(true);
            _rows.Add(header.gameObject);
        }

        int count = _orbiter.GetTargetCount();
        for (int i = 0; i < count; i++)
        {
            Transform t = _orbiter.GetTarget(i);
            string label = t != null ? t.name : "—";
            GameObject row = Object.Instantiate(_rowPrefab, _container, worldPositionStays);
            var tmp = row.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.text = $"Target {i}: {label}";

            var rowView = row.GetComponent<TargetRowView>();
            if (rowView == null)
                rowView = row.AddComponent<TargetRowView>();

            int index = i;
            rowView.Setup(index, _ => { _orbiter.RemoveTargetAt(index); Refresh(); });

            row.SetActive(true);
            _rows.Add(row);
        }

        if (_container is RectTransform contentRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    public void Clear()
    {
        _orbiter = null;
        foreach (GameObject go in _rows)
            Object.Destroy(go);
        _rows.Clear();
    }
}
