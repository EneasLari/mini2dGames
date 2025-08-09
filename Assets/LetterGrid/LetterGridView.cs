using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridView : MonoBehaviour {

    public enum GridFitMode {
        FitToCurrent, // cell size based on current grid size
        FitToMax      // cell size based on maxCols/maxRows
    }

    [Header("Refs")]
    [SerializeField] private Transform gridParent;

    [Header("🎨 Tile Colors (used by initial visuals)")]
    [SerializeField] public Color baseColor = Color.white;
    [SerializeField] public Color correctColor = Color.green;
    [SerializeField] public Color selectedColor = Color.yellow;

    [Header("Layout Settings")]
    [SerializeField] private bool autoUpdateLayoutOnResize = true;
    public int minRows = 4, minCols = 4;
    public int maxRows = 12, maxCols = 12;
    public GridFitMode fitMode = GridFitMode.FitToMax;
    private int GridSizeX = 0;
    private int GridSizeY = 0;
    private RectTransform watchedPanelRect;
    private GridLayoutGroup gridLayout;
    private Vector2 lastSize;


    [Header("🛠 Gameplay Settings")]
    public bool highlightWordTiles = true;
    public Color wordTileColor = new Color(0.5f, 0f, 0.8f, 1f);


    public void BuildGridView(LetterData[,] letterGrid) {
        if (gridParent == null || letterGrid == null) return;
        gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null) { 
            Debug.LogError($"{nameof(GridLayoutGroup)} missing on {gridParent.name}");
            return;
        }

        EnsureWatchedPanel();      // ← bind to gridParent now
        ForceLayoutPass();         // ← ensure rect is up to date

        // establish sizes first
        int rows = letterGrid.GetLength(0);   // rows
        int cols = letterGrid.GetLength(1);   // cols

        GridSizeX = cols;   // columns
        GridSizeY = rows;   // rows
        

        UpdateGridLayout(GridSizeX,GridSizeY);  // now valid
        ClearGridToPool();

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                var tile = LetterTilePool.Instance.GetTile(gridParent);
                var group = tile.GetComponent<CanvasGroup>();
                if (group == null) 
                    group = tile.AddComponent<CanvasGroup>();
                group.alpha = 0f;


                var text = tile.GetComponentInChildren<TMP_Text>();
                if (text != null) text.text = letterGrid[r, c].TileLetter.ToString();

                var letterTile = tile.GetComponent<LetterGridLetterTile>();
                letterTile.LetterData = letterGrid[r, c];
                letterTile.SetCurrentColor(baseColor);

                if (highlightWordTiles && letterGrid[r, c].Flag == LetterData.LetterFlag.InWord)
                    letterTile.SetCurrentColor(wordTileColor);
            }
        }
    }

    private void EnsureWatchedPanel() {
        if (gridParent == null) return;
        var rt = gridParent.GetComponent<RectTransform>();
        if (rt == null) return;

        // Rebind if null or changed
        if (watchedPanelRect != rt) {
            watchedPanelRect = rt;
            lastSize = watchedPanelRect.rect.size;
        }
    }

    private void ForceLayoutPass() {
        if (watchedPanelRect == null) return;
        // Make sure rect.size is current this frame
        LayoutRebuilder.ForceRebuildLayoutImmediate(watchedPanelRect);
        Canvas.ForceUpdateCanvases();
    }

    public void UpdateGridLayout(int currentCols, int currentRows) {
        if (gridLayout == null || watchedPanelRect == null) return;

        // Make sure we have at least 1x1
        currentCols = Mathf.Max(1, currentCols);
        currentRows = Mathf.Max(1, currentRows);

        // Fix column count to match visible grid
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = currentCols;

        float width = watchedPanelRect.rect.width;
        float height = watchedPanelRect.rect.height;

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        // Decide what "target grid" we're fitting to
        int fitCols = (fitMode == GridFitMode.FitToMax) ? maxCols : currentCols;
        int fitRows = (fitMode == GridFitMode.FitToMax) ? maxRows : currentRows;

        // Calculate total spacing for the chosen fit size
        float totalSpacingX = spacingX * Mathf.Max(0, fitCols - 1);
        float totalSpacingY = spacingY * Mathf.Max(0, fitRows - 1);

        // Guard the edge cases so cellSize never goes negative/NaN
        float availW = Mathf.Max(0, width - gridLayout.padding.left - gridLayout.padding.right - totalSpacingX);
        float availH = Mathf.Max(0, height - gridLayout.padding.top - gridLayout.padding.bottom - totalSpacingY);

        int denomX = Mathf.Max(1, fitCols);
        int denomY = Mathf.Max(1, fitRows);

        // Square cells
        float cell = Mathf.Floor(Mathf.Max(1f, Mathf.Min(availW / denomX, availH / denomY)));
        gridLayout.cellSize = new Vector2(cell, cell);

        // 4) Optional: ensure the *current* smaller grid is centered inside the max footprint.
        //    Easiest is to set alignment to MiddleCenter on the GridLayoutGroup in the Inspector.
        //    If you must do it here:
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
    }

    public void ClearGridToPool() {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in gridParent)
            children.Add(child.gameObject);
        foreach (var tile in children)
            LetterTilePool.Instance.ReturnTile(tile);
    }

    public LetterGridLetterTile GetTileAt(int row, int col) {
        if (row < 0 || row >= GridSizeY || col < 0 || col >= GridSizeX) return null;
        int index = row * GridSizeX + col;
        if (index < 0 || index >= gridParent.childCount) return null;
        return gridParent.GetChild(index).GetComponent<LetterGridLetterTile>();
    }

    private void Update() {
        if (!autoUpdateLayoutOnResize || watchedPanelRect == null) return;

        Vector2 currentSize = watchedPanelRect.rect.size;
        if (currentSize != lastSize) {
            lastSize = currentSize;
            if (autoUpdateLayoutOnResize) {
                UpdateGridLayout(GridSizeX, GridSizeY);
            }
        }
    }

}
