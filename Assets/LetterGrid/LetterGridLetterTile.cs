using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LetterGridLetterTile : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler {
    private char letter;
    private bool isSelected = false;
    private Color currentColor;
    private Vector2Int tilePos;

    public enum TileType { Normal, DoubleLetter, TripleWord }
    public TileType tileType = TileType.Normal;

    void Start() {
        currentColor = GetComponent<Image>().color;
    }

    public void SetLetter(char newLetter) {
        letter = newLetter;
        GetComponentInChildren<TMP_Text>().text = letter.ToString();
    }

    // Handle tile clicks
    public void OnPointerDown(PointerEventData eventData) {
        print("Pointer down");
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
        GetComponent<Image>().color = Color.yellow;
    }

    public void SetBaseColor(Color newColor) {
        currentColor = newColor;
        GetComponent<Image>().color = newColor;
    }

    public void SetTilePos(int x, int y) {
        tilePos.x = x;
        tilePos.y = y;
    }

    public Vector2Int GetTilePos() {
        return tilePos;
    }

    public void Deselect() {
        isSelected = false;
        GetComponent<Image>().color = currentColor;
    }
}