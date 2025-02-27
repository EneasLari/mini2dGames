using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CrosswordManager : MonoBehaviour {
    public TMP_Text clueText;
    public GameObject gridContainer;
    public Button checkButton;
    public Button resetButton;
    public Button solveButton;

    public int rows = 5;
    public int cols = 5;
    private char[,] crosswordGrid;

    // Dictionary to store words with their clues and positions
    private Dictionary<string, (string word, int row, int col, bool isAcross, string clue)> words = new Dictionary<string, (string, int, int, bool, string)>
    {
    // Across Words
    { "1A", ("CAT", 0, 0, true, "A small pet animal") },
    { "3A", ("RIVER", 2, 0, true, "A flowing body of water") },
    { "5A", ("HOUSE", 4, 2, true, "A place where people live") },
    { "7A", ("LIGHT", 6, 3, true, "Opposite of dark") },

    // Down Words
    { "2D", ("CAR", 0, 0, false, "A vehicle") },
    { "4D", ("BIRD", 0, 4, false, "An animal that flies") },
    { "6D", ("BED", 1, 3, false, "A place to sleep") },  // NEW WORD replacing "PLANE"
    { "8D", ("BRICK", 1, 7, false, "Used to build houses") }   // NEW WORD replacing "TOWER"
};




    void Start() {
        crosswordGrid = new char[rows, cols]; // Initialize grid with empty spaces
        InitializeGrid();
        gridContainer.GetComponent<CrosswordGrid>().GenerateGrid(rows,cols);
        HideEmptyCells();
        DisplayClues();
        checkButton.onClick.AddListener(CheckAnswers);
        resetButton.onClick.AddListener(ResetGrid);
        solveButton.onClick.AddListener(SolvePuzzle);
    }

    void InitializeGrid() {
        // Fill grid with empty spaces
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                crosswordGrid[i, j] = '-';  // Empty space placeholder
            }
        }

        // Temporary list to track words that need to be removed
        List<string> wordsToRemove = new List<string>();

        // Add words to the grid
        foreach (var entry in words) {
            string key = entry.Key;
            string word = entry.Value.word;
            int row = entry.Value.row;
            int col = entry.Value.col;
            bool isAcross = entry.Value.isAcross;

            bool hasConflict = false;

            for (int i = 0; i < word.Length; i++) {
                int currentRow = isAcross ? row : row + i;
                int currentCol = isAcross ? col + i : col;

                char currentLetter = crosswordGrid[currentRow, currentCol];

                // If there's a conflict, mark the word for removal
                if (currentLetter != '-' && currentLetter != word[i]) {
                    hasConflict = true;
                    break; // Stop checking further letters
                }
            }

            // If the word has a conflict, schedule it for removal
            if (hasConflict) {
                wordsToRemove.Add(key);
                Debug.LogWarning($"Removed '{word}' due to conflict.");
            } else {
                // Otherwise, place the word
                for (int i = 0; i < word.Length; i++) {
                    int currentRow = isAcross ? row : row + i;
                    int currentCol = isAcross ? col + i : col;
                    crosswordGrid[currentRow, currentCol] = word[i];
                }
            }
        }

        // Remove conflicting words from the dictionary
        foreach (string key in wordsToRemove) {
            words.Remove(key);
        }
    }



    void DisplayClues() {
        string formattedClues = "";
        foreach (var entry in words) {
            formattedClues += $"{entry.Key}: {entry.Value.clue}\n";
        }
        clueText.text = formattedClues;
    }

    public void CheckAnswers() {
        bool allCorrect = true;

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                string cellName = $"Cell_{i}_{j}";
                GameObject cell = GameObject.Find(cellName);

                if (cell != null) {
                    TMP_InputField input = cell.GetComponent<TMP_InputField>();
                    char correctLetter = crosswordGrid[i, j];

                    if (correctLetter != '-' && input.text.ToUpper() != correctLetter.ToString()) {
                        allCorrect = false;
                        input.image.color = Color.red;  // Highlight incorrect answers
                    } else {
                        input.image.color = Color.green;
                    }
                }
            }
        }

        if (allCorrect) {
            Debug.Log("Correct! You solved the crossword!");
        }
    }

    public void HideEmptyCells() {
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                string cellName = $"Cell_{i}_{j}";
                GameObject cell = GameObject.Find(cellName);

                if (cell != null) {
                    TMP_InputField input = cell.GetComponent<TMP_InputField>();
                    char correctLetter = crosswordGrid[i, j];
                    if (correctLetter == '-') {
                        input.image.enabled =false;  // Highlight incorrect answers
                    } 
                }
            }
        }
    }
    public void SolvePuzzle() {
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                string cellName = $"Cell_{i}_{j}";
                GameObject cell = GameObject.Find(cellName);

                if (cell != null) {
                    TMP_InputField input = cell.GetComponent<TMP_InputField>();
                    char correctLetter = crosswordGrid[i, j];

                    if (correctLetter != '-') {
                        input.text = correctLetter.ToString();  // Fill in correct answer
                        input.image.color = Color.green;  // Highlight correct letters
                    }
                }
            }
        }
    }

    public void ResetGrid() {
        foreach (Transform child in gridContainer.transform) {
            TMP_InputField input = child.GetComponent<TMP_InputField>();
            if (input != null) {
                input.text = "";
                input.image.color = Color.white;
            }
        }
    }
}
