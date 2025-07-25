using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;
using System.Collections;
using static LetterGridLetterTile;
using UnityEngine.EventSystems;

public class LetterGridWordManager : MonoBehaviour {
    public static LetterGridWordManager instance;
    public Canvas GridCanvas;
    public TMP_Text selectedWordText;
    public TMP_Text scoreText;
    public Button resetButton;
    public Button undoButton;

    // Add at top of LetterGridManager.cs
    [Header("Debug Settings")]
    public bool highlightWordTiles = true; // Toggle in Unity Inspector
    public Color wordTileColor = new Color(0.5f, 0f, 0.8f, 1f); // Purple color


    [Header("Colors")]
    public Color baseColor = Color.white;
    public Color correctColor = Color.green;
    public Color selectedColor = Color.yellow;

    private int score = 0;
    public HashSet<string> validWords = new HashSet<string>();
    private string currentWord = "";
    private List<LetterGridLetterTile> currentSelectedLetterTiles = new List<LetterGridLetterTile>();
    private HashSet<string> submittedWords = new HashSet<string>();
    public bool isFlashing = false;
    public bool isSelecting = false;
    private Vector2Int selectionDirection;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        LoadDictionary();
        resetButton.onClick.AddListener(ResetSelectedTiles);
    }

    private void Update() {
        if (isFlashing) return;

        // Handle mouse/touch release to complete selection
        bool mouseReleased = Input.GetMouseButtonUp(0);
        bool touchReleased = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        if (isSelecting && (mouseReleased || touchReleased)) {
            if (currentWord.Length >= 3) {
                ValidateWord();
            } else {
                ResetSelectedTiles();
            }
            isSelecting = false;
        }
    }

    public void StartSelection(LetterGridLetterTile firstTile) {
        if (isFlashing) return;
        ResetSelectedTiles();
        isSelecting = true;
        AddLetter(firstTile);
    }


    public void AddLetter(LetterGridLetterTile letterTile) {
        // Prevent duplicate selection
        if (currentSelectedLetterTiles.Contains(letterTile)) return;

        currentWord += letterTile.GetComponentInChildren<TMP_Text>().text;
        currentSelectedLetterTiles.Add(letterTile);
        selectedWordText.text = "Word: " + currentWord;

        // Update tile visual state
        letterTile.SelectTile();
        letterTile.SetCurrentColor(selectedColor);

        if (currentSelectedLetterTiles.Count == 2) {
            Vector2Int firstPos = currentSelectedLetterTiles[0].GetTilePos();
            Vector2Int secondPos = currentSelectedLetterTiles[1].GetTilePos();
            selectionDirection = secondPos - firstPos;
            //Now that we have the direction we can set the trigger area of every tile full
            LetterGridManager.instance.ResetTilesTriggerArea();
        }

    }


    public void ValidateWord() {
        if (isFlashing) return;

        bool isValid = validWords.Contains(currentWord) && !submittedWords.Contains(currentWord);
        Color flashColor = isValid ? Color.green : Color.red;

        if (isValid) {
            int wordScore = currentWord.Length * 10;
            foreach (var tile in currentSelectedLetterTiles) {
                tile.IsPartOfWord = true;
                if (tile.tileType == TileType.DoubleLetter) wordScore += 5;
                else if (tile.tileType == TileType.TripleWord) wordScore *= 3;
            }
            score += wordScore;
            scoreText.text = "Score: " + score;
            submittedWords.Add(currentWord);
        }

        StartCoroutine(FlashTilesAndReset(flashColor,isValid));
    }

    IEnumerator FlashTilesAndReset(Color flashColor,bool isValid) {
        isFlashing = true;
        List<LetterGridLetterTile> tilesToFlash = new List<LetterGridLetterTile>(currentSelectedLetterTiles);

        foreach (var tile in tilesToFlash) {
            Image img = tile.GetComponent<Image>();
            Color original = img.color;
            img.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            img.color = original;
        }

        ResetSelection();
        foreach (var tile in currentSelectedLetterTiles) {
            tile.Deselect();
            tile.SetCurrentColor((isValid || tile.IsPartOfWord) ? correctColor : baseColor);
        }
        currentSelectedLetterTiles.Clear();
        isFlashing = false;
    }

    public void ResetSelection() {
        currentWord = "";
        selectedWordText.text = "Word: ";
    }

    public void ResetSelectedTiles() {
        if (isFlashing) return;

        ResetSelection();
        foreach (var item in currentSelectedLetterTiles) {
            item.Deselect();
            item.SetCurrentColor(baseColor);
        }
        currentSelectedLetterTiles.Clear();
        isSelecting = false;
        ResetDirection();
    }

    void LoadDictionary() {
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

    public bool CanSelectTile(LetterGridLetterTile tile) {
        if (currentSelectedLetterTiles.Count == 0) return true;

        LetterGridLetterTile lastTile = currentSelectedLetterTiles[^1];
        Vector2Int lastPos = lastTile.GetTilePos();
        Vector2Int newPos = tile.GetTilePos();

        // For second tile: allow any adjacent tile
        if (currentSelectedLetterTiles.Count == 1) {
            int dx = Mathf.Abs(newPos.x - lastPos.x);
            int dy = Mathf.Abs(newPos.y - lastPos.y);
            return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
        }

        // For subsequent tiles: must follow initial direction
        Vector2Int requiredPos = lastPos + selectionDirection;
        return newPos == requiredPos;
    }

    private void ResetDirection() {
        selectionDirection = Vector2Int.zero;
        //now that we reset the direction we want smaller trigger area
        LetterGridManager.instance.SmallerTilesTriggerArea();
    }
}