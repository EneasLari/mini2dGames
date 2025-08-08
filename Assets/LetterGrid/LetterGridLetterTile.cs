using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class LetterData {

    public LetterData(int x,int y) {
        TilePosition = new Vector2Int(x, y);
    }
    public char TileLetter { get; private set; }
    public Vector2Int TilePosition { get; private set; }


    public bool IsSelected { get; set; } = false;
    public bool IsFound { get; set; } = false;

    public enum LetterType { Normal, DoubleLetter, TripleWord }
    public enum LetterFlag { InWord, Random }
    public LetterType Type { get; set; } = LetterType.Normal;
    public LetterFlag Flag { get; set; } = LetterFlag.Random;

    public void SetLetter(char letter) {
        TileLetter = letter;
    }

}

public class LetterGridLetterTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, ICanvasRaycastFilter {

    public LetterData LetterData = new LetterData(-1,-1);

    private Image tileImage;
    private TMP_Text tileText;
    public float CurrentTriggerAreaPercentage { get; private set; } = 1f;

    public float SmallerTriggerAreaPercentage { get; private set; } = 0.7f;

    void Awake() {
        tileImage = GetComponent<Image>();
        tileText = GetComponentInChildren<TMP_Text>();
    }

    void Start() {
    }

    public void SetLetterText(char newLetter) {
        tileText.text = newLetter.ToString();
    }

    public Vector2Int GetTilePos() {
        return LetterData.TilePosition;
    }

    public void SelectTile() {
        LetterData.IsSelected = true;
    }

    public void Deselect() {
        LetterData.IsSelected = false;
    }

    public void SetCurrentColor(Color newColor) {
        tileImage.color = newColor;
    }

    public Color GetCurrentColor() {
        return tileImage.color;
    }

    public void ResetTriggerAreaPercentage() {
        CurrentTriggerAreaPercentage = 1f;
    }

    public void SetSmallerTriggerAreaPercentage() {
        CurrentTriggerAreaPercentage = SmallerTriggerAreaPercentage;
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

        float effectiveWidth = rect.width * CurrentTriggerAreaPercentage;
        float effectiveHeight = rect.height * CurrentTriggerAreaPercentage;

        float xMin = -effectiveWidth / 2f;
        float xMax = effectiveWidth / 2f;
        float yMin = -effectiveHeight / 2f;
        float yMax = effectiveHeight / 2f;

        return localPoint.x >= xMin && localPoint.x <= xMax &&
               localPoint.y >= yMin && localPoint.y <= yMax;
    }
}
