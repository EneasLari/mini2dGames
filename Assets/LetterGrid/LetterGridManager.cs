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


    [Header("📦 Grid Setup")]
    public GameObject letterTilePrefab;
    public Transform gridParent;
    public int gridSize = 4;

    [HideInInspector]
    public List<string> placedWords = new();

    private char[,] letterGrid;
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly List<LetterGridLetterTile> gridTiles = new();
    private readonly HashSet<Vector2Int> wordTilePositions = new();

    private void Awake() {
    }

    private void Start() {
        

    }

    public void StartGridManager() {
        UpdateGridLayout();
        GenerateGrid();
    }

    private void OnRectTransformDimensionsChange() {
        UpdateGridLayout();
    }


    public void UpdateGridLayout() {
        GridLayoutGroup gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        RectTransform panelRect = gridParent.parent.GetComponent<RectTransform>();

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize;

        float width = panelRect.rect.width;
        float height = panelRect.rect.height;

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        float totalSpacingX = spacingX * (gridSize - 1);
        float totalSpacingY = spacingY * (gridSize - 1);

        float availableWidth = width - gridLayout.padding.left - gridLayout.padding.right - totalSpacingX;
        float availableHeight = height - gridLayout.padding.top - gridLayout.padding.bottom - totalSpacingY;

        float cellSize = Mathf.Min(availableWidth, availableHeight) / gridSize;

        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }

    private void GenerateGrid() {
        // 💥 Clear previous grid tiles
        foreach (Transform child in gridParent) {
            Destroy(child.gameObject);
        }

        letterGrid = new char[gridSize, gridSize];
        gridTiles.Clear();
        wordTilePositions.Clear();

        // Initialize grid with empty cells
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

        placedWords = new List<string>(successfullyPlaced);

        // Fill remaining empty spots with random letters
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (letterGrid[i, j] == '\0') {
                    letterGrid[i, j] = GetRandomLetter();
                }
            }
        }

        // Create UI tiles
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                GameObject tile = Instantiate(letterTilePrefab, gridParent);
                CanvasGroup group = tile.AddComponent<CanvasGroup>();
                group.alpha = 0f;
                tile.GetComponentInChildren<TMP_Text>().text = letterGrid[i, j].ToString();

                LetterGridLetterTile letterTile = tile.GetComponent<LetterGridLetterTile>();
                letterTile.SetLetter(letterGrid[i, j]);
                letterTile.SetTilePos(i, j);
                letterTile.SetCurrentColor(LetterGridGameManager.Instance.wordManager.baseColor);
                gridTiles.Add(letterTile);

                if (LetterGridGameManager.Instance.wordManager.highlightWordTiles && wordTilePositions.Contains(new Vector2Int(i, j))) {
                    letterTile.SetCurrentColor(LetterGridGameManager.Instance.wordManager.wordTileColor);
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
                    if (LetterGridGameManager.Instance.wordManager.highlightWordTiles) {
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
        foreach (string word in LetterGridGameManager.Instance.wordManager.validWords) {
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
    public LetterGridLetterTile GetTileAt(int x, int y) {
        return gridTiles.Find(tile => tile.GetTilePos() == new Vector2Int(x, y));
    }
    private char GetRandomLetter() {
        return alphabet[Random.Range(0, alphabet.Length)];
    }
}
