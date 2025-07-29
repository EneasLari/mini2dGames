using UnityEngine;
using System.Collections;

public class LetterGridGameManager : MonoBehaviour {
    public static LetterGridGameManager Instance { get; private set; }

    [Header("⚙️ Difficulty Settings")]
    public int minWordLength = 3;
    public int maxWordLength = 6;
    public int minWordsToPlace = 3;
    public int maxWordsToPlace = 5;


    [Header("🔗 Manager References")]
    [SerializeField] public LetterGridWordManager wordManager;
    [SerializeField] public LetterGridManager gridManager;
    [SerializeField] public LetterGridTimerManager timerManager;

    [Header("🕹 Game State")]
    public bool isGameActive { get; private set; } = false;

    [Header("🔼 Difficulty Scaling")]
    public int currentLevel { get; private set; } = 1;
    public int maxLevel = 10;

    private void Awake() {
        if (Instance == null || (Instance != null && Instance != this)) Instance = this;
        else Destroy(gameObject);
    }

    //private void Start() {
    //    StartNewRoundAtCurrentLevel();
    //}

    /// <summary>
    /// Resets the board/grid and timer at the current level. DOES NOT change level.
    /// </summary>
    public void StartNewRoundAtCurrentLevel() {
        isGameActive = true;
        UpdateDifficultySettings();
        gridManager.StartGridManager();
        wordManager.enabled = true;
        
        wordManager.StartWordManager();
        timerManager.StartNewRound();
    }

    /// <summary>
    /// Call this when a round is failed or ended without level-up.
    /// Resets only the grid at current level.
    /// </summary>
    public void EndGame(string message) {
        isGameActive = false;
        timerManager.StopTimer();
        wordManager.enabled = false;
        wordManager.ShowLevelMessageUntilContinue(message);
    }

    public void PauseGameTime() {
        Time.timeScale = 0f;
    }
    public void ResetGameTime() {
        Time.timeScale = 1f;
    }
    /// <summary>
    /// Advances to the next level and starts a new round at that harder level.
    /// </summary>
    public void NextLevel() {
        currentLevel++; // Always increment!
        StartNewRoundAtCurrentLevel();
    }

    public void RestartCurrentLevel() {
        StartNewRoundAtCurrentLevel();
    }

    /// <summary>
    /// Sets grid/word difficulty based on currentLevel. 
    /// </summary>
    private void UpdateDifficultySettings() {
        int cappedLevel = Mathf.Min(currentLevel, maxLevel); // Only use cappedLevel here

        int newGridSize = Mathf.Min(4 + (cappedLevel - 1) / 2, 8);
        int newMinWordLength = Mathf.Min(3 + cappedLevel / 3, newGridSize);
        int newMaxWordLength = Mathf.Min(5 + cappedLevel / 2, newGridSize);

        gridManager.gridSize = newGridSize;
        minWordLength = newMinWordLength;
        maxWordLength = newMaxWordLength;
        minWordsToPlace = Mathf.Min(3 + cappedLevel / 2, 8);
        maxWordsToPlace = Mathf.Min(5 + cappedLevel / 2, 10);
    }

}
