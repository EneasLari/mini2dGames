using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager2048 : MonoBehaviour {
    public int gridSize = 4;
    public Tile2048[,] grid;
    public GameObject tilePrefab;
    public Transform gridParent;
    public TMP_Text gameOverText;
    public TMP_Text scoreText; //  Added Score UI

    private bool isGameOver = false;
    private int score = 0; //  Score tracking

    void Start() {
        grid = new Tile2048[gridSize, gridSize];
        GridManager2048.Instance.GenerateGrid();
        SpawnTile();
        SpawnTile();
        UpdateScoreUI();
    }

    void SpawnTile() {
        if (isGameOver) return;

        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                if (grid[x, y] == null) {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }

        if (emptyCells.Count > 0) {
            Vector2Int spawnPos = emptyCells[Random.Range(0, emptyCells.Count)];
            GameObject newTile = Instantiate(tilePrefab, gridParent);
            Tile2048 tile = newTile.GetComponent<Tile2048>();
            tile.Initialize(spawnPos.x, spawnPos.y, Random.value < 0.9f ? 2 : 4);

            //  Set Correct Position Using GridManager2048
            Transform targetCell = GridManager2048.Instance.GetCellAt(spawnPos.x, spawnPos.y);
            newTile.transform.position = targetCell.position;

            //  Assign to the grid
            grid[spawnPos.x, spawnPos.y] = tile;
        } else if (!CanMove()) {
            GameOver();
        }
    }


    public IEnumerator MoveTiles(Vector2Int direction) {
        if (isGameOver) yield break;

        bool moved = false;
        bool[,] merged = new bool[gridSize, gridSize]; // Prevent multiple merges in one move
        List<Tile2048> movingTiles = new List<Tile2048>(); // Store moving tiles

        // ✅ Correct Loop Order Based on Movement Direction
        int startX = (direction.x > 0) ? gridSize - 1 : 0;
        int startY = (direction.y > 0) ? gridSize - 1 : 0;
        int stepX = (direction.x != 0) ? -direction.x : 1;
        int stepY = (direction.y != 0) ? -direction.y : 1;

        // 🔥 Correct Order for UP/DOWN Movements
        if (direction.y < 0) { startY = 0; stepY = 1; } // ✅ UP: Start at TOP (Y=0) go down
        if (direction.y > 0) { startY = gridSize - 1; stepY = -1; } // ✅ DOWN: Start at BOTTOM (Y=3) go up

        // ✅ Move all tiles first
        for (int y = startY; y >= 0 && y < gridSize; y += stepY) {
            for (int x = startX; x >= 0 && x < gridSize; x += stepX) {
                Tile2048 currentTile = grid[x, y];

                if (currentTile != null) {
                    Vector2Int newPos = new Vector2Int(x, y);
                    Vector2Int nextPos = newPos + direction;

                    while (IsWithinBounds(nextPos) && grid[nextPos.x, nextPos.y] == null) {
                        grid[nextPos.x, nextPos.y] = currentTile;
                        grid[newPos.x, newPos.y] = null;
                        newPos = nextPos;
                        nextPos = newPos + direction;
                        moved = true;
                    }

                    // ✅ Update logic position
                    currentTile.SetGridPosition(newPos.x, newPos.y);

                    // ✅ Add to movingTiles list
                    if (newPos != new Vector2Int(x, y)) {
                        movingTiles.Add(currentTile);
                    }
                }
            }
        }

        // ✅ Merge while tiles are still moving
        for (int y = startY; y >= 0 && y < gridSize; y += stepY) {
            for (int x = startX; x >= 0 && x < gridSize; x += stepX) {
                Tile2048 currentTile = grid[x, y];

                if (currentTile != null) {
                    Vector2Int nextPos = new Vector2Int(x, y) + direction;

                    if (IsWithinBounds(nextPos) && grid[nextPos.x, nextPos.y] != null &&
                        grid[nextPos.x, nextPos.y].value == currentTile.value && !merged[nextPos.x, nextPos.y]) {
                        // ✅ Merge tiles
                        grid[nextPos.x, nextPos.y].SetValue(grid[nextPos.x, nextPos.y].value * 2);
                        score += grid[nextPos.x, nextPos.y].value;

                        // ✅ Destroy old tile AFTER animation
                        StartCoroutine(DestroyAfterAnimation(currentTile, GridManager2048.Instance.GetCellAt(nextPos.x, nextPos.y).position));

                        grid[x, y] = null;
                        merged[nextPos.x, nextPos.y] = true;
                        moved = true;
                    }
                }
            }
        }

        // ✅ Move all tiles together
        List<Coroutine> animations = new List<Coroutine>();
        foreach (Tile2048 tile in movingTiles) {
            animations.Add(StartCoroutine(tile.MoveAnimation(GridManager2048.Instance.GetCellAt(tile.position.x, tile.position.y).position)));
        }

        // ✅ Wait for all animations to finish
        foreach (Coroutine animation in animations) {
            yield return animation;
        }

        if (moved) {
            yield return new WaitForSeconds(0.05f);
            SpawnTile();
            UpdateScoreUI();
        } else if (!CanMove()) {
            GameOver();
        }
    }






    private IEnumerator DestroyAfterAnimation(Tile2048 tile, Vector3 targetPosition) {
        // ✅ Wait for the tile to reach its final position
        yield return StartCoroutine(tile.MoveAnimation(targetPosition));

        // ✅ Now it's safe to destroy the tile
        Destroy(tile.gameObject);
    }




    bool IsWithinBounds(Vector2Int pos) {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }

    bool CanMove() {
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                if (grid[x, y] == null) return true;

                if (IsWithinBounds(new Vector2Int(x + 1, y)) && grid[x + 1, y].value == grid[x, y].value) return true;
                if (IsWithinBounds(new Vector2Int(x, y + 1)) && grid[x, y + 1].value == grid[x, y].value) return true;
            }
        }
        return false;
    }

    void GameOver() {
        isGameOver = true;
        gameOverText.gameObject.SetActive(true);
        Debug.Log("Game Over!");
    }

    // Update the Score UI
    void UpdateScoreUI() {
        scoreText.text = "Score: " + score;
    }
}
