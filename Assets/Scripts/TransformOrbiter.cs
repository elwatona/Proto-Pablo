using System.Collections.Generic;
using UnityEngine;

public class TransformOrbiter : MonoBehaviour
{
    [Header("Orbit")]
    [SerializeField] Transform[] _targets = new Transform[1];
    [Tooltip("Single target: semi-major axis. Multi target: extra clearance added to path margin.")]
    [SerializeField] float _radius = 5f;
    [Tooltip("Single target: mean motion (rad/s). Multi target: linear speed (world units/s) along path.")]
    [SerializeField] float _speed = 1f;
    [Header("Kepler (single target only)")]
    [Tooltip("Ellipse shape: 0 = circle, <1 = ellipse, higher = more elongated.")]
    [SerializeField, Range(0.01f, 0.9f)] float _eccentricity = 0.5f;
    [Header("Path (multi target)")]
    [Tooltip("Body transform for collision margin (e.g. Base child). If unset, finds child named \"Base\".")]
    [SerializeField] Transform _orbiterBodyTransform;

    const float PathMarginFactor = 0.5f;
    const float PathMarginMax = 20f;
    const int PathSampleMin = 48;
    const int PathSampleMax = 128;
    const float PathTargetSegmentLength = 0.6f;
    const int PathSmoothPassesBaseline = 80;
    const int PathSmoothPassesMax = 120;
    const float JaggednessThreshold = 0.08f;

    private float _meanAnomaly;
    private Vector3 _focus;
    private List<Vector2> _pathPoints = new List<Vector2>();
    private float[] _pathCumulativeLengths;
    private float _totalPathLength;
    private float _pathParameter;
    private bool _radiusInitializedFromDistance;
    private bool _orbitStateDirty = true;

    const int KeplerIterations = 5;

    bool HasValidTargets()
    {
        if (_targets == null || _targets.Length == 0)
            return false;
        for (int i = 0; i < _targets.Length; i++)
            if (_targets[i] == null)
                return false;
        return true;
    }

    bool UsePathMode() => HasValidTargets() && _targets.Length >= 2;

    void OnEnable()
    {
        CleanNullTargets();
        _orbitStateDirty = true;
        if (!HasValidTargets()) return;
        if (!_radiusInitializedFromDistance)
        {
            InitializeRadiusFromDistance();
            _radiusInitializedFromDistance = true;
        }
        if (UsePathMode())
        {
            BuildEnvelopePath();
            SetPathParameterFromPosition();
        }
        else
            SetValues();
        // Keep _orbitStateDirty true so first Update() syncs again with final target positions (avoids center at 0,0,0 when target positions apply after OnEnable).
    }

    void OnDisable()
    {
        _radiusInitializedFromDistance = false;
        _orbitStateDirty = true;
    }

    void InitializeRadiusFromDistance()
    {
        if (!HasValidTargets()) return;
        if (UsePathMode())
        {
            Vector3 center = GetBarycenter();
            float distanceToCenter = Vector2.Distance(transform.position, center);
            float systemRadius = GetSystemRadius(center);
            float minSafeMargin = GetMinimumSafeMargin();
            _radius = Mathf.Max(0f, distanceToCenter - systemRadius);
            _radius = Mathf.Max(_radius, minSafeMargin * 0.5f);
        }
        else
        {
            Vector3 focus = GetFocus();
            float distance = Vector2.Distance(transform.position, focus);
            float minSafeMargin = GetMinimumSafeMargin();
            float e = Mathf.Clamp(_eccentricity, 0.01f, 0.99f);
            float minSemiMajorAxis = minSafeMargin / (1f - e);
            _radius = Mathf.Max(distance, minSemiMajorAxis);
        }
    }
    void OnValidate()
    {
        ClampArray();
        if (!HasValidTargets()) return;
        if (UsePathMode())
        {
            BuildEnvelopePath();
            SetPathParameterFromPosition();
            ApplyPathOrbit();
        }
        else
        {
            SetValues();
            ApplyOrbit();
        }
    }
    void Update()
    {
        if (!HasValidTargets()) return;
        if (_orbitStateDirty)
        {
            SyncToTargets();
            _orbitStateDirty = false;
        }
        if (UsePathMode())
            ApplyPathOrbit();
        else
            ApplyOrbit();
    }

    /// <summary>Solve Kepler's equation M = E - e*sin(E) for eccentric anomaly E.</summary>
    static float MeanToEccentricAnomaly(float meanAnomaly, float e)
    {
        float E = meanAnomaly;
        for (int i = 0; i < KeplerIterations; i++)
        {
            float dE = (E - e * Mathf.Sin(E) - meanAnomaly) / (1f - e * Mathf.Cos(E));
            E -= dE;
            if (Mathf.Abs(dE) < 1e-6f) break;
        }
        return E;
    }

    /// <summary>True anomaly nu from eccentric anomaly E (radians).</summary>
    static float EccentricToTrueAnomaly(float E, float e)
    {
        float sqrt1PlusE = Mathf.Sqrt(1f + e);
        float sqrt1MinE = Mathf.Sqrt(1f - e);
        return 2f * Mathf.Atan2(sqrt1PlusE * Mathf.Sin(E * 0.5f), sqrt1MinE * Mathf.Cos(E * 0.5f));
    }

    /// <summary>Distance from focus at true anomaly nu; a = semi-major axis.</summary>
    static float RadiusAtTrueAnomaly(float semiMajorAxis, float e, float nu)
    {
        float oneMinE2 = 1f - e * e;
        return semiMajorAxis * oneMinE2 / (1f + e * Mathf.Cos(nu));
    }

    void ApplyOrbit()
    {
        float minSafeMargin = GetMinimumSafeMargin();
        float e = Mathf.Clamp(_eccentricity, 0.01f, 0.99f);
        float minSemiMajorAxis = minSafeMargin / (1f - e);
        float effectiveRadius = Mathf.Max(_radius, minSemiMajorAxis);
        _meanAnomaly += _speed * Time.deltaTime;
        float E = MeanToEccentricAnomaly(_meanAnomaly, _eccentricity);
        float nu = EccentricToTrueAnomaly(E, _eccentricity);
        float r = RadiusAtTrueAnomaly(effectiveRadius, _eccentricity, nu);

        float x = r * Mathf.Cos(nu);
        float y = r * Mathf.Sin(nu);
        Vector3 localPos = new Vector3(x, y, 0f);

        if (_targets.Length > 1 && _targets[0] != null && _targets[1] != null)
        {
            Vector3 dir = _targets[1].position - _targets[0].position;
            float orientation = Mathf.Atan2(dir.y, dir.x);
            float cos = Mathf.Cos(orientation);
            float sin = Mathf.Sin(orientation);
            localPos = new Vector3(
                localPos.x * cos - localPos.y * sin,
                localPos.x * sin + localPos.y * cos,
                0f
            );
        }

        transform.position = _focus + localPos;
    }

    Vector3 GetBarycenter()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null) continue;
            sum += _targets[i].position;
            count++;
        }
        return count > 0 ? sum / count : Vector3.zero;
    }

    float GetSystemRadius(Vector3 center)
    {
        float maxDist = 0f;
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null) continue;
            float d = Vector2.Distance(center, _targets[i].position);
            if (d > maxDist) maxDist = d;
        }
        return maxDist;
    }

    Transform GetBodyTransform(Transform root, bool isOrbiter)
    {
        if (root == null) return null;
        if (isOrbiter && _orbiterBodyTransform != null)
            return _orbiterBodyTransform;
        Transform baseChild = root.Find("Base");
        return baseChild != null ? baseChild : root;
    }

    float GetBodyRadius(Transform root, bool isOrbiter)
    {
        Transform body = GetBodyTransform(root, isOrbiter);
        if (body == null) return 0.5f;
        var astro = root.GetComponent<Astro>();
        if (astro != null) return astro.BodyRadius;
        Vector3 s = body.lossyScale;
        return 0.5f * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
    }

    float GetMinimumSafeMargin()
    {
        float orbiterRadius = GetBodyRadius(transform, true);
        float maxTargetRadius = 0f;
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] == null) continue;
            float r = GetBodyRadius(_targets[i], false);
            if (r > maxTargetRadius) maxTargetRadius = r;
        }
        float sumRadii = orbiterRadius + maxTargetRadius;
        return sumRadii * 1.02f + 0.02f;
    }

    void BuildEnvelopePath()
    {
        Vector3 center = GetBarycenter();
        float systemRadius = GetSystemRadius(center);
        float minSafeMargin = GetMinimumSafeMargin();
        float marginFromFactor = Mathf.Clamp(systemRadius * PathMarginFactor, 0f, PathMarginMax);
        float effectiveMargin = minSafeMargin + marginFromFactor + _radius;
        float estimatedPathLength = 2f * Mathf.PI * (systemRadius + effectiveMargin);

        int n = Mathf.Clamp(Mathf.CeilToInt(estimatedPathLength / PathTargetSegmentLength), PathSampleMin, PathSampleMax);

        float[] rawMaxR = new float[n];
        float[] radii = new float[n];
        for (int a = 0; a < n; a++)
        {
            float theta = 2f * Mathf.PI * a / n;
            float cos = Mathf.Cos(theta);
            float sin = Mathf.Sin(theta);
            float maxR = 0f;
            for (int i = 0; i < _targets.Length; i++)
            {
                if (_targets[i] == null) continue;
                Vector3 d = _targets[i].position - center;
                float r = d.x * cos + d.y * sin;
                if (r > maxR) maxR = r;
            }
            rawMaxR[a] = maxR;
            radii[a] = maxR + effectiveMargin;
        }

        float meanR = 0f;
        for (int a = 0; a < n; a++) meanR += radii[a];
        meanR = meanR / n;
        float jaggedness = 0f;
        for (int a = 0; a < n; a++)
        {
            int prev = (a + n - 1) % n;
            int prev2 = (a + n - 2) % n;
            float secondDiff = Mathf.Abs(radii[a] - 2f * radii[prev] + radii[prev2]);
            jaggedness += secondDiff;
        }
        jaggedness = (meanR > 0.001f) ? (jaggedness / n) / meanR : 0f;
        int smoothPasses = Mathf.Min(PathSmoothPassesMax, PathSmoothPassesBaseline + Mathf.RoundToInt(jaggedness / JaggednessThreshold));

        for (int pass = 0; pass < smoothPasses; pass++)
        {
            float[] nextRadii = new float[n];
            for (int a = 0; a < n; a++)
            {
                int p2 = (a + n - 2) % n;
                int p1 = (a + n - 1) % n;
                int n1 = (a + 1) % n;
                int n2 = (a + 2) % n;
                nextRadii[a] = (radii[p2] + 2f * radii[p1] + 2f * radii[a] + 2f * radii[n1] + radii[n2]) / 8f;
            }
            nextRadii.CopyTo(radii, 0);
        }
        for (int a = 0; a < n; a++)
        {
            float minRadius = rawMaxR[a] + minSafeMargin;
            if (radii[a] < minRadius) radii[a] = minRadius;
        }
        _pathPoints.Clear();
        for (int a = 0; a < n; a++)
        {
            float theta = 2f * Mathf.PI * a / n;
            _pathPoints.Add(new Vector2(radii[a] * Mathf.Cos(theta), radii[a] * Mathf.Sin(theta)));
        }
        _pathCumulativeLengths = new float[n + 1];
        _pathCumulativeLengths[0] = 0f;
        for (int a = 1; a <= n; a++)
        {
            int prev = a - 1;
            Vector2 p = _pathPoints[prev];
            Vector2 q = _pathPoints[a % n];
            _pathCumulativeLengths[a] = _pathCumulativeLengths[prev] + Vector2.Distance(p, q);
        }
        _totalPathLength = _pathCumulativeLengths[n];
        if (_totalPathLength < 0.0001f) _totalPathLength = 0.0001f;
    }

    Vector2 GetPositionOnPath(float s)
    {
        if (_pathPoints == null || _pathPoints.Count == 0 || _pathCumulativeLengths == null)
            return Vector2.zero;
        float t = Mathf.Repeat(s, 1f);
        float len = t * _totalPathLength;
        int n = _pathPoints.Count;
        int seg = 0;
        for (int i = 1; i <= n; i++)
        {
            if (_pathCumulativeLengths[i] >= len) { seg = i - 1; break; }
            seg = i - 1;
        }
        float segStart = _pathCumulativeLengths[seg];
        float segEnd = _pathCumulativeLengths[seg + 1];
        float segLen = segEnd - segStart;
        float u = segLen > 0.0001f ? (len - segStart) / segLen : 0f;
        Vector2 a = _pathPoints[seg];
        Vector2 b = _pathPoints[(seg + 1) % n];
        return Vector2.Lerp(a, b, u);
    }

    void SetPathParameterFromPosition()
    {
        if (_pathPoints == null || _pathPoints.Count == 0 || _totalPathLength < 0.0001f) return;
        Vector3 center = GetBarycenter();
        Vector2 local = new Vector2(transform.position.x - center.x, transform.position.y - center.y);
        int n = _pathPoints.Count;
        int bestSeg = 0;
        float bestT = 0f;
        float bestDistSq = float.MaxValue;
        for (int seg = 0; seg < n; seg++)
        {
            Vector2 a = _pathPoints[seg];
            Vector2 b = _pathPoints[(seg + 1) % n];
            Vector2 ab = b - a;
            float abLenSq = ab.sqrMagnitude;
            float t = abLenSq > 0.0001f ? Mathf.Clamp01(Vector2.Dot(local - a, ab) / abLenSq) : 0f;
            Vector2 pt = Vector2.Lerp(a, b, t);
            float dSq = (local - pt).sqrMagnitude;
            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;
                bestSeg = seg;
                bestT = t;
            }
        }
        float len = _pathCumulativeLengths[bestSeg] + bestT * (_pathCumulativeLengths[bestSeg + 1] - _pathCumulativeLengths[bestSeg]);
        _pathParameter = len / _totalPathLength;
    }

    void ApplyPathOrbit()
    {
        BuildEnvelopePath();
        float paramRate = _totalPathLength > 0.0001f ? _speed / _totalPathLength : 0f;
        _pathParameter += paramRate * Time.deltaTime;
        Vector2 localPos = GetPositionOnPath(_pathParameter);
        transform.position = GetBarycenter() + new Vector3(localPos.x, localPos.y, 0f);
    }

    Vector3 GetFocus()
    {
        if (_targets == null || _targets.Length == 0 || _targets[0] == null)
            return Vector3.zero;
        return _targets[0].position;
    }

    /// <summary>Reinitializes orbit state from current position and current targets. Call after assigning or changing targets at runtime.</summary>
    public void SyncToTargets()
    {
        if (!HasValidTargets()) return;
        if (UsePathMode())
        {
            BuildEnvelopePath();
            SetPathParameterFromPosition();
        }
        else
        {
            SetValues();
        }
    }

    void SetValues()
    {
        if (!HasValidTargets())
            return;

        _focus = GetFocus();

        Vector3 offset = transform.position - _focus;

        if (_targets.Length > 1 && _targets[0] != null && _targets[1] != null)
        {
            Vector3 dir = _targets[1].position - _targets[0].position;
            float orientation = Mathf.Atan2(dir.y, dir.x);
            float cos = Mathf.Cos(-orientation);
            float sin = Mathf.Sin(-orientation);
            offset = new Vector3(
                offset.x * cos - offset.y * sin,
                offset.x * sin + offset.y * cos,
                0f
            );
        }

        float r = new Vector2(offset.x, offset.y).magnitude;
        if (r < 0.0001f)
            return;

        float nu = Mathf.Atan2(offset.y, offset.x);
        float e = Mathf.Clamp(_eccentricity, 0.01f, 0.99f);
        float oneMinE2 = 1f - e * e;
        float cosNu = Mathf.Cos(nu);
        float denom = 1f + e * cosNu;
        if (Mathf.Abs(denom) < 0.001f)
            return;

        float aFromR = r * denom / oneMinE2;
        if (aFromR < 0.001f)
            return;

        float sinE = Mathf.Sqrt(1f - e * e) * Mathf.Sin(nu) / denom;
        float cosE = (e + cosNu) / denom;
        float E = Mathf.Atan2(sinE, cosE);
        _meanAnomaly = E - e * Mathf.Sin(E);
    }
    void ClampArray()
    {
        CleanNullTargets();
        if (_targets == null || _targets.Length == 0)
            _targets = new Transform[1];
    }

    void CleanNullTargets()
    {
        if (_targets == null || _targets.Length == 0) return;

        int count = 0;
        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] != null)
                count++;
        }

        if (count == 0)
        {
            _targets = new Transform[1];
            return;
        }

        if (count == _targets.Length)
            return;

        var cleaned = new Transform[count];
        int j = 0;
        for (int i = 0; i < _targets.Length && j < count; i++)
        {
            if (_targets[i] != null)
                cleaned[j++] = _targets[i];
        }
        _targets = cleaned;
    }

    /// <summary>Elimina referencias nulas del array de targets. Llamar antes de leer la lista (p. ej. desde la UI).</summary>
    public void EnsureTargetsClean()
    {
        CleanNullTargets();
        if (_targets == null || _targets.Length == 0)
            _targets = new Transform[1];
    }

    public int GetTargetCount() => _targets?.Length ?? 0;

    public Transform GetTarget(int index)
    {
        if (_targets == null || index < 0 || index >= _targets.Length)
            return null;
        return _targets[index];
    }

    public void AddTarget(Transform t)
    {
        if (t == null) return;
        int len = _targets != null ? _targets.Length : 0;
        var next = new Transform[len + 1];
        for (int i = 0; i < len; i++)
            next[i] = _targets[i];
        next[len] = t;
        _targets = next;
        SyncToTargets();
    }

    public void RemoveTargetAt(int index)
    {
        if (_targets == null || index < 0 || index >= _targets.Length) return;
        int len = _targets.Length;
        if (len <= 1)
        {
            _targets = new Transform[1];
            return;
        }
        var next = new Transform[len - 1];
        int j = 0;
        for (int i = 0; i < len; i++)
        {
            if (i != index)
                next[j++] = _targets[i];
        }
        _targets = next;
        if (HasValidTargets())
            SyncToTargets();
    }

    public List<PropertyDefinition> GetProperties()
    {
        const string group = "Transform Orbiter";
        var list = new List<PropertyDefinition>
        {
            new("Speed", 0.1f, 15f, _speed, value => _speed = value, group: group),
            new("Radius", 0.5f, 20f, _radius, value =>
            {
                _radius = value;
                if (UsePathMode())
                    BuildEnvelopePath();
                else
                    SetValues();
            }, group: group),
        };
        if (!UsePathMode())
        {
            list.Add(new("Eccentricity", 0.01f, 0.99f, _eccentricity, value =>
            {
                _eccentricity = value;
                SetValues();
            }, group: group));
        }
        return list;
    }
}
