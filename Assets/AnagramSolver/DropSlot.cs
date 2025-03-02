using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        GameObject dropped = eventData.pointerDrag;

        if (dropped != null) {
            dropped.transform.SetParent(transform); // Set DropSlot as the new parent
            dropped.transform.position = transform.position; // Align it properly
        }
    }
}
