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

        RectTransform myRect = rect;
        JigsawPiece bestOverlap = null;
        float maxOverlapArea = 0f;

        foreach (var other in manager.pieces) {
            if (other == this || other.isLocked) continue;
            RectTransform otherRect = other.GetComponent<RectTransform>();

            float overlapArea = GetRectOverlapArea(myRect, otherRect);
            if (overlapArea > maxOverlapArea) {
                maxOverlapArea = overlapArea;
                bestOverlap = other;
            }
        }

        if (bestOverlap != null && maxOverlapArea > 0f) {
            // Swap grid indices
            int tempIndex = currentGridIndex;
            currentGridIndex = bestOverlap.currentGridIndex;
            bestOverlap.currentGridIndex = tempIndex;

            // Snap both pieces to their new grid slots
            rect.anchoredPosition = manager.gridPositions[currentGridIndex];
            bestOverlap.rect.anchoredPosition = manager.gridPositions[bestOverlap.currentGridIndex];

            // After swapping, check if either is now in its correct slot and lock if so
            if (IsPlacedCorrectly()) {
                Lock();
            } else {
                transform.localScale = smallScale; // Not correct, shrink back
            }
            if (bestOverlap.IsPlacedCorrectly()) bestOverlap.Lock();

            return;
        }

        // No overlap—snap back to grid slot
        rect.anchoredPosition = manager.gridPositions[currentGridIndex];
        if (IsPlacedCorrectly()) {
            Lock();
        } else {
            transform.localScale = smallScale; // Not correct, shrink back
        }
    }

    float GetRectOverlapArea(RectTransform a, RectTransform b) {
        Vector3[] aWorld = new Vector3[4];
        Vector3[] bWorld = new Vector3[4];
        a.GetWorldCorners(aWorld);
        b.GetWorldCorners(bWorld);

        Rect rectA = new Rect(aWorld[0], aWorld[2] - aWorld[0]);
        Rect rectB = new Rect(bWorld[0], bWorld[2] - bWorld[0]);

        if (!rectA.Overlaps(rectB))
            return 0f;

        float xMin = Mathf.Max(rectA.xMin, rectB.xMin);
        float xMax = Mathf.Min(rectA.xMax, rectB.xMax);
        float yMin = Mathf.Max(rectA.yMin, rectB.yMin);
        float yMax = Mathf.Min(rectA.yMax, rectB.yMax);

        float width = Mathf.Max(0, xMax - xMin);
        float height = Mathf.Max(0, yMax - yMin);

        return width * height;
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
