using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class LetterGridWordManager : MonoBehaviour {
    public static LetterGridWordManager instance;

    public TMP_Text selectedWordText;
    public TMP_Text scoreText;
    public Button submitButton;
    public Button resetButton;

    private string currentWord = "";
    private int score = 0;
    private HashSet<string> validWords = new HashSet<string>();
    private List<LetterGridLetterTile> currentSelectedLetterTiles = new List<LetterGridLetterTile>();

    private void Awake() {
        instance = this;
    }

    private void Start() {
        LoadDictionary();
        submitButton.onClick.AddListener(ValidateWord);
        resetButton.onClick.AddListener(ResetSelectedTiles);
    }

    public void AddLetter(LetterGridLetterTile letterTile) {
        currentWord += letterTile.GetComponentInChildren<TMP_Text>().text;
        currentSelectedLetterTiles.Add(letterTile);
        selectedWordText.text = "Word: " + currentWord;
    }

    public void ValidateWord() {
        if (validWords.Contains(currentWord)) {
            score += currentWord.Length * 10; // Score based on word length
            scoreText.text = "Score: " + score;
        }
        ResetSelection();
        currentSelectedLetterTiles.Clear();
    }

    public void ResetSelection() {
        currentWord = "";
        selectedWordText.text = "Word: ";
    }

    public void ResetSelectedTiles() {
        ResetSelection();
        foreach (var item in currentSelectedLetterTiles) {
            item.Deselect();
        }
        currentSelectedLetterTiles.Clear();
    }

    void LoadDictionary() {
        TextAsset wordFile = Resources.Load<TextAsset>("wordlist"); // Place wordlist.txt in Resources folder
        string[] words = wordFile.text.Split('\n');
        foreach (string word in words) {
            validWords.Add(word.Trim().ToUpper());
        }
    }
}
