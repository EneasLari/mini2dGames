using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SudokuManager : MonoBehaviour {
    public TMP_InputField[] cells; // Assign 81 TMP Input Fields in the Unity Inspector
    public Button generateButton, checkButton, solveButton, resetButton;
    public TMP_Text statusText;

    private int[,] solution = new int[9, 9];
    private int[,] puzzle = new int[9, 9];

    void Start() {
        generateButton.onClick.AddListener(GenerateSudoku);
        checkButton.onClick.AddListener(CheckSolution);
        solveButton.onClick.AddListener(SolveSudoku);
        resetButton.onClick.AddListener(ResetBoard);
        foreach (TMP_InputField cell in cells) {
            cell.onValueChanged.AddListener(delegate { ValidateInput(cell); });
        }
        GenerateSudoku();
    }


    void GenerateSudoku() {
        SudokuGenerator.Generate(out puzzle, out solution);
        FillGridWithPuzzle();
        statusText.text = "Solve the Sudoku!";
    }

    void FillGridWithPuzzle() {
        for (int i = 0; i < 9; i++) {
            for (int j = 0; j < 9; j++) {
                int index = i * 9 + j;
                if (puzzle[i, j] != 0) {
                    cells[index].text = puzzle[i, j].ToString();
                    cells[index].interactable = false; // Disable pre-filled cells
                } else {
                    cells[index].text = "";
                    cells[index].interactable = true;
                }
            }
        }
    }

    void CheckSolution() {
        bool isCorrect = true;
        Color[] originalColors = new Color[cells.Length]; // Store initial colors

        for (int i = 0; i < 9; i++) {
            for (int j = 0; j < 9; j++) {
                int index = i * 9 + j;
                originalColors[index] = cells[index].textComponent.color; // Store original colors

                if (cells[index].text != solution[i, j].ToString()) {
                    isCorrect = false;
                    cells[index].textComponent.color = Color.red; // Highlight wrong answers
                } else {
                    cells[index].textComponent.color = Color.green; // Highlight correct answers
                }
            }
        }

        statusText.text = isCorrect ? "Correct Solution! 🎉" : "Some numbers are incorrect!";

        // Reset colors after 1.5 seconds using a coroutine
        StartCoroutine(ResetCellColors(originalColors, 1.5f));
    }


    IEnumerator ResetCellColors(Color[] originalColors, float delay) {
        yield return new WaitForSeconds(delay); // Wait before resetting colors

        for (int i = 0; i < cells.Length; i++) {
            cells[i].textComponent.color = originalColors[i]; // Restore original colors
        }
    }


    void SolveSudoku() {
        for (int i = 0; i < 9; i++) {
            for (int j = 0; j < 9; j++) {
                int index = i * 9 + j;
                cells[index].text = solution[i, j].ToString();
                cells[index].textComponent.color = Color.blue;
                cells[index].interactable = false; // Disable input after solving
            }
        }
        statusText.text = "Sudoku Solved!";
    }

    void ResetBoard() {
        FillGridWithPuzzle();
        statusText.text = "Try again!";
    }

    void ValidateInput(TMP_InputField inputField) {
        if (inputField.text.Length > 0) {
            char lastChar = inputField.text[inputField.text.Length - 1];

            // Check if the last entered character is a number between 1-9
            if (!char.IsDigit(lastChar) || lastChar == '0') {
                inputField.text = ""; // Clear invalid input
            } else {
                inputField.text = lastChar.ToString(); // Keep only the last valid number
            }
        }
    }
}
