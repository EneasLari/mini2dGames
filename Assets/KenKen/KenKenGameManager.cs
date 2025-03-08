using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KenKenGameManager : MonoBehaviour {
    public int gridSize = 4; // Example: 4x4 grid
    public GameObject cellPrefab; // Prefab should include the KenKenCell component and a TMP_InputField
    public Transform gridParent;  // Parent GameObject with a Grid Layout Group component

    // List of cages set in the inspector (or built dynamically)
    public List<KenKenCage> cages;

    private KenKenCell[,] cells;

    void Start() {
        cells = new KenKenCell[gridSize, gridSize];

        // Dynamically create the grid cells
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                GameObject cellObj = Instantiate(cellPrefab, gridParent);
                KenKenCell cell = cellObj.GetComponent<KenKenCell>();
                cell.row = i;
                cell.column = j;
                cells[i, j] = cell;
            }
        }
        foreach (var cage in cages) {
            // Generate a random color for this cage.
            Color randomColor = new Color(Random.value, Random.value, Random.value);

            // Iterate over each cell position in the cage.
            for (int i = 0; i < cage.cellPositions.Count; i++) {
                Vector2Int pos = cage.cellPositions[i];
                cells[pos.x, pos.y].gameObject.GetComponent<Image>().color = randomColor;

                // Only set the target text on the first cell of the cage.
                if (i == 0) {
                    cells[pos.x, pos.y].transform.Find("Target")
                        .GetComponent<TMP_Text>().text = cage.targetValue.ToString() + cage.OperationString();
                }
            }
        }
    }

    // Call this method (e.g., via a UI button) to check if the solution is valid
    public void CheckSolution() {
        bool valid = true;

        // Check each row for unique numbers
        for (int i = 0; i < gridSize; i++) {
            HashSet<int> rowValues = new HashSet<int>();
            for (int j = 0; j < gridSize; j++) {
                int value = cells[i, j].Value;
                // Ensure value is within the acceptable range and unique in the row
                if (value < 1 || value > gridSize || rowValues.Contains(value)) {
                    valid = false;
                    break;
                }
                rowValues.Add(value);
            }
            if (!valid)
                break;
        }

        // Check each column for unique numbers
        for (int j = 0; j < gridSize; j++) {
            HashSet<int> colValues = new HashSet<int>();
            for (int i = 0; i < gridSize; i++) {
                int value = cells[i, j].Value;
                if (value < 1 || value > gridSize || colValues.Contains(value)) {
                    valid = false;
                    break;
                }
                colValues.Add(value);
            }
            if (!valid)
                break;
        }

        // Check each cage's arithmetic condition
        foreach (var cage in cages) {
            if (!cage.CheckCage(cells)) {
                valid = false;
                break;
            }
        }

        if (valid)
            Debug.Log("Solution is valid!");
        else
            Debug.Log("Solution is invalid!");
    }
}
