using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using static LetterGridLetterTile;
using UnityEngine.EventSystems;

public class LetterGridWordManager : MonoBehaviour {
    public static LetterGridWordManager instance;

    [Header("🔷 UI References")]
    public Canvas gridCanvas;
    public TMP_Text wordDisplayText;
    public TMP_Text scoreDisplayText;
    public Button resetButton;
    public Button undoButton;

    [Header("🛠 Gameplay Settings")]
    public bool highlightWordTiles = true;
    [Tooltip("Color for word tiles in debug mode")]
    public Color wordTileColor = new Color(0.5f, 0f, 0.8f, 1f);

    [Header("🎨 Tile Colors")]
    public Color baseColor = Color.white;
    public Color correctColor = Color.green;
    public Color selectedColor = Color.yellow;

    [Header("📈 Scoring & Word State")]
    public HashSet<string> validWords = new HashSet<string>();
    private HashSet<string> foundWords = new HashSet<string>();
    private List<LetterGridLetterTile> selectedTiles = new List<LetterGridLetterTile>();

    [Header("🔒 Runtime State")]
    private int score = 0;
    private string activeWord = "";
    private bool isShowingFeedback = false;
    private bool isUserSelecting = false;
    private Vector2Int wordDirection;

    public bool IsShowingFeedback => isShowingFeedback;
    public bool IsUserSelecting => isUserSelecting;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        LoadDictionary();
        resetButton.onClick.AddListener(ClearTileSelection);
        // undoButton.onClick.AddListener(UndoLastTile);
    }

    private void Update() {
        if (isShowingFeedback) return;

        bool mouseReleased = Input.GetMouseButtonUp(0);
        bool touchReleased = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        if (isUserSelecting && (mouseReleased || touchReleased)) {
            if (activeWord.Length >= 3) ValidateSelectedWord();
            else ClearTileSelection();
            isUserSelecting = false;
        }

        if (isUserSelecting && Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                RaycastTouchToTile(touch.position);
            }
        }

        if (isUserSelecting && Input.GetMouseButton(0)) {
            RaycastTouchToTile(Input.mousePosition);
        }
    }

    private void RaycastTouchToTile(Vector2 screenPosition) {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results) {
            LetterGridLetterTile tile = result.gameObject.GetComponent<LetterGridLetterTile>();
            if (tile != null && !tile.isSelected) {
                TrySelectHoveredTile(tile);
                break;
            }
        }
    }

    public void StartSelection(LetterGridLetterTile firstTile) {
        if (isShowingFeedback) return;
        ClearTileSelection();
        isUserSelecting = true;
        AddTileToWord(firstTile);
    }

    public void AddTileToWord(LetterGridLetterTile tile) {
        if (selectedTiles.Contains(tile)) return;

        activeWord += tile.GetComponentInChildren<TMP_Text>().text;
        selectedTiles.Add(tile);
        wordDisplayText.text = "Word: " + activeWord;

        tile.SelectTile();
        tile.SetCurrentColor(selectedColor);

        if (selectedTiles.Count == 2) {
            Vector2Int firstPos = selectedTiles[0].GetTilePos();
            Vector2Int secondPos = selectedTiles[1].GetTilePos();
            wordDirection = secondPos - firstPos;
            LetterGridManager.instance.ResetTilesTriggerArea();
        }
    }

    public void TrySelectHoveredTile(LetterGridLetterTile tile) {
        if ((Input.GetMouseButton(0) || Input.touchCount > 0) && isUserSelecting && !isShowingFeedback) {
            if (selectedTiles.Count > 1 && tile == selectedTiles[^2]) {
                UndoLastTile();
                return;
            }

            if (!tile.isSelected && IsTileSelectable(tile)) {
                AddTileToWord(tile);
            }
        }
    }

    public bool IsTileSelectable(LetterGridLetterTile tile) {
        if (selectedTiles.Count == 0) return true;

        Vector2Int newPos = tile.GetTilePos();
        Vector2Int lastPos = selectedTiles[^1].GetTilePos();

        if (selectedTiles.Count > 1) {
            var secondLastTile = selectedTiles[^2];
            if (secondLastTile == tile) return true;
        }

        if (selectedTiles.Count == 1) {
            int dx = Mathf.Abs(newPos.x - lastPos.x);
            int dy = Mathf.Abs(newPos.y - lastPos.y);
            return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
        }

        return newPos == lastPos + wordDirection;
    }

    private void UndoLastTile() {
        if (selectedTiles.Count < 2) return;

        LetterGridLetterTile lastTile = selectedTiles[^1];
        selectedTiles.RemoveAt(selectedTiles.Count - 1);
        activeWord = activeWord.Substring(0, activeWord.Length - 1);
        wordDisplayText.text = "Word: " + activeWord;

        lastTile.Deselect();
        lastTile.SetCurrentColor(lastTile.IsPartOfWord ? correctColor : baseColor);

        if (selectedTiles.Count == 1) {
            ResetDirection();
        }
    }

    private void ResetDirection() {
        wordDirection = Vector2Int.zero;
        LetterGridManager.instance.SmallerTilesTriggerArea();
    }

    public void ValidateSelectedWord() {
        if (isShowingFeedback) return;

        bool isValid = validWords.Contains(activeWord) && !foundWords.Contains(activeWord);
        Color flashColor = isValid ? Color.green : Color.red;

        if (isValid) {
            int wordScore = activeWord.Length * 10;
            foreach (var tile in selectedTiles) {
                tile.IsPartOfWord = true;
                if (tile.tileType == TileType.DoubleLetter) wordScore += 5;
                else if (tile.tileType == TileType.TripleWord) wordScore *= 3;
            }
            score += wordScore;
            scoreDisplayText.text = "Score: " + score;
            foundWords.Add(activeWord);
        }

        StartCoroutine(FlashTilesAndReset(flashColor, isValid));
    }

    IEnumerator FlashTilesAndReset(Color flashColor, bool isValid) {
        isShowingFeedback = true;
        List<LetterGridLetterTile> tilesToFlash = new List<LetterGridLetterTile>(selectedTiles);

        foreach (var tile in tilesToFlash) {
            Image img = tile.GetComponent<Image>();
            Color original = img.color;
            img.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            img.color = original;
        }

        ClearActiveWord();
        foreach (var tile in selectedTiles) {
            tile.Deselect();
            tile.SetCurrentColor((isValid || tile.IsPartOfWord) ? correctColor : baseColor);
        }
        selectedTiles.Clear();
        isShowingFeedback = false;
    }

    public void ClearTileSelection() {
        if (isShowingFeedback) return;

        ClearActiveWord();
        foreach (var tile in selectedTiles) {
            tile.Deselect();
            tile.SetCurrentColor(baseColor);
        }
        selectedTiles.Clear();
        isUserSelecting = false;
        ResetDirection();
    }

    public void ClearActiveWord() {
        activeWord = "";
        wordDisplayText.text = "Word: ";
    }

    private void LoadDictionary() {
        TextAsset wordFile = Resources.Load<TextAsset>("wordlist");
        if (wordFile == null) {
            Debug.LogError("wordlist.txt not found in Resources folder!");
            return;
        }

        string[] words = wordFile.text.Split('\n');
        HashSet<string> filteredWords = new HashSet<string>();

        foreach (string word in words) {
            string cleanWord = word.Trim().ToUpper();
            if (cleanWord.Length >= 3 && cleanWord.Length <= LetterGridManager.instance.gridSize) {
                filteredWords.Add(cleanWord);
            }
        }

        validWords = filteredWords;
    }
}
