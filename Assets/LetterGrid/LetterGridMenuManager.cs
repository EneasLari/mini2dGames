using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class LetterMenuManager : MonoBehaviour {
    public GameObject mainMenuPanel;
    public GameObject settingsMenuPanel;
    public GameObject inGameMenuPanel;
    public GameObject hudPanel; // Your main game UI (score, timer, etc.)

    public Button startGameButton;
    public Button quitButton;
    public Button resumeButton;
    public Button mainMenuButton;
    public Button backtoMainMenuButton;
    public Button pauseButton;
    public Button settingsButton;

    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicMuteToggle;
    public Toggle sfxMuteToggle;    


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
        if (backtoMainMenuButton != null)
            backtoMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(SettingsMenu);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(ShowInGameMenu);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(LetterGridGameAudioEvents.RaiseMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(LetterGridGameAudioEvents.RaiseSFXVolumeChanged);
        if (musicMuteToggle != null)
            musicMuteToggle.onValueChanged.AddListener(LetterGridGameAudioEvents.RaiseMusicMuteChanged);
        if (sfxMuteToggle != null)
            sfxMuteToggle.onValueChanged.AddListener(LetterGridGameAudioEvents.RaiseSFXMuteChanged);


    }

    private void Start() {
        ShowMainMenu();
    }

    private void OnEnable() {
        // Subscribe to init values from the AudioManager
        LetterGridGameAudioEvents.OnMusicVolumeInit += HandleMusicVolumeInit;
        LetterGridGameAudioEvents.OnSFXVolumeInit += HandleSFXVolumeInit;
        LetterGridGameAudioEvents.OnMusicMuteInit += HandleMusicMuteInit;
        LetterGridGameAudioEvents.OnSFXMuteInit += HandleSFXMuteInit;
    }

    private void OnDisable() {
        // Unsubscribe
        LetterGridGameAudioEvents.OnMusicVolumeInit -= HandleMusicVolumeInit;
        LetterGridGameAudioEvents.OnSFXVolumeInit -= HandleSFXVolumeInit;
        LetterGridGameAudioEvents.OnMusicMuteInit -= HandleMusicMuteInit;
        LetterGridGameAudioEvents.OnSFXMuteInit -= HandleSFXMuteInit;
    }

    private void HandleMusicVolumeInit(float v) => musicVolumeSlider.value = v;
    private void HandleSFXVolumeInit(float v) => sfxVolumeSlider.value = v;
    private void HandleMusicMuteInit(bool b) => musicMuteToggle.isOn = b;
    private void HandleSFXMuteInit(bool b) => sfxMuteToggle.isOn = b;

    // MAIN MENU
    public void ShowMainMenu() {
        mainMenuPanel.SetActive(true);
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        settingsMenuPanel.SetActive(false);
    }

    public void ShowSettingsMenu() {
        mainMenuPanel.SetActive(false);
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    // GAME START
    public void StartGame() {
        mainMenuPanel.SetActive(false);
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(true);
        settingsMenuPanel.SetActive(false);
        LetterGridGameAudioEvents.RaiseStartGame();
        LetterGridGameManager.Instance.ResetGameTime();
        LetterGridGameManager.Instance.StartNewRoundAtCurrentLevel();
    }

    // IN-GAME MENU
    public void ShowInGameMenu() {
        isPaused = true;
        LetterGridGameManager.Instance.PauseGameTime();
        inGameMenuPanel.SetActive(true);
        hudPanel.SetActive(false);
    }

    public void ResumeGame() {
        isPaused = false;
        inGameMenuPanel.SetActive(false);
        hudPanel.SetActive(true);
        LetterGridGameManager.Instance.ResetGameTime();
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void ReturnToMainMenu() {
        // You might want to reset game state here
        ShowMainMenu();
    }

    public void SettingsMenu() {
        // You might want to reset game state here
        ShowSettingsMenu();
        LetterGridGameAudioEvents.RequestInitSettings();
    }

}
