using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class NameThePicture : MonoBehaviour {
    public Image displayImage;
    public TMP_Text[] optionTexts;
    public TMP_Text resulText;
    public Button[] optionButtons;
    public PictureData pictureData; // Reference the ScriptableObject

    private Dictionary<string, Sprite> imageDictionary;
    private string correctAnswer;
    private int correctIndex;
    void Start() {
        LoadImages();
        LoadNewQuestion();
    }

    void LoadImages() {
        imageDictionary = new Dictionary<string, Sprite>();

        foreach (PictureEntry entry in pictureData.pictures) {
            if (entry.image != null) {
                imageDictionary.Add(entry.name, entry.image);
            } else {
                Debug.LogError("Missing image for: " + entry.name);
            }
        }

        for (int i = 0; i < optionButtons.Length; i++) {
            int index = i;
            optionButtons[i].onClick.AddListener(() => StartCoroutine(SelectOption(index)));
        }
    }

    void  LoadNewQuestion() {
        resulText.text = "";
        if (imageDictionary.Count == 0) {
            Debug.LogError("No images loaded!");
            return;
        }

        List<string> keys = new List<string>(imageDictionary.Keys);
        int randomIndex = Random.Range(0, keys.Count);
        correctAnswer = keys[randomIndex];

        if (!imageDictionary.ContainsKey(correctAnswer)) {
            Debug.LogError("Image not found in dictionary: " + correctAnswer);
            return;
        }

        displayImage.sprite = imageDictionary[correctAnswer];

        Debug.Log("Correct answer: " + correctAnswer);

        correctIndex = Random.Range(0, optionTexts.Length);
        List<string> usedOptions = new List<string> { correctAnswer };

        for (int i = 0; i < optionTexts.Length; i++) {
            if (i == correctIndex) {
                optionTexts[i].text = correctAnswer;
            } else {
                string randomOption;
                do {
                    randomOption = keys[Random.Range(0, keys.Count)];
                } while (usedOptions.Contains(randomOption));

                usedOptions.Add(randomOption);
                optionTexts[i].text = randomOption;
            }
        }
    }

    public IEnumerator SelectOption(int index) {
        if (index == correctIndex) {
            resulText.text = "Correct Answer!";
            yield return new WaitForSeconds(1f);
            LoadNewQuestion();
        } else {
            resulText.text = "Try Again!";
            yield return new WaitForSeconds(1f);
            resulText.text = "";
        }
    }
}
