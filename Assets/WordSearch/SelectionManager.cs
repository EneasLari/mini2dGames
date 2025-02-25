using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SelectionManager : MonoBehaviour {
    public WordSearchGridManager gridManager;
    public TMP_Text selectedWordText;
    private List<GameObject> selectedLetters = new List<GameObject>();
    private string selectedWord = "";
    private List<string> foundWords = new List<string>();

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            selectedLetters.Clear();
            selectedWord = "";
        }

        if (Input.GetMouseButton(0)) {
            GameObject letter = GetUIElementUnderMouse();
            if (letter != null && letter.CompareTag("Letter")) {
                if (!selectedLetters.Contains(letter)) {
                    selectedLetters.Add(letter);
                    selectedWord += letter.GetComponentInChildren<TMP_Text>().text;
                    selectedWordText.text = selectedWord;
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            CheckWord();
        }
    }

    private GameObject GetUIElementUnderMouse() {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results) {
            if (result.gameObject.CompareTag("Letter")) {
                return result.gameObject;
            }
        }
        return null;
    }

    void CheckWord() {
        if (gridManager.wordsToPlace.Contains(selectedWord) && !foundWords.Contains(selectedWord)) {
            foundWords.Add(selectedWord);
            foreach (GameObject letter in selectedLetters) {
                letter.GetComponent<Image>().color = Color.green;
            }
            if (foundWords.Count == gridManager.wordsToPlace.Count) {
                Debug.Log("You found all words! You win!");
            }
        } else {
            foreach (GameObject letter in selectedLetters) {
                letter.GetComponent<Image>().color = Color.white;
            }
        }
        selectedWord = "";
        selectedWordText.text = "";
        selectedLetters.Clear();
    }
}
