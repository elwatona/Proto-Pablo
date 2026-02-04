
public interface IEditable
{
    AstroData Data {get;}
    void SetBaseRadius(float value);
    void SetOrbitRadius(float value);
    void SetGravity(float value);
    void SetRotationSpeed(float value);
    void Deactivate();
    void Selected();
    void Deselected();
}