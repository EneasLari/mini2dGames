# **📜 Hangman Game Documentation (Unity) 🎭**

This documentation provides a step-by-step guide to creating a **Hangman** game in **Unity** using **C#** and the **Unity UI system**, with **3D hangman parts** and a **QWERTY keyboard layout in a single GridLayoutGroup**.

---

## **🛠 Steps to Build Hangman in Unity**

We'll break it down into:

1️⃣ **Setting Up the Scene**  
2️⃣ **Creating the Word Selection System**  
3️⃣ **Handling Player Input**  
4️⃣ **Updating the UI**  
5️⃣ **Implementing the 3D Hangman**  
6️⃣ **Win/Lose Conditions**

---

## **1️⃣ Scene Setup in Unity**

🎮 Open **Unity** and create a **3D project**.

### **👀 UI Elements Needed**

- **TMP_Text** to display the hidden word (`_ _ _ _`).
- **TMP_Text** for incorrect guesses.
- **Buttons** (or keyboard input) for letter selection.
- **3D objects** for the Hangman parts.
- **Restart Button** (for replaying).

#### **Hierarchy Example**

```
🔹 Canvas (UI)
  ├── HiddenWordText (TMP_Text)
  ├── IncorrectGuessesText (TMP_Text)
  ├── LetterButtons (Grid Layout Group - 10 columns)
  │   ├── LetterButtonPrefab (Prefab for A-Z)
  ├── RestartButton (Button)

🔹 HangmanParts (Empty Object for 3D Hangman)
  ├── Head (3D Sphere)
  ├── Body (3D Capsule or Cylinder)
  ├── LeftArm (3D Cylinder)
  ├── RightArm (3D Cylinder)
  ├── LeftLeg (3D Cylinder)
  ├── RightLeg (3D Cylinder)

🔹 Main Camera (Adjusted for 3D view)
🔹 Directional Light (Lighting for 3D objects)
```

---

## **2️⃣ C# Script: Game Logic**

Now, create a **C# script** called **`HangmanGame.cs`** and attach it to an **empty GameObject** (`HangmanManager`).

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HangmanGame : MonoBehaviour {
    public TMP_Text hiddenWordText; // UI text showing "_ _ _ _"
    public TMP_Text incorrectGuessesText; // Shows incorrect letters guessed
    public GameObject[] hangmanParts; // Array of 3D hangman parts
    public Button restartButton; // Restart game button
    public GameObject letterButtonPrefab; // Prefab for letter buttons
    public Transform letterButtonContainer; // Parent container (GridLayout)

    private string[] wordList = { "UNITY", "GAMING", "SCRIPT", "HANGMAN", "DEVELOPER" };
    private string selectedWord;
    private char[] displayedWord;
    private List<char> incorrectGuesses = new List<char>();
    private int mistakes = 0;
    private List<Button> letterButtons = new List<Button>(); // Store created buttons

    void Start() {
        restartButton.onClick.AddListener(RestartGame);
        GenerateLetterButtons();
        RestartGame();
    }

    void GenerateLetterButtons() {
        // Clear previous buttons (if restarting)
        foreach (Transform child in letterButtonContainer) {
            Destroy(child.gameObject);
        }
        letterButtons.Clear();

        string letters = "QWERTYUIOPASDFGHJKLZXCVBNM"; // QWERTY order

        foreach (char letter in letters) {
            GameObject newButton = Instantiate(letterButtonPrefab, letterButtonContainer);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = letter.ToString();

            Button buttonComponent = newButton.GetComponent<Button>();
            letterButtons.Add(buttonComponent);
            buttonComponent.onClick.AddListener(() => OnLetterPressed(buttonComponent));
        }
    }

    void RestartGame() {
        // Pick a random word
        selectedWord = wordList[Random.Range(0, wordList.Length)];
        displayedWord = new string('_', selectedWord.Length).ToCharArray();

        // Reveal the first and last letter, including all their occurrences
        char firstLetter = selectedWord[0];
        char lastLetter = selectedWord[selectedWord.Length - 1];

        for (int i = 0; i < selectedWord.Length; i++) {
            if (selectedWord[i] == firstLetter || selectedWord[i] == lastLetter) {
                displayedWord[i] = selectedWord[i];
            }
        }

        hiddenWordText.text = string.Join(" ", displayedWord);

        // Reset mistakes & UI
        mistakes = 0;
        incorrectGuesses.Clear();
        incorrectGuessesText.text = "Incorrect: ";

        // Hide all hangman parts
        foreach (GameObject part in hangmanParts) part.SetActive(false);

        // Reactivate all letter buttons
        foreach (Button button in letterButtons) button.interactable = true;

    }

    void OnLetterPressed(Button button) {
        char letter = button.GetComponentInChildren<TMP_Text>().text[0];
        button.interactable = false; // Disable button after clicking

        if (selectedWord.Contains(letter.ToString())) {
            // Reveal correct letters
            for (int i = 0; i < selectedWord.Length; i++) {
                if (selectedWord[i] == letter)
                    displayedWord[i] = letter;
            }
            hiddenWordText.text = string.Join(" ", displayedWord);
        } else {
            // Wrong guess - Show next 3D hangman part
            incorrectGuesses.Add(letter);
            incorrectGuessesText.text = "Incorrect: " + string.Join(" ", incorrectGuesses);
            if (mistakes < hangmanParts.Length) {
                hangmanParts[mistakes].SetActive(true);
            }
            mistakes++;

            if (mistakes >= hangmanParts.Length) {
                GameOver(false);
            }
        }

        // Check for win condition
        if (!new string(displayedWord).Contains("_")) {
            GameOver(true);
        }
    }

    void GameOver(bool won) {
        hiddenWordText.text = won ? "🎉 You Win! 🎉" : $"💀 You Lose! 💀\nWord: {selectedWord}";

        // Disable all buttons
        foreach (Button button in letterButtons) button.interactable = false;
    }
}
```

---

## **3️⃣ Explanation of the Code**

🔹 **`RestartGame()`**

- Chooses a random word from `wordList`.
- Initializes the display (`_ _ _ _`).
- Hides all **3D Hangman parts**.
- Reactivates all buttons.

🔹 **`OnLetterPressed()`**

- Checks if the guessed letter is in the word.
- Updates the hidden word or shows an incorrect guess.
- Reveals a **3D Hangman part** if wrong.

🔹 **`GameOver(bool won)`**

- Ends the game by displaying a **win/lose message**.
- Disables all letter buttons.

---

## **4️⃣ Implementing 3D Hangman**

🔹 **Steps to Set Up 3D Hangman Parts:**

1. **Replace 2D images** with **3D objects**:
   - **Head** → Use a **Sphere**.
   - **Body** → Use a **Capsule** or **Cylinder**.
   - **Arms & Legs** → Use **Cylinders**.
2. **Organize them** inside an empty **GameObject (`HangmanParts`)**.
3. **Position them correctly** using the Scene View.
4. **Assign all parts** to the `hangmanParts[]` array in **Unity Inspector**.

---

## **5️⃣ Adjusting the Scene for 3D**

🎥 **Camera Settings**:

- Set **Projection** → `Perspective`.
- Adjust **Position (Z-axis)** to frame the Hangman.

💡 **Lighting**:

- Add a **Directional Light**.
- Ensure shadows are enabled for **3D objects**.

---

## **6️⃣ UI Improvements**

🎨 **Letter Buttons**:

- Stored in a **single GridLayoutGroup** (`10 columns`).
- Uses **QWERTY layout**.

🎵 **Sounds**:

- Add sound effects for **correct/wrong guesses**.

🎬 **Animations**:

- Use **Scale/Rotation** to make the Hangman appear dynamically.

---

### **🎯 Now, your game is fully 3D with QWERTY buttons and a dynamic Hangman model!** 🚀

Would you like to add **animations, sound effects, or difficulty levels**? 😃
