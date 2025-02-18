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
        bool[,] merged = new bool[gridSize, gridSize]; // Track merged tiles
        List<Coroutine> animations = new List<Coroutine>(); // Track animations

        // ✅ Step 1: Move Tiles (No Merging Yet)
        moved |= MoveTilesWithoutMerging(direction, animations);

        // ✅ Wait for movement animations before merging
        yield return WaitForAnimations(animations);
        animations.Clear();

        // ✅ Step 2: Merge Tiles
        moved |= MergeTiles(direction, merged);

        // ✅ Wait for merge animations
        yield return WaitForAnimations(animations);
        animations.Clear();

        // ✅ Step 3: Move Tiles Again After Merging
        moved |= MoveTilesWithoutMerging(direction, animations);

        // ✅ Wait for final movement animations
        yield return WaitForAnimations(animations);

        // ✅ Step 4: Spawn a New Tile if Movement or Merging Occurred
        if (moved) {
            SpawnTile();
            UpdateScoreUI();
        } else if (!CanMove()) {
            GameOver();
        }
    }


    private bool MoveTilesWithoutMerging(Vector2Int direction, List<Coroutine> animations) {
        bool moved = false;

        int startX = (direction.x > 0) ? gridSize - 1 : 0;
        int startY = (direction.y > 0) ? gridSize - 1 : 0;
        int stepX = (direction.x != 0) ? -direction.x : 1;
        int stepY = (direction.y != 0) ? -direction.y : 1;

        for (int y = startY; y >= 0 && y < gridSize; y += stepY) {
            for (int x = startX; x >= 0 && x < gridSize; x += stepX) {
                Tile2048 currentTile = grid[x, y];
                if (currentTile == null) continue;

                Vector2Int newPos = new Vector2Int(x, y);
                Vector2Int nextPos = newPos + new Vector2Int(direction.x, -direction.y);

                while (IsWithinBounds(nextPos) && grid[nextPos.x, nextPos.y] == null) {
                    grid[nextPos.x, nextPos.y] = currentTile;
                    grid[newPos.x, newPos.y] = null;
                    newPos = nextPos;
                    nextPos = newPos + new Vector2Int(direction.x, -direction.y);
                    moved = true;
                }

                if (newPos != new Vector2Int(x, y)) {
                    animations.Add(StartCoroutine(currentTile.MoveAnimation(GridManager2048.Instance.GetCellAt(newPos.x, newPos.y).position)));
                }

                currentTile.SetGridPosition(newPos.x, newPos.y);
            }
        }

        return moved;
    }

    private bool MergeTiles(Vector2Int direction, bool[,] merged) {
        bool moved = false;

        int startX = (direction.x > 0) ? gridSize - 1 : 0;
        int startY = (direction.y > 0) ? gridSize - 1 : 0;
        int stepX = (direction.x != 0) ? -direction.x : 1;
        int stepY = (direction.y != 0) ? -direction.y : 1;

        for (int y = startY; y >= 0 && y < gridSize; y += stepY) {
            for (int x = startX; x >= 0 && x < gridSize; x += stepX) {
                Tile2048 currentTile = grid[x, y];
                if (currentTile == null) continue;

                Vector2Int newPos = new Vector2Int(x, y);
                Vector2Int nextPos = newPos + new Vector2Int(direction.x, -direction.y);

                // Find the furthest valid position before merging
                while (IsWithinBounds(nextPos) && grid[nextPos.x, nextPos.y] == null) {
                    newPos = nextPos;
                    nextPos = newPos + new Vector2Int(direction.x, -direction.y);
                }

                // Check if we can merge with the tile at 'nextPos'
                if (IsWithinBounds(nextPos) && grid[nextPos.x, nextPos.y] != null &&
                    grid[nextPos.x, nextPos.y].value == currentTile.value &&
                    !merged[nextPos.x, nextPos.y]) {

                    // Merge tiles
                    grid[nextPos.x, nextPos.y].SetValue(grid[nextPos.x, nextPos.y].value * 2);
                    score += grid[nextPos.x, nextPos.y].value;
                    merged[nextPos.x, nextPos.y] = true;

                    // Destroy old tile AFTER movement animation finishes
                    StartCoroutine(DestroyAfterAnimation(currentTile, GridManager2048.Instance.GetCellAt(nextPos.x, nextPos.y).position));
                    grid[x, y] = null;
                    moved = true;
                }
            }
        }

        return moved;
    }




    private IEnumerator WaitForAnimations(List<Coroutine> animations) {
        foreach (Coroutine anim in animations) {
            yield return anim;
        }
        print("animations finished "+ animations.Count);
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
                if (grid[x, y] == null) return true; // ✅ Check for empty tiles (valid move)

                // ✅ Check if the right neighbor exists and has the same value
                if (IsWithinBounds(new Vector2Int(x + 1, y)) && grid[x + 1, y] != null && grid[x + 1, y].value == grid[x, y].value)
                    return true;

                // ✅ Check if the bottom neighbor exists and has the same value
                if (IsWithinBounds(new Vector2Int(x, y + 1)) && grid[x, y + 1] != null && grid[x, y + 1].value == grid[x, y].value)
                    return true;
            }
        }
        return false; // No moves available
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
