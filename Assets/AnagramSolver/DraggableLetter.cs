using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private Transform parentAfterDrag;
    private CanvasGroup canvasGroup;

    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        parentAfterDrag = transform.parent; // Store original parent
        transform.SetParent(transform.root); // Move to top UI layer
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Allow detection by DropSlot
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition; // Follow the cursor
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == transform.root) // If not dropped into a slot
        {
            transform.SetParent(parentAfterDrag); // Reset to original position
        }
    }
}
