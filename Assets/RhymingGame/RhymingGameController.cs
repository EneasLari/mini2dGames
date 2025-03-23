using System.Collections.Generic;
using UnityEngine;
using TMPro;  // For using TextMesh Pro
using UnityEngine.UI;
using System.Collections;  // For Button & Text components

public class RhymingGameController : MonoBehaviour {
    public TMP_Text wordText;  // TextMesh Pro Text to display the word
    public TMP_Text feedbackText;  // Text to display feedback (correct/incorrect)
    public Button[] optionButtons;  // Array of buttons for the choices
    public Color correctColor = Color.green;  // Color for correct answers
    public Color initilColor;  // Initial color for buttons
    // A simple dictionary of words and their rhymes (simplified)
    private Dictionary<string, List<string>> rhymeDictionary = new Dictionary<string, List<string>>()
    {
        { "cat", new List<string> { "hat", "bat", "rat", "mat" } },
        { "dog", new List<string> { "log", "frog", "hog", "jog" } },
        { "fish", new List<string> { "dish", "wish", "swish" } },
        { "tree", new List<string> { "bee", "free", "see", "three" } },
        { "sun", new List<string> { "fun", "run", "bun", "lan" } },
        { "star", new List<string> { "car", "far", "jar", "bar" } },
        { "moon", new List<string> { "soon", "noon", "spoon", "balloon" } },
        { "light", new List<string> { "night", "sight", "fight", "kite" } },
        { "book", new List<string> { "look", "cook", "hook", "took" } },
        { "mouse", new List<string> { "house", "spouse", "blouse", "louse" } },
        { "car", new List<string> { "bar", "far", "jar", "star" } },
        { "plane", new List<string> { "train", "lane", "gain", "main" } },
        { "phone", new List<string> { "tone", "bone", "cone", "zone" } },
        { "ring", new List<string> { "sing", "king", "wing", "bring" } },
        { "ball", new List<string> { "call", "fall", "hall", "mall" } }
    };

    // List of unrelated words (that are not in the rhyme dictionary)
    private List<string> nonRhymingWords = new List<string>
    {
        "apple", "banana", "grape", "orange", "pear", "peach", "plum", "kiwi", "mango", "berry",
        "computer", "keyboard", "monitor", "desk", "chair", "window", "door", "floor", "ceiling", "wall"
    };

    private string currentWord;  // Current word to rhyme with
    private int correctAnswers = 0; // Count correct answers

    void Start() {
        initilColor = optionButtons[0].GetComponent<Image>().color;
        // Prepare the game
        SetupNewRound();
    }

    void SetupNewRound() {
        print("Setting up new round...");
        correctAnswers = 0; // Reset correct answers count for the new round

        // Reset button states
        foreach (Button button in optionButtons) {
            button.interactable = true;
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null) {
                buttonImage.color = initilColor;  // Reset to initial color
            }
        }

        // Get a random word from the dictionary keys
        List<string> keys = new List<string>(rhymeDictionary.Keys);
        currentWord = keys[Random.Range(0, keys.Count)];

        // Display the word in the UI
        wordText.text = "Find a rhyme for: " + currentWord;
        feedbackText.text = "";  // Clear previous feedback

        // Get the correct rhymes for the current word
        List<string> correctRhymes = rhymeDictionary[currentWord];

        // Shuffle the correct rhymes and create fake rhymes
        List<string> allOptions = new List<string>(correctRhymes);

        // Add fake rhymes to the list (make sure to add only a few)
        while (allOptions.Count < optionButtons.Length) {
            string fakeRhyme = GetRandomFakeRhyme();
            if (!allOptions.Contains(fakeRhyme))
                allOptions.Add(fakeRhyme);
        }

        // Shuffle all options to make them random
        ShuffleList(allOptions);

        // Assign options to buttons
        for (int i = 0; i < optionButtons.Length; i++) {
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = allOptions[i];
            int index = i;  // Capture current index for button callback
            optionButtons[i].onClick.RemoveAllListeners();  // Remove any previous listeners
            optionButtons[i].onClick.AddListener(() => CheckRhyme(allOptions[index], optionButtons[index]));
        }
    }

    void CheckRhyme(string playerChoice, Button clickedButton) {
        // Check if the answer is correct
        if (rhymeDictionary[currentWord].Contains(playerChoice)) {
            feedbackText.text = "Correct! Well done!";
            correctAnswers++;

            // Change button color to indicate it's been selected
            Image buttonImage = clickedButton.GetComponent<Image>();
            if (buttonImage != null) {
                buttonImage.color = Color.green;
            }
            clickedButton.interactable = false;
        } else {
            feedbackText.text = "Try again!";
        }

        // Move to next question after all correct rhymes are found
        if (correctAnswers == rhymeDictionary[currentWord].Count) {
            StartCoroutine(WaitAndSetupNewRound(1f));
        }
    }

    IEnumerator WaitAndSetupNewRound(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        SetupNewRound();
    }

    string GetRandomFakeRhyme() {
        // Select a random word from non-rhyming list
        string fakeRhyme;
        do {
            fakeRhyme = nonRhymingWords[Random.Range(0, nonRhymingWords.Count)];
        } while (rhymeDictionary[currentWord].Contains(fakeRhyme));  // Ensure no overlap with rhymes
        return fakeRhyme;
    }

    // Shuffle the list of strings (simple Fisher-Yates shuffle)
    void ShuffleList(List<string> list) {
        for (int i = list.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            string temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
