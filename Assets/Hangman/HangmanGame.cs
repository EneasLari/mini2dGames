using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HangmanGame : MonoBehaviour {
    public TMP_Text hiddenWordText; // UI text showing "_ _ _ _"
    public TMP_Text incorrectGuessesText; // Shows incorrect letters guessed
    public GameObject[] hangmanParts; // Array of hangman parts to show progressively
    public Button restartButton; // Restart game button
    public GameObject letterButtonPrefab; // Prefab for letter buttons
    public Transform letterButtonContainer; // Parent container (grid) for buttons

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
        hiddenWordText.text = won ? " You Win! " : $" You Lose! \nWord: {selectedWord}";

        // Disable all buttons
        foreach (Button button in letterButtons) button.interactable = false;
    }
}
