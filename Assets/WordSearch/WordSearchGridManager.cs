using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WordSearchGridManager : MonoBehaviour {
    public GameObject letterButtonPrefab; // Letter UI prefab
    public Transform gridParent; // Parent for grid UI
    public int gridSize = 10; // 10x10 Grid
    [HideInInspector]
    public List<string> wordsToPlace = new List<string>
    {
        "UNITY", "GAME", "CODE", "SCRIPT", "DEBUG", "PLAYER", "LEVEL",
        "REWARD", "OBJECT", "SYSTEM", "ENGINE", "VECTOR", "PHYSICS",
        "BUTTON", "INPUT", "ACTION", "LAYER", "METHOD", "STRUCTURE"
    };


    private char[,] letterGrid; // 2D letter grid
    private List<Vector2Int> usedPositions = new List<Vector2Int>();

    void Start() {
        letterGrid = new char[gridSize, gridSize]; // Initialize grid
        PlaceWords();      // Place words first
        FillEmptySpaces(); // Fill empty spaces with random letters
        GenerateGridUI();  // Generate UI after placing words
    }

    /// <summary>
    /// Places words randomly in the grid.
    /// </summary>
    void PlaceWords() {
        List<Vector2Int> directions = new List<Vector2Int>
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

        foreach (string word in wordsToPlace) {
            bool placed = false;
            int attempts = 0, maxAttempts = 100;

            while (!placed && attempts < maxAttempts) {
                attempts++;
                int startX = Random.Range(0, gridSize);
                int startY = Random.Range(0, gridSize);
                Vector2Int direction = directions[Random.Range(0, directions.Count)];

                if (CanPlaceWord(word, startX, startY, direction.x, direction.y)) {
                    for (int i = 0; i < word.Length; i++) {
                        int newX = startX + i * direction.x;
                        int newY = startY + i * direction.y;

                        letterGrid[newX, newY] = word[i]; // ✅ Assign word letter
                        usedPositions.Add(new Vector2Int(newX, newY));
                    }
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
        if (word.Length > gridSize) return false; // Prevent long words from being placed

        for (int i = 0; i < word.Length; i++) {
            int newX = startX + i * directionX;
            int newY = startY + i * directionY;

            if (newX < 0 || newX >= gridSize || newY < 0 || newY >= gridSize)
                return false; // Prevent out-of-bounds placement

            if (usedPositions.Contains(new Vector2Int(newX, newY)) && letterGrid[newX, newY] != word[i])
                return false; // Prevent incorrect overlap
        }
        return true;
    }


    /// <summary>
    /// Fills remaining grid spaces with random letters.
    /// </summary>
    void FillEmptySpaces() {
        for (int row = 0; row < gridSize; row++) {
            for (int col = 0; col < gridSize; col++) {
                Vector2Int position = new Vector2Int(row, col);
                if (!usedPositions.Contains(position)) {
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
}
