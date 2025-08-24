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


    [Header("✏️ Simple Selection Line")]
    [SerializeField] private Sprite selectionLineSprite; // assign ONLY this in Inspector
    [SerializeField] private float lineThickness = 8f;   // optional tweak
    [SerializeField] private Color lineColor = Color.white; // optional


    [Header("📈 Scoring & Word State")]
    private List<LetterGridLetterTile> selectedTiles = new List<LetterGridLetterTile>();

    [Header("🔒 Runtime State")]
    private int score = 0;
    private string activeWord = "";
    private bool isShowingFeedback = false;
    private bool isUserSelecting = false;
    private Vector2Int wordDirection;
    private bool levelComplete = false;

    private Image _currentLine;                // the line for the active selection
    private readonly List<Image> _finalLines = new(); // all finalized lines (valid words)

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
        wordDisplayText.text = "";//"Word: ";
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
        SmallerTilesTriggerArea();
        UpdateCurrentLine();
    }

    public void AddTileToWord(LetterGridLetterTile tile) {
        if (selectedTiles.Contains(tile)) return;

        activeWord += tile.GetComponentInChildren<TMP_Text>().text;
        selectedTiles.Add(tile);
        wordDisplayText.text = activeWord;//"Word: " + activeWord;

        tile.SelectTile();
        tile.SetCurrentColor(LetterGridGameManager.Instance.gridView.selectedColor);

        if (selectedTiles.Count == 2) {
            Vector2Int firstPos = selectedTiles[0].GetTilePos();
            Vector2Int secondPos = selectedTiles[1].GetTilePos();
            wordDirection = secondPos - firstPos;
            ResetTilesTriggerArea();
        }
        LetterGridGameAudioEvents.RaiseTileAdded();
        UpdateCurrentLine();
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

    private Image CreateLineUnder(Transform gridParent) {
        Image lineUnder; // created on demand
        if (selectionLineSprite == null)
            return null;

        var go = new GameObject("SelectionLine", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(0f, lineThickness);

        lineUnder = go.AddComponent<Image>();
        lineUnder.sprite = selectionLineSprite;
        lineUnder.type = Image.Type.Simple;    // keep it simple
        lineUnder.color = lineColor;
        lineUnder.raycastTarget = false;
        lineUnder.enabled = false;

        rt.SetParent(gridParent.parent, false);
        go.transform.SetSiblingIndex(gridParent.GetSiblingIndex()); // before grid

        return lineUnder;
    }

    private void UpdateCurrentLine() {
        // Need at least two tiles to have a length
        if (selectedTiles == null || selectedTiles.Count < 2) {
            if (_currentLine != null) _currentLine.enabled = false;
            return;
        }

        Transform tilesParent = selectedTiles[0].transform.parent;//gridParent
        if (_currentLine == null)
            _currentLine = CreateLineUnder(tilesParent);

        var lineRT = _currentLine.rectTransform;
        var containerRT = (RectTransform)_currentLine.rectTransform.parent; // same parent as gridParent.parent

        var parentCanvas = containerRT.GetComponentInParent<Canvas>();
        Camera uiCam = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? parentCanvas.worldCamera
            : null;

        RectTransform aRT = selectedTiles[0].GetComponent<RectTransform>();
        RectTransform bRT = selectedTiles[^1].GetComponent<RectTransform>();

        Vector2 sa = RectTransformUtility.WorldToScreenPoint(uiCam, aRT.position);
        Vector2 sb = RectTransformUtility.WorldToScreenPoint(uiCam, bRT.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRT, sa, uiCam, out var la);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRT, sb, uiCam, out var lb);

        Vector2 dir = lb - la;
        float length = dir.magnitude;
        if (length < 0.01f) { _currentLine.enabled = false; return; }

        Vector2 mid = (la + lb) * 0.5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRT.anchoredPosition = mid;
        lineRT.localRotation = Quaternion.Euler(0, 0, angle);
        lineRT.sizeDelta = new Vector2(length, lineThickness);
        _currentLine.enabled = true;
    }


    // Commit the current line if valid; otherwise discard it.
    private void FinalizeOrDiscardCurrentLine(bool isValid) {
        if (_currentLine == null) return;

        if (isValid) {
            _currentLine.enabled = true;
            _finalLines.Add(_currentLine);
            print(_finalLines.Count);
            _currentLine = null; // next selection will create a NEW line
        } else {
            Destroy(_currentLine.gameObject); // wrong word => remove preview line
            _currentLine = null;
        }
    }

    private void DestroyAllLines() {
        if (_currentLine != null) {
            Destroy(_currentLine.gameObject);
            _currentLine = null;
        }
        for (int i = 0; i < _finalLines.Count; i++) {
            if (_finalLines[i] != null) Destroy(_finalLines[i].gameObject);
        }
        _finalLines.Clear();
    }

    public void ValidateSelectedWord() {
        if (isShowingFeedback || levelComplete) return;

        var gridManager = LetterGridGameManager.Instance.gridManager;
        string wordToValidate = activeWord.ToUpperInvariant();

        bool isTarget = gridManager.placedWords.Contains(wordToValidate);
        bool already = gridManager.foundWords.Contains(wordToValidate);

        // strict targets-only
        bool isValid = isTarget && !already;

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
        }
        else {
            LetterGridGameAudioEvents.RaiseMoveWrong();
        }
        bool finalWord = AllPlacedWordsFound() && !levelComplete;
        if (finalWord) {
            levelComplete = true;
        }
        // ✅ keep a new line if valid; ❌ remove the preview line if invalid
        FinalizeOrDiscardCurrentLine(isValid);
        StartCoroutine(FlashTilesAndHandleWordResult(flashColor, isValid, finalWord));
    }

    private IEnumerator FlashTilesAndHandleWordResult(Color flashColor, bool isValid, bool isFinalValidWord) {
        yield return StartCoroutine(FlashTilesAndReset(flashColor, isValid));
        if (isValid) {
            UpdateRemainingWordsDisplay();
            if (isFinalValidWord) {
                DestroyAllLines();
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
            tile.SetCurrentColor((isValid || tile.LetterData.IsFound) ? LetterGridGameManager.Instance.gridView.correctColor : LetterGridGameManager.Instance.gridView.baseColor);
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
        var gridView = LetterGridGameManager.Instance.gridView;
        int sizeX = grid.GridSizeX;
        int sizeY = grid.GridSizeY;

        int rowStart = bottomToTop ? sizeX - 1 : 0;
        int rowEnd = bottomToTop ? -1 : sizeX;
        int rowStep = bottomToTop ? -1 : 1;

        int colStart = leftToRight ? 0 : sizeY - 1;
        int colEnd = leftToRight ? sizeY : -1;
        int colStep = leftToRight ? 1 : -1;

        for (int row = rowStart; row != rowEnd; row += rowStep) {
            for (int col = colStart; col != colEnd; col += colStep) {
                var tile = gridView.GetTileAt(row, col);
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
        wordDisplayText.text = activeWord;//"Word: " + activeWord;

        lastTile.Deselect();
        lastTile.SetCurrentColor(lastTile.LetterData.IsFound ? LetterGridGameManager.Instance.gridView.correctColor : LetterGridGameManager.Instance.gridView.baseColor);

        if (selectedTiles.Count == 1) {
            ResetDirection();
        }
        UpdateCurrentLine();
    }

    private void ResetDirection() {
        wordDirection = Vector2Int.zero;
        SmallerTilesTriggerArea();
    }

    public void ClearTileSelection() {
        if (isShowingFeedback) return;

        ClearActiveWord();
        foreach (var tile in selectedTiles) {
            tile.Deselect();
            tile.SetCurrentColor(tile.LetterData.IsFound ? LetterGridGameManager.Instance.gridView.correctColor : LetterGridGameManager.Instance.gridView.baseColor);
        }
        selectedTiles.Clear();
        isUserSelecting = false;
        ResetDirection();
        ResetTilesTriggerArea();

    }

    public void ResetTilesTriggerArea() {
        var gridView = LetterGridGameManager.Instance.gridView;
        var grid = LetterGridGameManager.Instance.gridManager;
        for (int i = 0; i < grid.GridSizeX; i++) {
            for (int j = 0; j < grid.GridSizeY; j++) {
                var tile = gridView.GetTileAt(i, j);
                if (tile != null) 
                    tile.ResetTriggerAreaPercentage();
            }
        }
    }

    public void SmallerTilesTriggerArea() {
        var gridView = LetterGridGameManager.Instance.gridView;
        var grid = LetterGridGameManager.Instance.gridManager;
        for (int i = 0; i < grid.GridSizeX; i++) {
            for (int j = 0; j < grid.GridSizeY; j++) {
                var tile = gridView.GetTileAt(i, j);
                if (tile != null)
                    tile.SetSmallerTriggerAreaPercentage();
            }
        }
    }

    public void ClearActiveWord() {
        activeWord = "";
        wordDisplayText.text = "";//"Word: ";
    }

}
