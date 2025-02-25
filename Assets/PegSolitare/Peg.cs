using UnityEngine;
using UnityEngine.EventSystems;

public class Peg : MonoBehaviour, IPointerClickHandler {
    public Vector2Int boardPosition; // Set when the peg is placed

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private BoardManager boardManager;

    private void Start() {
        boardManager = FindFirstObjectByType<BoardManager>();
        // Store the original parent and local position (relative to the parent)
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
    }

    public void OnPointerClick(PointerEventData eventData) {
        boardManager.SelectPeg(this);
    }

    public void MoveTo(Vector2 newPosition) {
        transform.position = newPosition;
    }

    public void ResetPosition() {
        // Reparent to the original parent and reset local position.
        transform.SetParent(originalParent);
        transform.localPosition = originalLocalPosition;
    }
}
