using System;

public struct PropertyDefinition
{
    public readonly string group;
    public readonly string name;
    public readonly float min;
    public readonly float max;
    public readonly float value;
    public readonly bool wholeNumbers;
    public readonly Action<float> setter;

    public PropertyDefinition(string name, float min, float max, float value, Action<float> setter, bool wholeNumbers = false, string group = null)
    {
        this.group = group;
        this.name = name;
        this.min = min;
        this.max = max;
        this.value = value;
        this.setter = setter;
        this.wholeNumbers = wholeNumbers;
    }
}
