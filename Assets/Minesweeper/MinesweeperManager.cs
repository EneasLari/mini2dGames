using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinesweeperManager : MonoBehaviour {
    public int width = 10;
    public int height = 10;
    public int mineCount = 10;
    public Tile tilePrefab;
    public Transform gridParent;
    private GridLayoutGroup gridLayout;

    private Tile[,] grid;

    void Start() {
        gridLayout = gridParent.GetComponent<GridLayoutGroup>();  
        GenerateGrid();
        AdjustTileSize();
        PlaceMines();
        CalculateNumbers();
    }

    void AdjustTileSize() {
        gridLayout = gridParent.GetComponent<GridLayoutGroup>();

        // Set the constraint to Fixed Column Count
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = width; // Set column count to grid width

        // Set spacing
        Vector2 spacing = new Vector2(5f, 5f); // Adjust this value as needed
        gridLayout.spacing = spacing;

        // Get the available size of the grid
        RectTransform gridRect = gridParent.GetComponent<RectTransform>();
        float gridWidth = gridRect.rect.width;
        float gridHeight = gridRect.rect.height;

        // Adjust available space by subtracting spacing
        float adjustedWidth = gridWidth - (spacing.x * (width - 1));
        float adjustedHeight = gridHeight - (spacing.y * (height - 1));

        // Calculate optimal tile size
        float cellSize = Mathf.Min(adjustedWidth / width, adjustedHeight / height);
        gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // ✅ After adjusting tile size, update font size for each tile
        UpdateFontSizes(cellSize);
    }

    void UpdateFontSizes(float tileSize) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (grid[x, y] != null) // ✅ Check if the tile exists
                {
                    grid[x, y].AdjustFontSize(tileSize); // ✅ Adjust font size
                }
            }
        }
    }




    void GenerateGrid() {
        grid = new Tile[width, height]; // ✅ Initialize the 2D array
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Tile newTile = Instantiate(tilePrefab, gridParent);
                newTile.name = $"Tile {x},{y}";
                grid[x, y] = newTile; // ✅ Store the tile in the grid
            }
        }
    }


    void PlaceMines() {
        List<Tile> allTiles = new List<Tile>();

        foreach (var tile in grid)
            allTiles.Add(tile);

        for (int i = 0; i < mineCount; i++) {
            int index = Random.Range(0, allTiles.Count);
            allTiles[index].SetMine();
            allTiles.RemoveAt(index);
        }
    }

    void CalculateNumbers() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (grid[x, y].isMine) continue;

                int mines = CountAdjacentMines(x, y);
                grid[x, y].SetAdjacentMines(mines);
            }
        }
    }

    int CountAdjacentMines(int x, int y) {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height) {
                    if (grid[nx, ny].isMine) count++;
                }
            }
        }
        return count;
    }

    public void RevealAdjacentTiles(Tile tile) {
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(tile);

        while (queue.Count > 0) {
            Tile current = queue.Dequeue();
            int x = (int)current.transform.position.x;
            int y = (int)current.transform.position.y;

            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && ny >= 0 && nx < width && ny < height) {
                        Tile neighbor = grid[nx, ny];
                        if (!neighbor.isMine && !neighbor.IsRevealed()) {
                            neighbor.RevealTile();
                            if (neighbor.adjacentMines == 0)
                                queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }
    }

    public void GameOver() {
        foreach (var tile in grid) {
            tile.RevealTile();
        }
        Debug.Log("Game Over!");
    }
}
