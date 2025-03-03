using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class LetterGridWordManager : MonoBehaviour {
    public static LetterGridWordManager instance;

    public Text selectedWordText;
    public Text scoreText;
    public Button submitButton;
    public Button resetButton;

    private string currentWord = "";
    private int score = 0;
    private HashSet<string> validWords = new HashSet<string>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        LoadDictionary();
        submitButton.onClick.AddListener(ValidateWord);
        resetButton.onClick.AddListener(ResetSelection);
    }

    public void AddLetter(char letter) {
        currentWord += letter;
        selectedWordText.text = "Word: " + currentWord;
    }

    public void ValidateWord() {
        if (validWords.Contains(currentWord)) {
            score += currentWord.Length * 10; // Score based on word length
            scoreText.text = "Score: " + score;
        }
        ResetSelection();
    }

    public void ResetSelection() {
        currentWord = "";
        selectedWordText.text = "Word: ";
    }

    void LoadDictionary() {
        TextAsset wordFile = Resources.Load<TextAsset>("wordlist"); // Place wordlist.txt in Resources folder
        string[] words = wordFile.text.Split('\n');
        foreach (string word in words) {
            validWords.Add(word.Trim().ToUpper());
        }
    }
}
