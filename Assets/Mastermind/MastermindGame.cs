using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Using TextMeshPro for text rendering

public class MastermindGame : MonoBehaviour {
    public Button[] colorButtons; // Buttons used for selecting colors
    private List<Color> availableColors = new List<Color>(); // Extract colors from buttons

    public Image[] secretCodeSlots; // 4 hidden slots for the secret code
    private Color[] secretCode; // The randomly generated secret code

    public GameObject[] guessRows; // 8-10 guess rows
    public GameObject[] hintRows; // 8-10 hint rows

    public Button submitButton; // Submit button
    public TMP_Text feedbackText; // TextMeshPro for displaying feedback

    private Color[] playerGuess = new Color[4]; // Stores the player's current guess
    private int currentRow = 0; // Tracks the active guess row
    private int guessIndex = 0; // Tracks which slot in the row is being filled

    private void Start() {
        ExtractColorsFromButtons(); // Get colors dynamically from buttons
        GenerateSecretCode(); // Generate a new secret code
        AssignColorButtons(); // Assign click events to buttons
        submitButton.onClick.AddListener(CheckGuess); // Assign the submit event

        HighlightCurrentRow(); // Highlight the first row at start
    }

    void ExtractColorsFromButtons() {
        availableColors.Clear(); // Reset the list to avoid duplicates

        foreach (Button btn in colorButtons) {
            if (btn != null && btn.image != null) {
                availableColors.Add(btn.image.color); // Store button colors
            } else {
                Debug.LogWarning("Warning: One or more color buttons are missing an Image component.");
            }
        }

        if (availableColors.Count == 0) {
            Debug.LogError("Error: No colors found in color buttons! Assign colors to the buttons.");
        }
    }

    void GenerateSecretCode() {
        if (availableColors.Count == 0) {
            Debug.LogError("Error: Cannot generate secret code. No colors available.");
            return;
        }

        secretCode = new Color[4];
        for (int i = 0; i < 4; i++) {
            secretCode[i] = availableColors[Random.Range(0, availableColors.Count)];
            secretCodeSlots[i].color = Color.black; // Keep the secret code hidden
        }
    }

    void AssignColorButtons() {
        for (int i = 0; i < colorButtons.Length; i++) {
            int index = i; // Capture index for lambda function
            colorButtons[i].onClick.AddListener(() => SelectColor(index));
        }
    }

    public void SelectColor(int colorIndex) {
        if (guessIndex < 4) // Ensure we don't exceed 4 slots per row
        {
            // Get only the Image components of the slots (ignoring the row background)
            Image[] currentGuessSlots = guessRows[currentRow].transform.GetComponentsInChildren<Image>();

            // Filter out the row background by ensuring we only select child objects
            List<Image> slotImages = new List<Image>();
            foreach (Image img in currentGuessSlots) {
                if (img.gameObject != guessRows[currentRow]) // Exclude the row itself
                {
                    slotImages.Add(img);
                }
            }

            // Ensure we only assign colors to the slots
            if (guessIndex < slotImages.Count) {
                playerGuess[guessIndex] = availableColors[colorIndex];
                slotImages[guessIndex].color = availableColors[colorIndex]; // Apply selected color
            }

            guessIndex++;
        }
    }



    void CheckGuess() {
        if (currentRow >= guessRows.Length) return; // Prevent out-of-bounds errors

        int correctPosition = 0, correctColor = 0;
        bool[] checkedSecret = new bool[4];
        bool[] checkedGuess = new bool[4];

        for (int i = 0; i < 4; i++) {
            if (playerGuess[i] == secretCode[i]) {
                correctPosition++;
                checkedSecret[i] = true;
                checkedGuess[i] = true;
            }
        }

        for (int i = 0; i < 4; i++) {
            if (!checkedGuess[i]) {
                for (int j = 0; j < 4; j++) {
                    if (!checkedSecret[j] && playerGuess[i] == secretCode[j]) {
                        correctColor++;
                        checkedSecret[j] = true;
                        break;
                    }
                }
            }
        }

        ShowHints(correctPosition, correctColor, currentRow);

        if (correctPosition == 4) {
            feedbackText.text = "You Win!";
        } else {
            currentRow++; // Move to the next row
            if (currentRow < guessRows.Length) {
                HighlightCurrentRow(); // Update the row highlight
            } else {
                feedbackText.text = " Game Over! Try Again.";
            }
        }

        guessIndex = 0; // Reset for next round
    }

    void ShowHints(int correct, int misplaced, int rowIndex) {
        Transform hintRow = hintRows[rowIndex].transform;
        for (int i = 0; i < correct; i++) {
            hintRow.GetChild(i).GetComponent<Image>().color = Color.green;
        }
        for (int i = 0; i < misplaced; i++) {
            hintRow.GetChild(correct + i).GetComponent<Image>().color = Color.yellow;
        }
    }

    void HighlightCurrentRow() {
        for (int i = 0; i < guessRows.Length; i++) {
            // Reset row background color to default (gray) without affecting slots
            guessRows[i].GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);
        }

        if (currentRow < guessRows.Length) {
            // Highlight only the row background without affecting slots
            guessRows[currentRow].GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

}
