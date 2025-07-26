using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LetterMenuManager : MonoBehaviour {
    public GameObject mainMenuPanel;
    public GameObject inGameMenuPanel;
    public GameObject hudPanel; // Your main game UI (score, timer, etc.)

    public Button startGameButton;
    public Button quitButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button pauseButton;



    private bool isPaused = false;

    void Awake() {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(ShowInGameMenu);
    }

    private void Start() {
        ShowMainMenu();
    }

    // MAIN MENU
    public void ShowMainMenu() {
        Time.timeScale = 0f; // Pause everything
        mainMenuPanel.SetActive(true);
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
    }

    // GAME START
    public void StartGame() {
        mainMenuPanel.SetActive(false);
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(true);
        Time.timeScale = 1f;
        LetterGridGameManager.Instance.StartNewRoundAtCurrentLevel();
    }

    // IN-GAME MENU
    public void ShowInGameMenu() {
        isPaused = true;
        Time.timeScale = 0f;
        inGameMenuPanel.SetActive(true);
        hudPanel.SetActive(false);
    }

    public void ResumeGame() {
        isPaused = false;
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(true);
        Time.timeScale = 1f;
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void ReturnToMainMenu() {
        // You might want to reset game state here
        ShowMainMenu();
    }

}
