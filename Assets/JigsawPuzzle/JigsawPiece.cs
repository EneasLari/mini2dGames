using UnityEngine;
using UnityEngine.EventSystems;

public class JigsawPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [HideInInspector] public Vector2 correctPosition;
    RectTransform rect;
    Canvas canvas;

    void Awake() {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        correctPosition = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData) {
        // Raycast for another JigsawPiece under the pointer
        var pointerResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, pointerResults);

        foreach (var result in pointerResults) {
            if (result.gameObject == this.gameObject) continue; // Ignore self

            JigsawPiece otherPiece = result.gameObject.GetComponent<JigsawPiece>();
            if (otherPiece != null) {
                print(otherPiece.name);
                // Swap positions
                Vector2 tempPos = rect.anchoredPosition;
                rect.anchoredPosition = otherPiece.rect.anchoredPosition;
                otherPiece.rect.anchoredPosition = tempPos;

                // End drag here, don't snap to correct position automatically
                return;
            }
        }

        // If not dropped on another piece, snap to correct if close
        if (Vector2.Distance(rect.anchoredPosition, correctPosition) < 30f) {
            rect.anchoredPosition = correctPosition;
        }
    }

    public bool IsPlacedCorrectly() {
        return Vector2.Distance(rect.anchoredPosition, correctPosition) < 1f;
    }
}
