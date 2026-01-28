using UnityEngine;
using UnityEngine.EventSystems;

public class Draggeable : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
}
