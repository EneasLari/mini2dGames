using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public Button startButton;
    public GameObject mainMenuCanvas;
    public GameObject mainGameCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        mainGameCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
        startButton.onClick.AddListener(StartGame);
    }

    // Update is called once per frame
    void Update() {

    }

    void StartGame() {
        if (NameThePicture.Instance != null) {
            NameThePicture.Instance.StartGame();
            mainGameCanvas.SetActive(true);
            mainMenuCanvas.SetActive(false);
        } else {
            Debug.LogError("NameThePicture instance not found!");
        }
    }
}
