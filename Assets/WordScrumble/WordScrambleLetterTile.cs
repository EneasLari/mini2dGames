using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WordScrambleLetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private Transform parentAfterDrag;
    private Vector3 startPosition;
    private int startIndex;

    void Start() {
        parentAfterDrag = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startPosition = transform.position;
        startIndex = transform.GetSiblingIndex();
        transform.SetParent(parentAfterDrag.parent); // Temporarily move to parent to allow reordering
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition; // Move with the cursor
    }

    public void OnEndDrag(PointerEventData eventData) {
        transform.SetParent(parentAfterDrag);

        // Find nearest tile to swap position
        int closestIndex = GetClosestTileIndex();
        transform.SetSiblingIndex(closestIndex);
    }

    private int GetClosestTileIndex() {
        float minDistance = float.MaxValue;
        int closestIndex = startIndex;

        for (int i = 0; i < parentAfterDrag.childCount; i++) {
            Transform child = parentAfterDrag.GetChild(i);
            float distance = Vector3.Distance(transform.position, child.position);
            if (distance < minDistance && child != transform) {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
