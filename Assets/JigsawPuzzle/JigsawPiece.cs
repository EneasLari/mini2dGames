using UnityEngine;
using UnityEngine.EventSystems;

public class JigsawPiece :  MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Vector2 correctPosition;
    RectTransform rect;
    Canvas canvas;

    void Awake() {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        correctPosition = rect.anchoredPosition;
    }

    void Start() {
        rect.anchoredPosition = new Vector2(Random.Range(-300, 300), Random.Range(-300, 300));
    }

    public void OnBeginDrag(PointerEventData eventData) {
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData) {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (Vector2.Distance(rect.anchoredPosition, correctPosition) < 30f) {
            rect.anchoredPosition = correctPosition;
        }
    }

    public bool IsPlacedCorrectly() {
        return Vector2.Distance(rect.anchoredPosition, correctPosition) < 1f;
    }
}
