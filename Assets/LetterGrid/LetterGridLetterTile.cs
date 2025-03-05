using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridLetterTile : MonoBehaviour {
    private char letter;
    private bool isSelected = false;
    private Button button;
    private Color currentColor;

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(SelectLetter);
        currentColor = GetComponent<Image>().color;
    }

    public void SetLetter(char newLetter) {
        letter = newLetter;
        GetComponentInChildren<TMP_Text>().text = letter.ToString();
    }

    void SelectLetter() {
        if (!isSelected) {
            LetterGridWordManager.instance.AddLetter(this);
            isSelected = true;
            GetComponent<Image>().color = Color.yellow; // Highlight selection
        }
    }

    public void Deselect() {
        isSelected = false;
        GetComponent<Image>().color = currentColor; // Reset color
    }
}
