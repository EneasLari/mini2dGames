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
    [SerializeField] public LetterGridView gridView;
    [SerializeField] public LetterGridTimerManager timerManager;

    [Header("🕹 Game State")]
    public bool isGameActive { get; private set; } = false;

    [Header("🔼 Difficulty Scaling")]
    public int currentLevel { get; private set; } = 1;
    public int maxLevel = 10;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            // If you want it persistent across scenes, uncomment:
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) {
            Destroy(gameObject);
            return;
        }
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
        var grid=gridManager.SetupGrid();
        gridView.BuildGridView(grid);
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
        int L = Mathf.Min(currentLevel, maxLevel);

        // --- Tunables (easy to tweak) ---
        const int minRows = 4, minCols = 4;
        const int maxRows = 12, maxCols = 12;

        // how fast each dimension grows with level
        const int levelsPerRowStep = 2;  // every 2 levels, +1 row
        const int levelsPerColStep = 3;  // every 3 levels, +1 col

        // word length scaling
        const int baseMinWordLen = 3;    // starts at 3
        const int baseMaxWordLen = 5;    // starts at 5
        const int levelsPerMinLenStep = 3;
        const int levelsPerMaxLenStep = 2;

        // words-to-place scaling
        const int baseMinWords = 3;
        const int baseMaxWords = 5;
        const int maxMinWords = 10;
        const int maxMaxWords = 14;

        // --- Compute grid size ---
        int newRows = Mathf.Clamp(minRows + (L - 1) / levelsPerRowStep, minRows, maxRows);
        int newCols = Mathf.Clamp(minCols + (L - 1) / levelsPerColStep, minCols, maxCols);

        // --- Word lengths (cap by the shorter side) ---
        int boardMin = Mathf.Min(newRows, newCols);
        int newMinWordLength = Mathf.Clamp(baseMinWordLen + L / levelsPerMinLenStep, baseMinWordLen, boardMin);
        int newMaxWordLength = Mathf.Clamp(baseMaxWordLen + L / levelsPerMaxLenStep, newMinWordLength, boardMin);

        // --- Words to place ---
        int newMinWordsToPlace = Mathf.Clamp(baseMinWords + L / 2, baseMinWords, maxMinWords);
        int newMaxWordsToPlace = Mathf.Clamp(baseMaxWords + L / 2, newMinWordsToPlace, maxMaxWords);

        // --- Apply ---
        gridManager.GridSizeX = newCols;
        gridManager.GridSizeY = newRows;
        minWordLength = newMinWordLength;
        maxWordLength = newMaxWordLength;
        minWordsToPlace = newMinWordsToPlace;
        maxWordsToPlace = newMaxWordsToPlace;

        Debug.Log($"[Difficulty] L{currentLevel}: {newRows}x{newCols}, words {minWordsToPlace}-{maxWordsToPlace}, len {minWordLength}-{maxWordLength}");
    }


}
