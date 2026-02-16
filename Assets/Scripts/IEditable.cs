using System.Collections.Generic;

public interface IEditable
{
    string DisplayName { get; }
    List<PropertyDefinition> GetProperties();
    void Selected();
    void Deselected();
    void Deactivate();
}
