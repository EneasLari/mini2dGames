using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridManager : MonoBehaviour {

    // Add to LetterGridManager
    [Header("Difficulty Settings")]
    public int minWordLength = 3;
    public int maxWordLength = 6;
    public int minWords = 3;
    public int maxWords = 5;

    public static LetterGridManager instance;
    public GameObject letterTilePrefab; // Prefab for letter tiles
    public Transform gridParent; // Parent panel for grid tiles
    public int gridSize = 4; // 4x4 grid
    
    private char[,] letterGrid;
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private List<LetterGridLetterTile> tilesList=new();
    private HashSet<Vector2Int> wordTilePositions = new HashSet<Vector2Int>();


    private void Awake() {
        instance = this;
    }
    private void Start() {
        gridParent.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridParent.GetComponent<GridLayoutGroup>().constraintCount = gridSize;
        GenerateGrid();
    }

    void GenerateGrid() {
        letterGrid = new char[gridSize, gridSize];
        tilesList.Clear();
        // Clear previous positions
        wordTilePositions.Clear();

        // Initialize grid with null characters
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                letterGrid[i, j] = '\0';
            }
        }

        // Select and place words
        List<string> wordsToPlace = GetWordsForGrid(3, 5);
        List<string> successfullyPlaced = new List<string>();

        foreach (string word in wordsToPlace) {
            if (TryPlaceWord(word.ToUpper())) {
                successfullyPlaced.Add(word);
            }
        }

        // Fill remaining spaces with random letters
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (letterGrid[i, j] == '\0') {
                    letterGrid[i, j] = GetRandomLetter();
                }
            }
        }

        // Instantiate tiles
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                GameObject tile = Instantiate(letterTilePrefab, gridParent);
                tile.GetComponentInChildren<TMP_Text>().text = letterGrid[i, j].ToString();
                LetterGridLetterTile letterTile = tile.GetComponent<LetterGridLetterTile>();
                letterTile.SetLetter(letterGrid[i, j]);
                letterTile.SetTilePos(i, j);
                letterTile.SetCurrentColor(LetterGridWordManager.instance.baseColor);
                tilesList.Add(letterTile);

                // DEBUG: Apply purple color to word tiles
                if (LetterGridWordManager.instance.highlightWordTiles && wordTilePositions.Contains(new Vector2Int(i, j))) {
                    // Update tile's base color to maintain purple when deselected
                    LetterGridLetterTile tileScript = tile.GetComponent<LetterGridLetterTile>();
                    tileScript.SetCurrentColor(LetterGridWordManager.instance.wordTileColor);
                }
            }
        }

        // Debug: Show placed words
        Debug.Log("Successfully placed words: " + string.Join(", ", successfullyPlaced));
    }

    public void ResetTilesTriggerArea() {
        foreach (var item in tilesList)
        {
            item.ResetTriggerAreaPercentage();
        }
    }

    public void SmallerTilesTriggerArea() {
        foreach (var item in tilesList) {
            item.SmallerTriggerAreaPercentage();
        }
    }


    private bool TryPlaceWord(string word) {
        // Directions: right, down, down-right, down-left
        Vector2Int[] directions = {
            new Vector2Int(1, 0),  // Horizontal →
            new Vector2Int(0, 1),  // Vertical ↓
            new Vector2Int(1, 1),  // Diagonal ↘
            new Vector2Int(1, -1)  // Diagonal ↙
        };

        // Try multiple positions and directions
        for (int attempt = 0; attempt < 100; attempt++) {
            Vector2Int startPos = new Vector2Int(
                Random.Range(0, gridSize),
                Random.Range(0, gridSize)
            );

            // Create shuffled list of directions
            List<Vector2Int> shuffledDirections = new List<Vector2Int>(directions);
            Shuffle(shuffledDirections);

            // Try each direction in random order
            foreach (Vector2Int dir in shuffledDirections) {
                // Check if word fits in chosen direction
                Vector2Int endPos = startPos + dir * (word.Length - 1);
                if (endPos.x < 0 || endPos.x >= gridSize || endPos.y < 0 || endPos.y >= gridSize)
                    continue;

                // Check for conflicts
                Vector2Int pos = startPos;
                bool canPlace = true;
                foreach (char c in word) {
                    // Only allow placement if cell is empty or has matching letter
                    if (letterGrid[pos.x, pos.y] != '\0' && letterGrid[pos.x, pos.y] != c) {
                        canPlace = false;
                        break;
                    }
                    pos += dir;
                }

                if (!canPlace) continue;

                // Place the word
                pos = startPos;
                foreach (char c in word) {
                    letterGrid[pos.x, pos.y] = c;
                    // Mark position as part of a word
                    if (LetterGridWordManager.instance.highlightWordTiles) {
                        wordTilePositions.Add(pos);
                    }
                    pos += dir;
                }
                return true;
            }
        }
        return false; // Failed to place after 100 attempts
    }

    // Fisher-Yates shuffle algorithm
    private void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    // Returns random words from dictionary (length 3 - gridSize)
    private List<string> GetWordsForGrid(int minWords, int maxWords) {
        List<string> words = new List<string>();
        int wordCount = Random.Range(minWords, maxWords + 1);

        // Filter suitable words (avoid very short/long words)
        List<string> candidates = new List<string>();
        foreach (string word in LetterGridWordManager.instance.validWords) {
            if (word.Length >= minWordLength && word.Length <= maxWordLength) {
                candidates.Add(word);
            }
        }

        // Select random words
        for (int i = 0; i < wordCount && candidates.Count > 0; i++) {
            int index = Random.Range(0, candidates.Count);
            words.Add(candidates[index]);
            candidates.RemoveAt(index); // Prevent duplicates
        }
        return words;
    }


    private char GetRandomLetter() {
        return alphabet[Random.Range(0, alphabet.Length)];
    }
}
