using UnityEngine;

public class LineRendererController
{
    readonly LineRenderer _lineRenderer;
    readonly Rigidbody _rb;
    readonly Transform _transform;
    
    public LineRendererController(LineRenderer lineRenderer, Rigidbody rb, Transform transform)
    {
        _lineRenderer = lineRenderer;
        _rb = rb;
        _transform = transform;
    }
    public void Update()
    {
        if (_rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        Vector3 start = _transform.position;
        Vector3 end = start + _rb.linearVelocity.normalized * 0.3f;

        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1, end);
    }
}
