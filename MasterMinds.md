# **📌 Mastermind Game - Documentation**

### **📖 Overview**

This is a **Mastermind** game built in **Unity (C#)** where the player **guesses a secret 4-color code** within a limited number of attempts. The game provides **hints** about correct colors in the right or wrong positions.

---

# **🎨 UI Structure**

The game's UI is structured using **Unity's Canvas system** with **Grid Layout Groups and Vertical Layout Groups** to ensure a clean and responsive layout.

### **📌 UI Elements**

| **Element**                       | **Purpose**                                                                                      |
| --------------------------------- | ------------------------------------------------------------------------------------------------ |
| **Secret Code Panel**             | Displays the **4 hidden slots** for the secret code (initially black).                           |
| **Guess Panel**                   | Contains **8-10 rows**, each with **4 empty slots** where the player makes their guesses.        |
| **Hint Panel**                    | Provides feedback (Green = correct color & position, Yellow = correct color but wrong position). |
| **Color Selection Panel**         | Contains **8-10 color buttons** for the player to choose from.                                   |
| **Submit Button**                 | Confirms the guess and checks correctness.                                                       |
| **Feedback Text (`TextMeshPro`)** | Displays win/loss messages and other instructions.                                               |

---

## **📌 UI Hierarchy in Unity**

```
Canvas
│── SecretCodePanel (Hidden Code)  → [ 🔲 🔲 🔲 🔲 ]
│
│── GuessPanel (Vertical Layout)
│   │── GuessRow1 (Horizontal Layout) → [ ⬜ ⬜ ⬜ ⬜ ]
│   │── GuessRow2 (Horizontal Layout) → [ ⬜ ⬜ ⬜ ⬜ ]
│   │── GuessRow3 (Horizontal Layout) → [ ⬜ ⬜ ⬜ ⬜ ]
│
│── HintPanel (Vertical Layout)
│   │── HintRow1 → [ ⬤ ⬤ ⬤ ⬤ ]
│   │── HintRow2 → [ ⬤ ⬤ ⬤ ⬤ ]
│
│── ColorSelectionPanel (Horizontal Layout)
│   │── ColorButton1 🎨
│   │── ColorButton2 🎨
│   │── ColorButton3 🎨
│   │── ColorButton4 🎨
│   │── ColorButton5 🎨
│   │── ColorButton6 🎨
│   │── ColorButton7 🎨
│
│── SubmitButton
│── FeedbackText (TextMeshPro)
```

---

# **📜 Code Documentation - `MastermindGame.cs`**

## **📌 Purpose**

This script handles:

- **Generating the secret code**
- **Tracking player guesses**
- **Validating guesses**
- **Providing hints**
- **Managing UI updates (row highlighting, color selection, etc.)**

---

## **📜 Final `MastermindGame.cs`**

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Using TextMeshPro for text rendering

public class MastermindGame : MonoBehaviour
{
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

    private void Start()
    {
        ExtractColorsFromButtons();
        GenerateSecretCode();
        AssignColorButtons();
        submitButton.onClick.AddListener(CheckGuess);
        HighlightCurrentRow();
    }

    void ExtractColorsFromButtons()
    {
        availableColors.Clear();

        foreach (Button btn in colorButtons)
        {
            if (btn != null && btn.image != null)
            {
                availableColors.Add(btn.image.color);
            }
            else
            {
                Debug.LogWarning("Warning: One or more color buttons are missing an Image component.");
            }
        }

        if (availableColors.Count == 0)
        {
            Debug.LogError("Error: No colors found in color buttons! Assign colors to the buttons.");
        }
    }

    void GenerateSecretCode()
    {
        if (availableColors.Count == 0)
        {
            Debug.LogError("Error: Cannot generate secret code. No colors available.");
            return;
        }

        secretCode = new Color[4];
        for (int i = 0; i < 4; i++)
        {
            secretCode[i] = availableColors[Random.Range(0, availableColors.Count)];
            secretCodeSlots[i].color = Color.black; // Keep the secret code hidden
        }
    }

    void AssignColorButtons()
    {
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int index = i;
            colorButtons[i].onClick.AddListener(() => SelectColor(index));
        }
    }

    public void SelectColor(int colorIndex)
    {
        if (guessIndex < 4)
        {
            Image[] currentGuessSlots = guessRows[currentRow].transform.GetComponentsInChildren<Image>();

            List<Image> slotImages = new List<Image>();
            foreach (Image img in currentGuessSlots)
            {
                if (img.gameObject != guessRows[currentRow])
                {
                    slotImages.Add(img);
                }
            }

            if (guessIndex < slotImages.Count)
            {
                playerGuess[guessIndex] = availableColors[colorIndex];
                slotImages[guessIndex].color = availableColors[colorIndex];
            }

            guessIndex++;
        }
    }

    void CheckGuess()
    {
        if (currentRow >= guessRows.Length) return;

        int correctPosition = 0, correctColor = 0;
        bool[] checkedSecret = new bool[4];
        bool[] checkedGuess = new bool[4];

        for (int i = 0; i < 4; i++)
        {
            if (playerGuess[i] == secretCode[i])
            {
                correctPosition++;
                checkedSecret[i] = true;
                checkedGuess[i] = true;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (!checkedGuess[i])
            {
                for (int j = 0; j < 4; j++)
                {
                    if (!checkedSecret[j] && playerGuess[i] == secretCode[j])
                    {
                        correctColor++;
                        checkedSecret[j] = true;
                        break;
                    }
                }
            }
        }

        ShowHints(correctPosition, correctColor, currentRow);

        if (correctPosition == 4)
        {
            feedbackText.text = "🎉 You Win! 🎉";
        }
        else
        {
            currentRow++;
            if (currentRow < guessRows.Length)
            {
                HighlightCurrentRow();
            }
            else
            {
                feedbackText.text = "❌ Game Over! Try Again.";
            }
        }

        guessIndex = 0;
    }

    void ShowHints(int correct, int misplaced, int rowIndex)
    {
        Transform hintRow = hintRows[rowIndex].transform;
        for (int i = 0; i < correct; i++)
        {
            hintRow.GetChild(i).GetComponent<Image>().color = Color.green;
        }
        for (int i = 0; i < misplaced; i++)
        {
            hintRow.GetChild(correct + i).GetComponent<Image>().color = Color.yellow;
        }
    }

    void HighlightCurrentRow()
    {
        for (int i = 0; i < guessRows.Length; i++)
        {
            guessRows[i].GetComponent<Image>().color = Color.gray;
        }

        if (currentRow < guessRows.Length)
        {
            guessRows[currentRow].GetComponent<Image>().color = Color.white;
        }
    }
}
```

---

## **🎯 Final Summary**

✅ **Fully functional Mastermind game in Unity!**  
✅ **Auto-detects colors from buttons.**  
✅ **Correctly assigns colors to guess slots.**  
✅ **Highlights the active row.**  
✅ **Uses `TextMeshPro` for feedback.**

Let me know if you need any modifications! 🚀🔥😊
