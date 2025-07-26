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

    private void Start() {
        StartNewGame();
    }

    public void StartNewGame() {
        isGameActive = true;
        UpdateDifficultySettings();
        wordManager.enabled = true;
        wordManager.ClearTileSelection();
        wordManager.StartWordManager();
        gridManager.StartGridManager();

        StartCoroutine(wordManager.AnimateGridTiles(revealTileVisuals:true,hideTileVisuals:false, bottomToTop: true, leftToRight: false));

        timerManager.StartNewRound();
    }

    public void EndGame(string message) {
        isGameActive = false;
        timerManager.StopTimer();
        wordManager.ClearTileSelection();
        wordManager.enabled = false;
        StartCoroutine(wordManager.ShowLevelMessage(message, 2f, false));
    }

    public void RestartGameWithDelay(float delay = 2f) {
        StartCoroutine(RestartAfterDelay(delay));
    }

    private IEnumerator RestartAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        StartNewGame();
    }

    public void UpdateDifficultySettings() {
        // Example logic: every 2 levels, increase grid size, but max out at 8x8
        int newGridSize = Mathf.Min(4 + (currentLevel - 1) / 2, 8);
        int newMinWordLength = Mathf.Min(3 + currentLevel / 3, newGridSize);
        int newMaxWordLength = Mathf.Min(5 + currentLevel / 2, newGridSize);

        gridManager.gridSize = newGridSize;
        gridManager.minWordLength = newMinWordLength;
        gridManager.maxWordLength = newMaxWordLength;
        gridManager.minWordsToPlace = Mathf.Min(3 + currentLevel / 2, 8);
        gridManager.maxWordsToPlace = Mathf.Min(5 + currentLevel / 2, 10);
    }

    public void NextLevel() {
        currentLevel++;
        if (currentLevel > maxLevel)
            currentLevel = maxLevel;
        UpdateDifficultySettings();
        StartNewGame();
    }


}
