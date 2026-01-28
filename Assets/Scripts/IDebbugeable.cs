
public interface IDebbugeable<T> where T : struct
{
    public T Data {get;}
    public void Selected();
    public void Deselected();
    public void UpdateData(T data);
    public void Deactivate();
}
public interface IMoonEditable
{
    MoonData Data {get;}
    void SetBaseRadius(float value);
    void SetOrbitRadius(float value);
    void SetGravity(float value);
    void SetRotationSpeed(float value);
    void Deactivate();
    void Selected();
    void Deselected();
}
