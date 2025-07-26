using UnityEngine;
using System.Collections;

public class LetterGridGameManager : MonoBehaviour {
    public static LetterGridGameManager Instance { get; private set; }

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
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        wordManager.enabled = true;
        wordManager.ClearTileSelection();
        wordManager.StartWordManager();
        gridManager.StartGridManager();

        StartCoroutine(wordManager.AnimateGridTiles(
            revealTileVisuals: true,
            hideTileVisuals: false,
            bottomToTop: true,
            leftToRight: false
        ));

        timerManager.StartNewRound();
    }

    /// <summary>
    /// Call this when a round is failed or ended without level-up.
    /// Resets only the grid at current level.
    /// </summary>
    public void EndGame(string message) {
        isGameActive = false;
        timerManager.StopTimer();
        wordManager.ClearTileSelection();
        wordManager.enabled = false;
        StartCoroutine(wordManager.ShowLevelMessage(message, 2f, false));
    }

    /// <summary>
    /// Advances to the next level and starts a new round at that harder level.
    /// </summary>
    public void NextLevel() {
        currentLevel++; // Always increment!
        UpdateDifficultySettings(); // This clamps actual gameplay difficulty, see below
        StartNewRoundAtCurrentLevel();
    }


    public void RestartGameWithDelay(float delay = 2f) {
        StartCoroutine(RestartAfterDelay(delay));
    }

    private IEnumerator RestartAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        StartNewRoundAtCurrentLevel();
    }

    /// <summary>
    /// Sets grid/word difficulty based on currentLevel. 
    /// </summary>
    public void UpdateDifficultySettings() {
        int cappedLevel = Mathf.Min(currentLevel, maxLevel); // Only use cappedLevel here

        int newGridSize = Mathf.Min(4 + (cappedLevel - 1) / 2, 8);
        int newMinWordLength = Mathf.Min(3 + cappedLevel / 3, newGridSize);
        int newMaxWordLength = Mathf.Min(5 + cappedLevel / 2, newGridSize);

        gridManager.gridSize = newGridSize;
        gridManager.minWordLength = newMinWordLength;
        gridManager.maxWordLength = newMaxWordLength;
        gridManager.minWordsToPlace = Mathf.Min(3 + cappedLevel / 2, 8);
        gridManager.maxWordsToPlace = Mathf.Min(5 + cappedLevel / 2, 10);
    }

}
