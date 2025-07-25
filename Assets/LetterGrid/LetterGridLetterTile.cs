using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LetterGridLetterTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, ICanvasRaycastFilter {
    private char tileLetter;
    public bool isSelected = false;
    private Vector2Int tilePosition;

    [SerializeField] private float smallerTriggerAreaPercentage = 0.7f;
    private float currentTriggerAreaPercentage;

    public enum TileType { Normal, DoubleLetter, TripleWord }
    public TileType tileType = TileType.Normal;

    [HideInInspector] public bool IsFound = false;

    void Start() {
        currentTriggerAreaPercentage = 1f;
    }

    public void SetLetter(char newLetter) {
        tileLetter = newLetter;
        GetComponentInChildren<TMP_Text>().text = tileLetter.ToString();
    }

    public void ResetTriggerAreaPercentage() {
        currentTriggerAreaPercentage = 1f;
    }

    public void SmallerTriggerAreaPercentage() {
        currentTriggerAreaPercentage = smallerTriggerAreaPercentage;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (LetterGridGameManager.Instance.wordManager.IsShowingFeedback) return;

        if (!LetterGridGameManager.Instance.wordManager.IsUserSelecting) {
            LetterGridGameManager.Instance.wordManager.StartSelection(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        LetterGridGameManager.Instance.wordManager.TrySelectHoveredTile(this);
    }

    public void SelectTile() {
        isSelected = true;
    }

    public void Deselect() {
        isSelected = false;
    }

    public void SetTilePos(int x, int y) {
        tilePosition = new Vector2Int(x, y);
    }

    public Vector2Int GetTilePos() {
        return tilePosition;
    }

    public void SetCurrentColor(Color newColor) {
        GetComponent<Image>().color = newColor;
    }

    public Color GetCurrentColor() {
        return GetComponent<Image>().color;
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPoint,
            eventCamera,
            out Vector2 localPoint
        );

        Vector2 pivot = rectTransform.pivot;
        Rect rect = rectTransform.rect;

        float effectiveWidth = rect.width * currentTriggerAreaPercentage;
        float effectiveHeight = rect.height * currentTriggerAreaPercentage;

        float xMin = -effectiveWidth / 2f;
        float xMax = effectiveWidth / 2f;
        float yMin = -effectiveHeight / 2f;
        float yMax = effectiveHeight / 2f;

        return localPoint.x >= xMin && localPoint.x <= xMax &&
               localPoint.y >= yMin && localPoint.y <= yMax;
    }
}
