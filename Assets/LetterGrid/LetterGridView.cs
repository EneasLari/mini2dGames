using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridView : MonoBehaviour {
    [Header("Refs")]
    [SerializeField] private Transform gridParent;

    [Header("🎨 Tile Colors (used by initial visuals)")]
    [SerializeField] public Color baseColor = Color.white;
    [SerializeField] public Color correctColor = Color.green;
    [SerializeField] public Color selectedColor = Color.yellow;

    [Header("Layout")]
    [SerializeField] private bool autoUpdateLayoutOnResize = true;
    // --- Tunables (easy to tweak) ---
    public  int minRows = 4, minCols = 4;
    public  int maxRows = 12, maxCols = 12;
    private int GridSizeX = 0;
    private int GridSizeY = 0;
    private RectTransform watchedPanelRect;
    private Vector2 lastSize;


    [Header("🛠 Gameplay Settings")]
    public bool highlightWordTiles = true;
    public Color wordTileColor = new Color(0.5f, 0f, 0.8f, 1f);




    //private void OnRectTransformDimensionsChange() {
    //    print("view changed");
    //    if (autoUpdateLayoutOnResize) UpdateGridLayout(GridSizeX, GridSizeY);
    //}

    public void BuildGridView(LetterData[,] letterGrid) {
        if (gridParent == null || letterGrid == null) return;

        EnsureWatchedPanel();      // ← bind to gridParent now
        ForceLayoutPass();         // ← ensure rect is up to date

        // establish sizes first
        int rows = letterGrid.GetLength(0);   // rows
        int cols = letterGrid.GetLength(1);   // cols

        GridSizeX = cols;   // columns
        GridSizeY = rows;   // rows
        

        UpdateGridLayout(maxCols, maxRows,GridSizeX,GridSizeY);  // now valid
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

    public void UpdateGridLayout(int maxCols, int maxRows, int currenCols,int currentRows) {
        var gridLayout = gridParent.GetComponent<GridLayoutGroup>();

        currenCols = Mathf.Max(1, currenCols);
        currentRows = Mathf.Max(1, currentRows);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = currenCols;

        float width = watchedPanelRect.rect.width;
        float height = watchedPanelRect.rect.height;

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        float totalSpacingX = spacingX * (currenCols - 1);
        float totalSpacingY = spacingY * (currentRows - 1);

        float availableWidth = width - gridLayout.padding.left - gridLayout.padding.right - totalSpacingX;
        float availableHeight = height - gridLayout.padding.top - gridLayout.padding.bottom - totalSpacingY;

        float cellW = availableWidth / maxCols;
        float cellH = availableHeight / maxRows;
        float cell = Mathf.Min(cellW, cellH);

        gridLayout.cellSize = new Vector2(cell, cell);
        print("Cell Size:"+ gridLayout.cellSize);
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
                print("view changed and grid Updated");
                UpdateGridLayout(maxCols, maxRows, GridSizeX, GridSizeY);
            }
        }
    }

}
