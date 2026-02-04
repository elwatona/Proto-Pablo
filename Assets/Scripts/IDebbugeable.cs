public interface IDebbugeable<T> where T : struct
{
    public T Data {get;}
    public void Selected();
    public void Deselected();
    public void UpdateData(T data);
    public void Deactivate();
}
