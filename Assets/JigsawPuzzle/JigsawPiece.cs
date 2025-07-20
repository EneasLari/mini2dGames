using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class JigsawPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler , IPointerUpHandler{
    [HideInInspector] public int correctGridIndex;
    [HideInInspector] public int currentGridIndex;
    [HideInInspector] public JigsawManager manager;

    public readonly Vector3 smallScale = Vector3.one * 0.9f;
    public readonly Vector3 fullScale = Vector3.one;
    public bool isLocked = false;
    public bool draggingStarted = false;


    private RectTransform rect;
    private Canvas canvas;
    

    void Awake() {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (isLocked) return;
        transform.localScale = fullScale; // Show at full size during drag
        manager.PlayClickSound();
    }
    public void OnPointerUp(PointerEventData eventData) {
        if (isLocked || draggingStarted) return;
        transform.localScale = smallScale;
    }
    public void OnBeginDrag(PointerEventData eventData) {
        if (isLocked) return;
        draggingStarted = true;
        transform.SetAsLastSibling();
    }


    public void OnDrag(PointerEventData eventData) {
        if (isLocked) return;
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (isLocked) return;
        draggingStarted = false;
        var pointerResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, pointerResults);

        foreach (var result in pointerResults) {
            if (result.gameObject == this.gameObject) continue;

            JigsawPiece otherPiece = result.gameObject.GetComponent<JigsawPiece>();
            if (otherPiece != null && !otherPiece.isLocked) { // Can't swap with locked piece!
                // Swap grid indices
                int tempIndex = currentGridIndex;
                currentGridIndex = otherPiece.currentGridIndex;
                otherPiece.currentGridIndex = tempIndex;

                // Snap both pieces to their new grid slots
                rect.anchoredPosition = manager.gridPositions[currentGridIndex];
                otherPiece.rect.anchoredPosition = manager.gridPositions[otherPiece.currentGridIndex];

                // After swapping, check if either is now in its correct slot and lock if so
                if (IsPlacedCorrectly()) {
                    Lock();
                } else {
                    transform.localScale = smallScale; // Not correct, shrink back
                }
                if (otherPiece.IsPlacedCorrectly()) otherPiece.Lock();

                return;
            }
        }
        // Snap back to grid slot after drag
        rect.anchoredPosition = manager.gridPositions[currentGridIndex];

        // After manual drop, check if now in correct slot and lock if so
        if (IsPlacedCorrectly()) {
            Lock();
        } else {
            transform.localScale = smallScale; // Not correct, shrink back
        }
    }

    public bool IsPlacedCorrectly() {
        return currentGridIndex == correctGridIndex;
    }

    private void Lock() {
        isLocked = true;
        StartCoroutine(ScaleUp());
        manager.PlayCorrectSound(); // <-- play correct sound here
    }

    private IEnumerator ScaleUp() {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one;
        float duration = 0.15f;
        float elapsed = 0f;
        while (elapsed < duration) {
            transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = endScale;
    }


}
