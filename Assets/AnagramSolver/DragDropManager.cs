using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DragDropManager : MonoBehaviour {
    public GameObject letterPrefab; // Prefab for draggable letters
    public GameObject dropSlotPrefab; // Prefab for drop slots
    public Transform letterContainer; // Where scrambled letters appear
    public Transform dropZoneContainer; // Where users place letters

    public Button checkButton, shuffleButton, hintButton, restartButton;
    public TextMeshProUGUI timerText, scoreText, statusText;
    public Image GameStatusPanel;

    private string correctWord;
    private string scrambledWord;
    private int score = 0;
    private float timeLeft = 30f;
    private bool isGameActive = true;
    private bool isPaused = false; // NEW: Track if timer is paused

    private List<string> wordList = new List<string> { "UNITY", "SCRIPT", "BUTTON", "TEXT", "GAME", "CANVAS" };
    private List<GameObject> letterObjects = new List<GameObject>();

    void Start() {
        restartButton.gameObject.SetActive(false); // Hide restart button at start
        GameStatusPanel.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);

        GenerateNewWord();
        checkButton.onClick.AddListener(CheckAnswer);
        shuffleButton.onClick.AddListener(ShuffleLetters);
        hintButton.onClick.AddListener(GiveHint);
        StartCoroutine(TimerCountdown());
    }

    void GenerateNewWord() {
        if (!isGameActive) return; // Stop generating new words if game over

        correctWord = wordList[Random.Range(0, wordList.Count)];
        scrambledWord = ScrambleWord(correctWord);

        CreateLetterObjects(scrambledWord);
        CreateDropSlots(correctWord.Length); // Create slots equal to word length
    }

    string ScrambleWord(string word) {
        char[] letters = word.ToCharArray();
        int length = letters.Length;

        if (length <= 1) return word; // No need to scramble single-letter words

        do {
            for (int i = 0; i < length; i++) {
                int randomIndex = Random.Range(0, length);
                char temp = letters[i];
                letters[i] = letters[randomIndex];
                letters[randomIndex] = temp;
            }
        } while (new string(letters) == word); // Keep scrambling until it changes

        return new string(letters);
    }

    void CreateLetterObjects(string word) {
        // Clear previous letters
        foreach (GameObject letter in letterObjects) {
            Destroy(letter);
        }
        letterObjects.Clear();

        // Generate new draggable letters
        foreach (char letter in word) {
            GameObject newLetter = Instantiate(letterPrefab, letterContainer);
            newLetter.GetComponentInChildren<TextMeshProUGUI>().text = letter.ToString();
            letterObjects.Add(newLetter);
        }
    }

    void CreateDropSlots(int wordLength) {
        // Clear previous drop slots
        foreach (Transform child in dropZoneContainer) {
            Destroy(child.gameObject);
        }

        // Generate new drop slots based on word length
        for (int i = 0; i < wordLength; i++) {
            Instantiate(dropSlotPrefab, dropZoneContainer);
        }
    }

    void CheckAnswer() {
        string playerWord = "";
        foreach (Transform slot in dropZoneContainer) {
            if (slot.childCount > 0) {
                playerWord += slot.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text;
            }
        }

        if (playerWord == correctWord) {
            score += 10;
            scoreText.text = "Score: " + score;

            StartCoroutine(DisplayCorrectAnswerMessage()); // Show message before next word
        }
    }

    IEnumerator DisplayCorrectAnswerMessage() {
        isPaused = true; // NEW: Pause the timer
        statusText.gameObject.SetActive(true); // Show message
        GameStatusPanel.gameObject.SetActive(true);
        statusText.text = "Correct! Well Done!";

        yield return new WaitForSeconds(3f); // Wait for 3 seconds

        statusText.gameObject.SetActive(false); // Hide message
        GameStatusPanel.gameObject.SetActive(false);
        GenerateNewWord(); // Load new word
        isPaused = false; // NEW: Resume the timer
    }

    IEnumerator TimerCountdown() {
        while (timeLeft > 0 && isGameActive) {
            yield return new WaitForSeconds(1f);

            if (!isPaused) // NEW: Only decrease time if NOT paused
            {
                timeLeft--;
                timerText.text = "Time Left: " + timeLeft + "s";
            }
        }

        isGameActive = false;
        StartCoroutine(DisplayGameOverMessage());
    }

    IEnumerator DisplayGameOverMessage() {
        statusText.gameObject.SetActive(true);
        GameStatusPanel.gameObject.SetActive(true);
        statusText.text = "Time's Up! Game Over!";
        restartButton.gameObject.SetActive(true); // Show restart button

        yield return new WaitForSeconds(3f);
    }

    void RestartGame() {
        score = 0;
        timeLeft = 30f;
        isGameActive = true;
        isPaused = false;
        scoreText.text = "Score: 0";
        timerText.text = "Time Left: 30s";

        statusText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        GameStatusPanel.gameObject.SetActive(false);

        GenerateNewWord();
        StartCoroutine(TimerCountdown());
    }

    void ShuffleLetters() {
        scrambledWord = ScrambleWord(correctWord);
        CreateLetterObjects(scrambledWord);
    }

    void GiveHint() {
        foreach (Transform slot in dropZoneContainer) {
            if (slot.childCount == 0) {
                GameObject hintLetter = Instantiate(letterPrefab, slot);
                hintLetter.GetComponentInChildren<TextMeshProUGUI>().text = correctWord[0].ToString();
                break;
            }
        }
    }
}
