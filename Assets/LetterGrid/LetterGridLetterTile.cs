using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LetterGridLetterTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, ICanvasRaycastFilter {
    private char letter;
    private bool isSelected = false;
    private Vector2Int tilePosition;
    [SerializeField] private float smallerTriggerAreaPercentage = 0.7f;
    private float triggerAreaPercentage; // 70% of tile area

    public enum TileType { Normal, DoubleLetter, TripleWord }
    public TileType tileType = TileType.Normal;
    [HideInInspector]public bool IsPartOfWord = false;

    void Start() {
        triggerAreaPercentage = smallerTriggerAreaPercentage;
    }

    public void SetLetter(char newLetter) {
        letter = newLetter;
        GetComponentInChildren<TMP_Text>().text = letter.ToString();
    }

    public void ResetTriggerAreaPercentage() {
        triggerAreaPercentage = 1;
    }

    public void SmallerTriggerAreaPercentage() {
        triggerAreaPercentage = smallerTriggerAreaPercentage;
    }

    // Handle tile clicks
    public void OnPointerDown(PointerEventData eventData) {
        if (LetterGridWordManager.instance.isFlashing) return;

        // Only start new selection if not already selecting
        if (!LetterGridWordManager.instance.isSelecting) {
            LetterGridWordManager.instance.StartSelection(this);
        }
    }

    // Handle drag enter
    public void OnPointerEnter(PointerEventData eventData) {
        if (Input.GetMouseButton(0) &&
            LetterGridWordManager.instance.isSelecting &&
            !isSelected &&
            LetterGridWordManager.instance.CanSelectTile(this)) {
            LetterGridWordManager.instance.AddLetter(this);
        }
    }

    public void SelectTile() {
        isSelected = true;
    }

    public void SetCurrentColor(Color newColor) {
        GetComponent<Image>().color = newColor;
    }

    public Color GetCurrentColor() {
        return GetComponent<Image>().color;
    }
    public void SetTilePos(int x, int y) {
        tilePosition.x = x;
        tilePosition.y = y;
    }

    public Vector2Int GetTilePos() {
        return tilePosition;
    }

    public void Deselect() {
        isSelected = false;     
    }
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        // Convert screen point to local coordinates
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            screenPoint,
            eventCamera,
            out localPoint
        );

        // Calculate the effective trigger area
        Vector2 pivot = rectTransform.pivot;
        Rect rect = rectTransform.rect;

        float effectiveWidth = rect.width * triggerAreaPercentage;
        float effectiveHeight = rect.height * triggerAreaPercentage;

        // Create a smaller rectangle centered in the tile
        float xMin = -effectiveWidth / 2f;
        float xMax = effectiveWidth / 2f;
        float yMin = -effectiveHeight / 2f;
        float yMax = effectiveHeight / 2f;

        // Check if point is within the effective area
        return localPoint.x >= xMin &&
               localPoint.x <= xMax &&
               localPoint.y >= yMin &&
               localPoint.y <= yMax;
    }

}