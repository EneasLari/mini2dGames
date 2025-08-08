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

    [Header("🛠 Gameplay Settings")]
    public bool highlightWordTiles = true;
    public Color wordTileColor = new Color(0.5f, 0f, 0.8f, 1f);

    private int GridSizeX = 0;
    private int GridSizeY = 0;

    private void OnRectTransformDimensionsChange() {
        if (autoUpdateLayoutOnResize) UpdateGridLayout(GridSizeX, GridSizeY);
    }

    public void BuildGridView(LetterData[,] letterGrid) {
        if (gridParent == null || letterGrid == null) return;

        // establish sizes first
        int rows = letterGrid.GetLength(0);   // rows
        int cols = letterGrid.GetLength(1);   // cols

        GridSizeX = cols;   // columns
        GridSizeY = rows;   // rows
        

        UpdateGridLayout(GridSizeX, GridSizeY);  // now valid
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


    public void UpdateGridLayout(int cols, int rows) {
        var gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        var panelRect = gridParent.parent.GetComponent<RectTransform>();

        cols = Mathf.Max(1, cols);
        rows = Mathf.Max(1, rows);

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cols;

        float width = panelRect.rect.width;
        float height = panelRect.rect.height;

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        float totalSpacingX = spacingX * (cols - 1);
        float totalSpacingY = spacingY * (rows - 1);

        float availableWidth = width - gridLayout.padding.left - gridLayout.padding.right - totalSpacingX;
        float availableHeight = height - gridLayout.padding.top - gridLayout.padding.bottom - totalSpacingY;

        float cellW = availableWidth / cols;
        float cellH = availableHeight / rows;
        float cell = Mathf.Min(cellW, cellH);

        gridLayout.cellSize = new Vector2(cell, cell);
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


}
