using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class TypingGame : MonoBehaviour {
    public GameObject letterPrefab; // Prefab for individual letter tiles
    public Transform sentenceGrid; // Grid to hold letters and SentenceGrid RectTransform
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public float timeLimit = 30f;

    private string[] sentences = {
        "NEVER STOP LEARNING",
        "UNITY GAME DEVELOPMENT",
        "PRACTICE MAKES PERFECT"
    };

    private string currentSentence;
    private List<GameObject> letterTiles = new List<GameObject>();
    private int currentLetterIndex = 0;
    private float timer;
    private bool gameActive = false;

    void Start() {
        StartGame();
    }

    void StartGame() {
        timer = timeLimit;
        gameActive = true;
        currentLetterIndex = 0;
        resultText.text = "";

        // Select a random sentence
        currentSentence = sentences[Random.Range(0, sentences.Length)];

        // Display the sentence as individual letters
        GenerateLetterTiles();
    }

    void GenerateLetterTiles() {
        // Clear old letters
        foreach (Transform child in sentenceGrid) {
            Destroy(child.gameObject);
        }
        letterTiles.Clear();

        // Get total available width of SentenceGrid
        float totalWidth = sentenceGrid.GetComponent<RectTransform>().rect.width;
        float tileWidth = totalWidth / currentSentence.Length; // Calculate width per letter

        foreach (char letter in currentSentence) {
            GameObject letterObj = Instantiate(letterPrefab, sentenceGrid);
            TextMeshProUGUI letterText = letterObj.GetComponentInChildren<TextMeshProUGUI>();
            letterText.text = letter.ToString();

            Image tileImage = letterObj.GetComponent<Image>();
            tileImage.color = Color.white; // Default color

            // Adjust the tile size dynamically
            RectTransform tileRect = letterObj.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(tileWidth, sentenceGrid.GetComponent<RectTransform>().rect.height);

            LayoutElement layout = letterObj.GetComponent<LayoutElement>();
            if (layout) {
                layout.preferredWidth = tileWidth;
                layout.preferredHeight = sentenceGrid.GetComponent<RectTransform>().rect.height;
            }

            letterTiles.Add(letterObj);
        }

        SetTilesColor(); // Start with the first letter highlighted
    }

    void Update() {
        if (gameActive) {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timer) + "s";

            if (Input.anyKeyDown) {
                HandleTyping();
            }

            if (currentLetterIndex >= currentSentence.Length) {
                EndGame(true);
            }

            if (timer <= 0) {
                EndGame(false);
            }
        }
    }

    void HandleTyping() {
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            if (currentLetterIndex > 0) {
                currentLetterIndex--;
                letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.white; // Reset to white
            }
            return;
        }

        if (currentLetterIndex >= currentSentence.Length)
            return;

        string keyPressed = Input.inputString.ToUpper();
        if (keyPressed.Length > 0) {
            char typedChar = keyPressed[0];
            char expectedChar = currentSentence[currentLetterIndex];

            if (typedChar == expectedChar) {
                letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.green; // Correct
                currentLetterIndex++;
                if (currentLetterIndex < letterTiles.Count - 1)
                    letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.yellow;
            } else {
                letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.red; // Incorrect
                currentLetterIndex++;
            }
        }
    }

    void SetTilesColor() {
        for (int i = 0; i < letterTiles.Count; i++) {
           letterTiles[i].GetComponent<Image>().color = Color.white; // Not yet typed
        }
    }

    void EndGame(bool success) {
        gameActive = false;
        if (success) {
            resultText.text = "You Win!";
        } else {
            resultText.text = "Time's Up!";
        }
    }
}
