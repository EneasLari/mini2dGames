using UnityEngine;

public class GridManager2048 : MonoBehaviour {
    public static GridManager2048 Instance; // Singleton for easy access

    public int gridSize = 4;
    public GameObject cellPrefab;
    public Transform gridParent;
    public float cellSize = 100f; // Adjust based on your UI scale

    private Transform[,] gridCells; // Stores cell positions

    void Awake() {
        Instance = this; // Assign Singleton instance
    }


    public void GenerateGrid() {
        gridCells = new Transform[gridSize, gridSize];

        float gridOffset = (gridSize - 1) * cellSize / 2;

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                // Instantiate grid cell UI
                GameObject newCell = Instantiate(cellPrefab, gridParent);
                RectTransform cellRect = newCell.GetComponent<RectTransform>();

                // Correct positioning within the grid container
                cellRect.localPosition = new Vector3((x * cellSize) - gridOffset, (gridOffset - y * cellSize), 0);
                cellRect.localScale = Vector3.one;

                // Store the cell's Transform for later tile placement
                gridCells[x, y] = newCell.transform;
            }
        }
    }

    // Function to Get the Correct Cell Position
    public Transform GetCellAt(int x, int y) {
        return gridCells[x, y];
    }
}
