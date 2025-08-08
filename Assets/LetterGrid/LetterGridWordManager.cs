using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class LetterGridWordManager : MonoBehaviour {

    [Header("🔷 UI References")]
    public Canvas gridCanvas;
    public TMP_Text wordDisplayText;
    public TMP_Text scoreDisplayText;
    public TMP_Text levelDisplayText;
    public TMP_Text levelMessageText;
    public GameObject levelMessagePanel;
    public TMP_Text remainingWordsText; // Reference this in the Inspector!
    public Button continueButton;


    [Header("📈 Scoring & Word State")]
    private List<LetterGridLetterTile> selectedTiles = new List<LetterGridLetterTile>();

    [Header("🔒 Runtime State")]
    private int score = 0;
    private string activeWord = "";
    private bool isShowingFeedback = false;
    private bool isUserSelecting = false;
    private Vector2Int wordDirection;
    private bool levelComplete = false;


    public bool IsShowingFeedback => isShowingFeedback;
    public bool IsUserSelecting => isUserSelecting;


    private void Awake() {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueGameBtnPressed);
    }

    private void Start() {

    }

    public void StartWordManager() {
        levelComplete = false;
        continueButton.gameObject.SetActive(false);
        scoreDisplayText.text = $"Score: {score}"; ;
        levelDisplayText.text = $"Level: {LetterGridGameManager.Instance.currentLevel}";
        wordDisplayText.text = "Word: ";
        ClearTileSelection();
        StartCoroutine(AnimateGridTiles(
            revealTileVisuals: true,
            hideTileVisuals: false,
            bottomToTop: true,
            leftToRight: false
        ));
        UpdateRemainingWordsDisplay();
    }


    private void Update() {
        if (isShowingFeedback) return;

        bool mouseReleased = Input.GetMouseButtonUp(0);
        bool touchReleased = (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        if (isUserSelecting && (mouseReleased || touchReleased)) {
            isUserSelecting = false;
            if (activeWord.Length >= 3) ValidateSelectedWord();
            else ClearTileSelection();
        }

        if (isUserSelecting && Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                RaycastTouchToTile(touch.position);
            }
        }

        if (isUserSelecting && Input.GetMouseButton(0)) {
            RaycastTouchToTile(Input.mousePosition);
        }
    }

    private void RaycastTouchToTile(Vector2 screenPosition) {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results) {
            LetterGridLetterTile tile = result.gameObject.GetComponent<LetterGridLetterTile>();
            if (tile != null && !tile.LetterData.IsSelected) {
                TrySelectHoveredTile(tile);
                break;
            }
        }
    }

    public void StartSelection(LetterGridLetterTile firstTile) {
        if (isShowingFeedback) return;
        ClearTileSelection();
        isUserSelecting = true;
        AddTileToWord(firstTile);
        LetterGridGameManager.Instance.gridManager.SmallerTilesTriggerArea();
    }

    public void AddTileToWord(LetterGridLetterTile tile) {
        if (selectedTiles.Contains(tile)) return;

        activeWord += tile.GetComponentInChildren<TMP_Text>().text;
        selectedTiles.Add(tile);
        wordDisplayText.text = "Word: " + activeWord;

        tile.SelectTile();
        tile.SetCurrentColor(LetterGridGameManager.Instance.gridManager.selectedColor);

        if (selectedTiles.Count == 2) {
            Vector2Int firstPos = selectedTiles[0].GetTilePos();
            Vector2Int secondPos = selectedTiles[1].GetTilePos();
            wordDirection = secondPos - firstPos;
            LetterGridGameManager.Instance.gridManager.ResetTilesTriggerArea();
        }
        LetterGridGameAudioEvents.RaiseTileAdded();
    }

    public void TrySelectHoveredTile(LetterGridLetterTile tile) {
        if ((Input.GetMouseButton(0) || Input.touchCount > 0) && isUserSelecting && !isShowingFeedback) {
            if (selectedTiles.Count > 1 && tile == selectedTiles[^2]) {
                UndoLastTile();
                return;
            }

            if (!tile.LetterData.IsSelected && IsTileSelectable(tile)) {
                AddTileToWord(tile);
            }
        }
    }

    public bool IsTileSelectable(LetterGridLetterTile tile) {
        if (selectedTiles.Count == 0) return true;

        Vector2Int newPos = tile.GetTilePos();
        Vector2Int lastPos = selectedTiles[^1].GetTilePos();

        if (selectedTiles.Count > 1) {
            var secondLastTile = selectedTiles[^2];
            if (secondLastTile == tile) return true;
        }

        if (selectedTiles.Count == 1) {
            int dx = Mathf.Abs(newPos.x - lastPos.x);
            int dy = Mathf.Abs(newPos.y - lastPos.y);
            return (dx <= 1 && dy <= 1) && !(dx == 0 && dy == 0);
        }

        return newPos == lastPos + wordDirection;
    }

    public void ValidateSelectedWord() {
        if (isShowingFeedback || levelComplete) return;

        bool isValid = LetterGridGameManager.Instance.gridManager.validWords.Contains(activeWord) && !LetterGridGameManager.Instance.gridManager.foundWords.Contains(activeWord);
        Color flashColor = isValid ? Color.green : Color.red;

        if (isValid) {
            int wordScore = activeWord.Length * 10;
            foreach (var tile in selectedTiles) {
                tile.LetterData.IsFound = true;
                if (tile.LetterData.Type == LetterData.LetterType.DoubleLetter) wordScore += 5;
                else if (tile.LetterData.Type == LetterData.LetterType.TripleWord) wordScore *= 3;
            }
            score += wordScore;
            scoreDisplayText.text = $"Score: {score}";
            LetterGridGameManager.Instance.gridManager.foundWords.Add(activeWord.ToUpper());
            LetterGridGameAudioEvents.RaiseMoveCorrect();

            bool finalWord = AllPlacedWordsFound() && !levelComplete;
            if (finalWord) {
                levelComplete = true;
            }
            StartCoroutine(FlashTilesAndHandleWordResult(flashColor, isValid, finalWord));
            return;
        }
        else {
            LetterGridGameAudioEvents.RaiseMoveWrong();
            StartCoroutine(FlashTilesAndHandleWordResult(flashColor, isValid, false));
        }
    }

    private IEnumerator FlashTilesAndHandleWordResult(Color flashColor, bool isValid, bool isFinalValidWord) {
        yield return StartCoroutine(FlashTilesAndReset(flashColor, isValid));
        if (isValid) {
            UpdateRemainingWordsDisplay();
            if (isFinalValidWord) {
                yield return StartCoroutine(HandleRoundVictory());
            }
        }
    }

    private IEnumerator HandleRoundVictory() {
        LetterGridGameAudioEvents.RaiseLevelSuccess();
        yield return StartCoroutine(AnimateGridTiles(revealTileVisuals: false, hideTileVisuals: true, bottomToTop: false, leftToRight: true));
        yield return StartCoroutine(ShowLevelMessage("Round Complete!", 2f));
    }



    IEnumerator FlashTilesAndReset(Color flashColor, bool isValid) {
        isShowingFeedback = true;
        List<LetterGridLetterTile> tilesToFlash = new List<LetterGridLetterTile>(selectedTiles);

        foreach (var tile in tilesToFlash) {
            Image img = tile.GetComponent<Image>();
            Color original = img.color;
            img.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            img.color = original;
        }

        ClearActiveWord();
        foreach (var tile in selectedTiles) {
            tile.Deselect();
            tile.SetCurrentColor((isValid || tile.LetterData.IsFound) ? LetterGridGameManager.Instance.gridManager.correctColor : LetterGridGameManager.Instance.gridManager.baseColor);
        }
        selectedTiles.Clear();
        isShowingFeedback = false;
    }

    private bool AllPlacedWordsFound() {
        foreach (string placedWord in LetterGridGameManager.Instance.gridManager.placedWords) {
            if (!LetterGridGameManager.Instance.gridManager.foundWords.Contains(placedWord.ToUpper())) {
                return false;
            }
        }
        return true;
    }

    public void UpdateRemainingWordsDisplay() {
        var placedWords = LetterGridGameManager.Instance.gridManager.placedWords;
        var foundWords = LetterGridGameManager.Instance.gridManager.foundWords;

        int remainingCount = 0;
        foreach (var word in placedWords) {
            if (!foundWords.Contains(word.ToUpper()))
                remainingCount++;
        }

        if (remainingWordsText != null) {
            remainingWordsText.text = $"Words left: {remainingCount}";
        }
    }



    public IEnumerator ShowLevelMessage(string message, float duration, bool resetAfterMessage = true) {
        levelMessageText.text = message;
        levelMessagePanel.SetActive(true);

        CanvasGroup group = levelMessagePanel.GetComponent<CanvasGroup>();
        RectTransform rectTransform = levelMessagePanel.GetComponent<RectTransform>();

        // Fade in
        yield return StartCoroutine(FadeLevelMessagePanel(true, group, rectTransform));

        yield return new WaitForSeconds(duration);

        // Fade out
        yield return StartCoroutine(FadeLevelMessagePanel(false, group, rectTransform));

        levelMessagePanel.SetActive(false);

        if (resetAfterMessage) {
            LetterGridGameManager.Instance.NextLevel();
        }
    }
    public void OnContinueGameBtnPressed() {
        StartCoroutine(FadeOutAndContinue());
    }

    private IEnumerator FadeOutAndContinue() {
        CanvasGroup group = levelMessagePanel.GetComponent<CanvasGroup>();
        RectTransform rectTransform = levelMessagePanel.GetComponent<RectTransform>();

        // Fade out
        yield return StartCoroutine(FadeLevelMessagePanel(false, group, rectTransform));
        levelMessagePanel.SetActive(false);
        continueButton.gameObject.SetActive(false);

        LetterGridGameManager.Instance.RestartCurrentLevel();
    }
    public void ShowLevelMessageUntilContinue(string message) {
        continueButton.gameObject.SetActive(true);
        StartCoroutine(ShowLevelMessageUntilContinueRoutine(message));
    }

    private IEnumerator ShowLevelMessageUntilContinueRoutine(string message) {
        levelMessageText.text = message;
        levelMessagePanel.SetActive(true);

        CanvasGroup group = levelMessagePanel.GetComponent<CanvasGroup>();
        RectTransform rectTransform = levelMessagePanel.GetComponent<RectTransform>();

        // Fade in
        yield return StartCoroutine(FadeLevelMessagePanel(true, group, rectTransform));

        // Don't fade out or deactivate panel here — wait for player to press Continue.
    }

    // Shared fade animation
    private IEnumerator FadeLevelMessagePanel(bool fadeIn, CanvasGroup group, RectTransform rectTransform) {
        if (group == null || rectTransform == null)
            yield break;

        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        Vector3 startScale = fadeIn ? Vector3.one * 0.5f : Vector3.one;
        Vector3 endScale = fadeIn ? Vector3.one : Vector3.one * 0.5f;

        rectTransform.localScale = startScale;
        group.alpha = startAlpha;

        float t = 0f;
        while (t < 1f) {
            float eased = Mathf.SmoothStep(0f, 1f, t);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, eased);
            t += Time.deltaTime * 2f;
            yield return null;
        }

        group.alpha = endAlpha;
        rectTransform.localScale = endScale;
    }


    public IEnumerator AnimateGridTiles(
    bool revealTileVisuals= false,
    bool hideTileVisuals=false,
    bool bottomToTop = true,
    bool leftToRight = true,
    float tileDelay = 0.1f,
    float punchScale = 1.1f,
    float punchDuration = 0.2f) {
        var grid = LetterGridGameManager.Instance.gridManager;
        int size = grid.gridSize;

        int rowStart = bottomToTop ? size - 1 : 0;
        int rowEnd = bottomToTop ? -1 : size;
        int rowStep = bottomToTop ? -1 : 1;

        int colStart = leftToRight ? 0 : size - 1;
        int colEnd = leftToRight ? size : -1;
        int colStep = leftToRight ? 1 : -1;

        for (int row = rowStart; row != rowEnd; row += rowStep) {
            for (int col = colStart; col != colEnd; col += colStep) {
                var tile = grid.GetTileAt(row, col);
                if (tile != null) {
                    // Reveal tile visuals
                    CanvasGroup group = tile.GetComponent<CanvasGroup>();
                    if (group != null && revealTileVisuals) group.alpha = 1f;
                    else if (group != null && hideTileVisuals) group.alpha = 0f;
                    LetterGridGameAudioEvents.RaiseTileFlip();
                    RectTransform rt = tile.GetComponent<RectTransform>();
                    StartCoroutine(PunchScale(rt, punchScale, punchDuration));
                    yield return new WaitForSeconds(tileDelay);
                }
            }
        }
    }


    private IEnumerator PunchScale(RectTransform target, float punchScale, float duration) {
        Vector3 original = target.localScale;
        float t = 0f;

        while (t < duration) {
            float scale = Mathf.Lerp(punchScale, 1f, t / duration);
            target.localScale = original * scale;
            t += Time.deltaTime;
            yield return null;
        }

        target.localScale = original;
    }




    private void UndoLastTile() {
        if (selectedTiles.Count < 2) return;

        LetterGridLetterTile lastTile = selectedTiles[^1];
        selectedTiles.RemoveAt(selectedTiles.Count - 1);
        activeWord = activeWord.Substring(0, activeWord.Length - 1);
        wordDisplayText.text = "Word: " + activeWord;

        lastTile.Deselect();
        lastTile.SetCurrentColor(lastTile.LetterData.IsFound ? LetterGridGameManager.Instance.gridManager.correctColor : LetterGridGameManager.Instance.gridManager.baseColor);

        if (selectedTiles.Count == 1) {
            ResetDirection();
        }
    }

    private void ResetDirection() {
        wordDirection = Vector2Int.zero;
        LetterGridGameManager.Instance.gridManager.SmallerTilesTriggerArea();
    }

    public void ClearTileSelection() {
        if (isShowingFeedback) return;

        ClearActiveWord();
        foreach (var tile in selectedTiles) {
            tile.Deselect();
            tile.SetCurrentColor(tile.LetterData.IsFound ? LetterGridGameManager.Instance.gridManager.correctColor : LetterGridGameManager.Instance.gridManager.baseColor);
        }
        selectedTiles.Clear();
        isUserSelecting = false;
        ResetDirection();
        LetterGridGameManager.Instance.gridManager.ResetTilesTriggerArea();
    }

    public void ClearActiveWord() {
        activeWord = "";
        wordDisplayText.text = "Word: ";
    }

}
