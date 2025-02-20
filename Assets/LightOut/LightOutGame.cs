using UnityEngine;
using UnityEngine.UI;

public class LightsOutGame : MonoBehaviour {
    public int gridSize = 5; // Grid size (5x5)
    public GameObject lightPrefab; // Light button prefab
    public Transform gridPanel; // UI Panel for the grid
    private LightButton[,] grid;

    void Start() {
        GenerateGrid();
    }

    void GenerateGrid() {
        grid = new LightButton[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                GameObject lightObj = Instantiate(lightPrefab, gridPanel);
                LightButton light = lightObj.GetComponent<LightButton>();

                light.x = x;
                light.y = y;
                light.SetState(Random.value > 0.5f); // Randomly set initial state

                grid[x, y] = light;
            }
        }
    }

    public void ToggleLight(int x, int y) {
        if (grid == null) return;

        ToggleSingleLight(x, y); // The clicked light
        ToggleSingleLight(x - 1, y); // Left
        ToggleSingleLight(x + 1, y); // Right
        ToggleSingleLight(x, y - 1); // Down
        ToggleSingleLight(x, y + 1); // Up

        CheckWinCondition();
    }

    void ToggleSingleLight(int x, int y) {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize) {
            grid[x, y].SetState(!grid[x, y].IsOn());
        }
    }

    void CheckWinCondition() {
        foreach (var light in grid) {
            if (light.IsOn()) return; // If any light is still on, game continues
        }
        Debug.Log("You Win!");
    }
}
