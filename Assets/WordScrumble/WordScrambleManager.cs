using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordScrambleManager : MonoBehaviour {
    public GameObject letterTilePrefab;
    public Transform letterContainer; // Horizontal Layout Group
    public TMP_Text timerText, hintText, scoreText;
    public Button submitButton, hintButton;

    private string originalWord;
    private List<GameObject> letterTiles = new List<GameObject>();
    private float timeRemaining = 30f;
    private int score = 1000;
    private bool isGameActive = true;

    private Dictionary<string, string> wordList = new Dictionary<string, string>()
    {
        { "planet", "A celestial body orbiting a star" },
        { "bridge", "Structure to cross over a river" },
        { "guitar", "A musical instrument with strings" },
        { "school", "A place where students learn" },
        { "castle", "A fortified structure from medieval times" }
    };

    void Start() {
        ChooseWord();
        submitButton.onClick.AddListener(CheckAnswer);
        hintButton.onClick.AddListener(ShowHint);
        scoreText.text = "Score: " + score;
    }

    void Update() {
        if (isGameActive) {
            timeRemaining -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();

            if (timeRemaining <= 0) {
                isGameActive = false;
                timerText.text = "Time's Up!";
                hintText.text = "The word was: " + originalWord;
            }
        }
    }

    void ChooseWord() {
        int randomIndex = Random.Range(0, wordList.Count);
        originalWord = new List<string>(wordList.Keys)[randomIndex];
        string scrambledWord = ScrambleWord(originalWord);

        foreach (char letter in scrambledWord) {
            GameObject tile = Instantiate(letterTilePrefab, letterContainer);
            tile.GetComponentInChildren<TMP_Text>().text = letter.ToString();
            letterTiles.Add(tile);
        }
    }

    string ScrambleWord(string word) {
        char[] letters = word.ToCharArray();
        for (int i = 0; i < letters.Length; i++) {
            int randomIndex = Random.Range(0, letters.Length);
            char temp = letters[i];
            letters[i] = letters[randomIndex];
            letters[randomIndex] = temp;
        }
        return new string(letters);
    }

    public void CheckAnswer() {
        string playerWord = "";

        // **Check order of letters in the Horizontal Layout**
        for (int i = 0; i < letterContainer.childCount; i++) {
            playerWord += letterContainer.GetChild(i).GetComponentInChildren<TMP_Text>().text;
        }

        if (playerWord == originalWord) {
            hintText.text = "Correct!";
            isGameActive = false;
        } else {
            hintText.text = "Try Again!";
        }
    }

    public void ShowHint() {
        hintText.text = "Hint: " + wordList[originalWord];
        score -= 500; // Deduct points for using hint
        scoreText.text = "Score: " + score;
    }
}
