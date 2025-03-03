using UnityEngine;
using UnityEngine.UI;

public class LetterGridLetterTile : MonoBehaviour {
    private char letter;
    private bool isSelected = false;
    private Button button;

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectLetter);
    }

    public void SetLetter(char newLetter) {
        letter = newLetter;
        GetComponentInChildren<Text>().text = letter.ToString();
    }

    void SelectLetter() {
        if (!isSelected) {
            LetterGridWordManager.instance.AddLetter(letter);
            isSelected = true;
            GetComponent<Image>().color = Color.yellow; // Highlight selection
        }
    }

    public void Deselect() {
        isSelected = false;
        GetComponent<Image>().color = Color.white; // Reset color
    }
}
