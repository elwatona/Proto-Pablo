using UnityEngine;

public class TransformOrbiter : MonoBehaviour
{
    [SerializeField] Transform[] _targets = new Transform[1];
    [SerializeField] float _radius = 5f;
    [SerializeField] float _speed = 1f;
    [SerializeField, Range (0.01f, 1)] float _eccentricity = 0.5f;

    private float _angle;
    private Vector3 _center;
    private Vector2 _desiredRadius;

    void OnEnable()
    {
        SetValues();
    }
    void OnValidate()
    {
        ClampArray();
        SetValues();
        Orbit(_center, _desiredRadius);
    }
    void Update()
    {
        Orbit(_center, _desiredRadius);
    }
    void Orbit(Vector3 center, Vector2 radius)
    {
        _angle += _speed * Time.deltaTime;

        float x = Mathf.Cos(_angle) * radius.x;
        float y = Mathf.Sin(_angle) * radius.y;

        Vector3 localPos = new Vector3(x, y, 0);

        if (_targets.Length > 1)
        {
            Vector3 dir = _targets[1].position - _targets[0].position;
            float orientation = Mathf.Atan2(dir.y, dir.x);

            float cos = Mathf.Cos(orientation);
            float sin = Mathf.Sin(orientation);

            localPos = new Vector3(
                localPos.x * cos - localPos.y * sin,
                localPos.x * sin + localPos.y * cos,
                0
            );
        }

        transform.position = center + localPos;
    }
    Vector2 GetDesiredRadius()
    {
        if (_targets.Length == 1 ) return new Vector2(_radius, _radius);

        float x = Vector3.Distance(_targets[0].position, _targets[1].position) / 2f;
        float y = x * _eccentricity;

        return new Vector2(x, y);
    }
    Vector3 GetDesiredCenter()
    {
        if ( _targets.Length < 1 ) return Vector3.zero;
        if (_targets.Length > 1 ) return (_targets[0].position + _targets[1].position) / 2;
        return _targets[0].position;
    }
    void SetValues()
    {
        _desiredRadius = GetDesiredRadius();
        _center = GetDesiredCenter();
    }
    void ClampArray()
    {
        if (_targets == null || _targets.Length == 0)
        {
            _targets = new Transform[1];
        }
        else if (_targets.Length > 2)
        {
            System.Array.Resize(ref _targets, 2);
        }
    }
}
