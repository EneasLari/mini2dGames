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
}
