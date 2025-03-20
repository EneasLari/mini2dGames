using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class NameThePicture : MonoBehaviour {
    public Image displayImage;
    public TMP_Text[] optionTexts;
    public TMP_Text resulText;
    public TMP_Text gameFinishedTxt;
    public Button replayButton; // Button to replay the game
    public GameObject gameFinishedPanel; // Panel to display when the game is finished
    public Button[] optionButtons;
    public PictureData pictureData; // Reference the ScriptableObject
    public AudioSource audioSource; // AudioSource to play the audio clips
    public AudioClip wrongAnswerAudioClip; // Audio clip for wrong answers
    public AudioClip universalCorrectAudioClip; // Universal audio clip for correct answers
    public AudioSource backgroundMusicSource; // AudioSource for background music
    public AudioClip[] backgroundMusicTracks; // Array of background music tracks

    private string correctAnswer;
    private int correctIndex;
    private AudioClip correctAudioClip;
    private List<int> usedIndices = new List<int>();

    void Start() {
        gameFinishedPanel.SetActive(false);
        gameFinishedTxt.text = "";
        replayButton.gameObject.SetActive(false);
        replayButton.onClick.AddListener(ReplayGame);
        if (ValidatePictureData()) {
            LoadNewQuestion();
        }
        StartCoroutine(PlayBackgroundMusic());
    }

    bool ValidatePictureData() {
        foreach (PictureEntry entry in pictureData.pictures) {
            if (entry.image == null) {
                Debug.LogError("Missing image for: " + entry.name);
                return false;
            }
        }
        return true;
    }

    void LoadNewQuestion() {
        resulText.text = "";
        if (pictureData.pictures.Count == 0) {
            Debug.LogError("No images loaded!");
            return;
        }

        if (usedIndices.Count == pictureData.pictures.Count) {
            gameFinishedTxt.text = "Congratulations! You've answered all questions!";
            gameFinishedPanel.SetActive(true);
            replayButton.gameObject.SetActive(true);
            return;
        }

        List<PictureEntry> pictures = pictureData.pictures;
        int randomIndex;
        do {
            randomIndex = Random.Range(0, pictures.Count);
        } while (usedIndices.Contains(randomIndex));

        usedIndices.Add(randomIndex);
        PictureEntry correctEntry = pictures[randomIndex];
        correctAnswer = correctEntry.name;
        correctAudioClip = correctEntry.audio;

        displayImage.sprite = correctEntry.image;

        Debug.Log("Correct answer: " + correctAnswer);

        correctIndex = Random.Range(0, optionTexts.Length);
        List<string> usedOptions = new List<string> { correctAnswer };

        for (int i = 0; i < optionTexts.Length; i++) {
            if (i == correctIndex) {
                optionTexts[i].text = correctAnswer;
            } else {
                string randomOption;
                do {
                    randomOption = pictures[Random.Range(0, pictures.Count)].name;
                } while (usedOptions.Contains(randomOption));

                usedOptions.Add(randomOption);
                optionTexts[i].text = randomOption;
            }
        }

        for (int i = 0; i < optionButtons.Length; i++) {
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => StartCoroutine(SelectOption(index)));
        }
    }

    public IEnumerator SelectOption(int index) {
        if (index == correctIndex) {
            resulText.text = "Correct Answer!";
            if (correctAudioClip != null) {
                audioSource.PlayOneShot(correctAudioClip);
            } else if (universalCorrectAudioClip != null) {
                audioSource.PlayOneShot(universalCorrectAudioClip);
            }
            yield return new WaitForSeconds(1f);
            LoadNewQuestion();
        } else {
            resulText.text = "Try Again!";
            if (wrongAnswerAudioClip != null) {
                audioSource.PlayOneShot(wrongAnswerAudioClip);
            }
            yield return new WaitForSeconds(1f);
            resulText.text = "";
        }
    }

    void ReplayGame() {
        usedIndices.Clear();
        gameFinishedPanel.SetActive(false);
        gameFinishedTxt.text = "";
        replayButton.gameObject.SetActive(false);
        LoadNewQuestion();
    }

    IEnumerator PlayBackgroundMusic() {
        while (true) {
            List<AudioClip> shuffledTracks = new List<AudioClip>(backgroundMusicTracks);
            Shuffle(shuffledTracks);

            foreach (AudioClip track in shuffledTracks) {
                backgroundMusicSource.clip = track;
                backgroundMusicSource.Play();
                yield return new WaitForSeconds(track.length);
            }
        }
    }

    void Shuffle<T>(List<T> list) {
        for (int i = 0; i < list.Count; i++) {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
