using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WordSearchGridManager : MonoBehaviour {
    public GameObject letterButtonPrefab; // Letter UI prefab
    public Transform gridParent; // Parent for grid UI
    public int gridSize = 10; // 10x10 Grid
    public List<string> wordsToPlace = new List<string>
    {
        "UNITY", "GAME", "CODE", "SCRIPT", "DEBUG", "PLAYER", "LEVEL",
        "REWARD", "OBJECT", "SYSTEM", "ENGINE", "VECTOR", "PHYSICS",
        "BUTTON", "INPUT", "ACTION", "LAYER", "METHOD", "STRUCTURE"
    };
    private List<string> wPlaced = new List<string>();
    private List<Vector2Int> directions = new List<Vector2Int>
{
        new Vector2Int(1, 0),  // Right
        new Vector2Int(-1, 0), // Left
        new Vector2Int(0, 1),  // Down
        new Vector2Int(0, -1), // Up
        new Vector2Int(1, 1),  // Diagonal Down-Right
        new Vector2Int(-1, -1),// Diagonal Up-Left
        new Vector2Int(-1, 1), // Diagonal Down-Left
        new Vector2Int(1, -1)  // Diagonal Up-Right
    };
    private char[,] letterGrid; // 2D letter grid

    void Start() {
        letterGrid = new char[gridSize, gridSize]; // Initialize grid
        InitializeGrid();
        PlaceWords();      // Place words first
        FillEmptySpaces(); // Fill empty spaces with random letters
        GenerateGridUI();  // Generate UI after placing words
    }

    void InitializeGrid() {
        letterGrid = new char[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                letterGrid[x, y] = '-'; // Fill with default placeholder
    }

    /// <summary>
    /// Places words randomly in the grid.
    /// </summary>
    void PlaceWords() {
        wordsToPlace.Sort((a, b) => b.Length.CompareTo(a.Length)); // Sort longest words first

        foreach (string word in wordsToPlace) {
            bool placed = false;
            int attempts = 0, maxAttempts = 500; // Increased attempts for better placement

            while (!placed && attempts < maxAttempts) {
                attempts++;
                int startX = UnityEngine.Random.Range(0, gridSize);
                int startY = UnityEngine.Random.Range(0, gridSize);
                Vector2Int direction = directions[UnityEngine.Random.Range(0, directions.Count)];

                if (CanPlaceWord(word, startX, startY, direction.x, direction.y)) {
                    for (int i = 0; i < word.Length; i++) {
                        int newX = startX + i * direction.x;
                        int newY = startY + i * direction.y;
                        letterGrid[newX, newY] = word[i]; // Assign word letter
                    }
                    Debug.Log("Word Placed: " + word);
                    wPlaced.Add(word);
                    placed = true;
                }
            }

            if (!placed) {
                Debug.LogWarning($"Could not place word: {word}");
            }
        }
    }

    /// <summary>
    /// Checks if a word can be placed at a given position.
    /// </summary>
    bool CanPlaceWord(string word, int startX, int startY, int directionX, int directionY) {
        if (word.Length > gridSize) return false;

        for (int i = 0; i < word.Length; i++) {
            int newX = startX + i * directionX;
            int newY = startY + i * directionY;

            if (newX < 0 || newX >= gridSize || newY < 0 || newY >= gridSize)
                return false; // Out-of-bounds check

            if (letterGrid[newX, newY] != '-' && letterGrid[newX, newY] != word[i])
                return false; // Allow matching overlaps, prevent conflicts
        }
        return true;
    }


    /// <summary>
    /// Fills remaining grid spaces with random letters.
    /// </summary>
    void FillEmptySpaces() {
        for (int row = 0; row < gridSize; row++) {
            for (int col = 0; col < gridSize; col++) {
                if (letterGrid[row, col] == '-') {
                    letterGrid[row, col] = (char)('A' + Random.Range(0, 26)); // ✅ Random letter
                }
            }
        }
    }

    /// <summary>
    /// Creates UI elements for the letter grid.
    /// </summary>
    void GenerateGridUI() {
        for (int row = 0; row < gridSize; row++) {
            for (int col = 0; col < gridSize; col++) {
                GameObject letterButton = Instantiate(letterButtonPrefab, gridParent);
                letterButton.GetComponentInChildren<TMP_Text>().text = letterGrid[row, col].ToString();
                letterButton.name = $"Letter_{row}_{col}";
            }
        }
    }

    public List<string> GetPlacedWords() {
        return wPlaced;
    }
}
