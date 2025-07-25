using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridManager : MonoBehaviour {

    [Header("⚙️ Difficulty Settings")]
    public int minWordLength = 3;
    public int maxWordLength = 6;
    public int minWordsToPlace = 3;
    public int maxWordsToPlace = 5;

    public static LetterGridManager instance;

    [Header("📦 Grid Setup")]
    public GameObject letterTilePrefab;
    public Transform gridParent;
    public int gridSize = 4;

    private char[,] letterGrid;
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly List<LetterGridLetterTile> gridTiles = new();
    private readonly HashSet<Vector2Int> wordTilePositions = new();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        gridParent.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridParent.GetComponent<GridLayoutGroup>().constraintCount = gridSize;
        GenerateGrid();
    }

    public void GenerateGrid() {
        letterGrid = new char[gridSize, gridSize];
        gridTiles.Clear();
        wordTilePositions.Clear();

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                letterGrid[i, j] = '\0';
            }
        }

        List<string> wordsToPlace = GetWordsForGrid(minWordsToPlace, maxWordsToPlace);
        List<string> successfullyPlaced = new();

        foreach (string word in wordsToPlace) {
            if (TryPlaceWord(word.ToUpper())) {
                successfullyPlaced.Add(word);
            }
        }

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (letterGrid[i, j] == '\0') {
                    letterGrid[i, j] = GetRandomLetter();
                }
            }
        }

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                GameObject tile = Instantiate(letterTilePrefab, gridParent);
                tile.GetComponentInChildren<TMP_Text>().text = letterGrid[i, j].ToString();
                LetterGridLetterTile letterTile = tile.GetComponent<LetterGridLetterTile>();
                letterTile.SetLetter(letterGrid[i, j]);
                letterTile.SetTilePos(i, j);
                letterTile.SetCurrentColor(LetterGridWordManager.instance.baseColor);
                gridTiles.Add(letterTile);

                if (LetterGridWordManager.instance.highlightWordTiles && wordTilePositions.Contains(new Vector2Int(i, j))) {
                    letterTile.SetCurrentColor(LetterGridWordManager.instance.wordTileColor);
                }
            }
        }

        Debug.Log("Successfully placed words: " + string.Join(", ", successfullyPlaced));
    }

    public void ResetTilesTriggerArea() {
        foreach (var tile in gridTiles) {
            tile.ResetTriggerAreaPercentage();
        }
    }

    public void SmallerTilesTriggerArea() {
        foreach (var tile in gridTiles) {
            tile.SmallerTriggerAreaPercentage();
        }
    }

    private bool TryPlaceWord(string word) {
        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1)
        };

        for (int attempt = 0; attempt < 100; attempt++) {
            Vector2Int startPos = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            List<Vector2Int> shuffledDirections = new(directions);
            Shuffle(shuffledDirections);

            foreach (Vector2Int dir in shuffledDirections) {
                Vector2Int endPos = startPos + dir * (word.Length - 1);
                if (endPos.x < 0 || endPos.x >= gridSize || endPos.y < 0 || endPos.y >= gridSize)
                    continue;

                Vector2Int pos = startPos;
                bool canPlace = true;

                foreach (char c in word) {
                    if (letterGrid[pos.x, pos.y] != '\0' && letterGrid[pos.x, pos.y] != c) {
                        canPlace = false;
                        break;
                    }
                    pos += dir;
                }

                if (!canPlace) continue;

                pos = startPos;
                foreach (char c in word) {
                    letterGrid[pos.x, pos.y] = c;
                    if (LetterGridWordManager.instance.highlightWordTiles) {
                        wordTilePositions.Add(pos);
                    }
                    pos += dir;
                }
                return true;
            }
        }
        return false;
    }

    private void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    private List<string> GetWordsForGrid(int minWords, int maxWords) {
        List<string> words = new();
        int wordCount = Random.Range(minWords, maxWords + 1);

        List<string> candidates = new();
        foreach (string word in LetterGridWordManager.instance.validWords) {
            if (word.Length >= minWordLength && word.Length <= maxWordLength) {
                candidates.Add(word);
            }
        }

        for (int i = 0; i < wordCount && candidates.Count > 0; i++) {
            int index = Random.Range(0, candidates.Count);
            words.Add(candidates[index]);
            candidates.RemoveAt(index);
        }
        return words;
    }

    private char GetRandomLetter() {
        return alphabet[Random.Range(0, alphabet.Length)];
    }
}
