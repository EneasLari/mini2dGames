Below is an overview and sample code structure to help you build a KenKen Puzzle game in Unity using Text Mesh Pro for your UI elements.

---

## Overview

**KenKen Puzzle Mechanics:**

- **Grid-Based:** Like Sudoku, you’ll have an _N×N_ grid (e.g., 4×4, 6×6) where each row and column must contain unique numbers.
- **Cages:** Groups of cells (cages) have a math operation and target value. For example, a cage might state “6+” meaning the sum of its numbers must equal 6.
- **Solution Validation:** Besides checking for unique numbers in rows/columns, you also need to check that each cage meets its arithmetic condition.

**Unity Implementation:**

- **UI Setup:**
  - Use Unity’s UI system with a `Grid Layout Group` to dynamically generate the board.
  - For each cell, use a prefab that includes a [Text Mesh Pro - Input Field](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest) so users can enter numbers.
- **Game Logic:**
  - A **Cell Script** to hold the number (with TMP InputField).
  - A **GameManager Script** that initializes the grid, holds a reference to each cell, and validates rows, columns, and cages.
  - A **Cage Script** to store the list of cell coordinates for that cage, the target number, and the operation (addition, subtraction, multiplication, or division).

---

## Example Code

### KenKenCell.cs

This script represents each grid cell. It links to a TMP InputField for number entry.

```csharp
using UnityEngine;
using TMPro;

public class KenKenCell : MonoBehaviour
{
    public int row;
    public int column;
    public TMP_InputField inputField;

    // Returns the entered value (defaulting to 0 if parsing fails)
    public int Value {
        get {
            int.TryParse(inputField.text, out int val);
            return val;
        }
    }
}
```

### KenKenCage.cs

This script stores the cage’s properties and checks whether the cage’s cells satisfy the math condition.

```csharp
using System.Collections.Generic;
using UnityEngine;

public enum Operation {
    Addition,
    Subtraction,
    Multiplication,
    Division,
    None // For cages with a single cell
}

[System.Serializable]
public class KenKenCage
{
    public int targetValue;
    public Operation op;
    // List of cell positions in the grid that form this cage (row, column)
    public List<Vector2Int> cellPositions;

    // Check if the cage's values meet the target condition
    public bool CheckCage(KenKenCell[,] cells)
    {
        List<int> values = new List<int>();
        foreach (var pos in cellPositions)
        {
            values.Add(cells[pos.x, pos.y].Value);
        }

        switch(op)
        {
            case Operation.Addition:
                int sum = 0;
                foreach (int v in values) sum += v;
                return sum == targetValue;
            case Operation.Multiplication:
                int product = 1;
                foreach (int v in values) product *= v;
                return product == targetValue;
            case Operation.Subtraction:
                // Assuming subtraction cages only have 2 cells
                if (values.Count == 2)
                    return Mathf.Abs(values[0] - values[1]) == targetValue;
                break;
            case Operation.Division:
                // Assuming division cages only have 2 cells
                if (values.Count == 2)
                {
                    int a = values[0], b = values[1];
                    if (b != 0 && a / b == targetValue && a % b == 0) return true;
                    if (a != 0 && b / a == targetValue && b % a == 0) return true;
                }
                break;
            case Operation.None:
                // For a single cell cage, just check the value
                if (values.Count == 1)
                    return values[0] == targetValue;
                break;
        }
        return false;
    }
}
```

### KenKenGameManager.cs

This script manages grid initialization, stores cell references, and validates the overall puzzle solution.

```csharp
using System.Collections.Generic;
using UnityEngine;

public class KenKenGameManager : MonoBehaviour
{
    public int gridSize = 4; // Example: 4x4 grid
    public GameObject cellPrefab; // Prefab should include the KenKenCell component and a TMP_InputField
    public Transform gridParent;  // Parent GameObject with a Grid Layout Group component

    // List of cages set in the inspector (or built dynamically)
    public List<KenKenCage> cages;

    private KenKenCell[,] cells;

    void Start()
    {
        cells = new KenKenCell[gridSize, gridSize];

        // Dynamically create the grid cells
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject cellObj = Instantiate(cellPrefab, gridParent);
                KenKenCell cell = cellObj.GetComponent<KenKenCell>();
                cell.row = i;
                cell.column = j;
                cells[i, j] = cell;
            }
        }
    }

    // Call this method (e.g., via a UI button) to check if the solution is valid
    public void CheckSolution()
    {
        bool valid = true;

        // Check each row for unique numbers
        for (int i = 0; i < gridSize; i++)
        {
            HashSet<int> rowValues = new HashSet<int>();
            for (int j = 0; j < gridSize; j++)
            {
                int value = cells[i, j].Value;
                // Ensure value is within the acceptable range and unique in the row
                if (value < 1 || value > gridSize || rowValues.Contains(value))
                {
                    valid = false;
                    break;
                }
                rowValues.Add(value);
            }
            if (!valid)
                break;
        }

        // Check each column for unique numbers
        for (int j = 0; j < gridSize; j++)
        {
            HashSet<int> colValues = new HashSet<int>();
            for (int i = 0; i < gridSize; i++)
            {
                int value = cells[i, j].Value;
                if (value < 1 || value > gridSize || colValues.Contains(value))
                {
                    valid = false;
                    break;
                }
                colValues.Add(value);
            }
            if (!valid)
                break;
        }

        // Check each cage's arithmetic condition
        foreach (var cage in cages)
        {
            if (!cage.CheckCage(cells))
            {
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
```

---

## Implementation Steps

1. **Project Setup:**
   - Create a new Unity project.
   - Import the [Text Mesh Pro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest) package if it isn’t already in your project.
2. **UI Grid:**

   - Create an empty GameObject (e.g., “GridContainer”) and add a `Grid Layout Group` component.
   - Set the cell size, spacing, and constraint count (equal to gridSize).

3. **Cell Prefab:**

   - Create a prefab for your grid cell.
   - Add a UI element (such as a panel) to the prefab.
   - Add a TMP InputField as a child and attach the **KenKenCell.cs** script.
   - Ensure the InputField is properly linked in the inspector.

4. **Game Manager:**

   - Create an empty GameObject (e.g., “GameManager”) and attach the **KenKenGameManager.cs** script.
   - Assign the cell prefab and grid parent (GridContainer) in the inspector.
   - Set up the cages with their respective target values, operations, and cell coordinates (using a list of `Vector2Int` for each cage).

5. **Testing:**

   - Run the scene, fill in the grid numbers, and use a UI button (or key input) to call `CheckSolution()` to validate the puzzle.

6. **Enhancements:**
   - Add visual feedback for correct/incorrect input.
   - Animate cage borders and include hints.
   - Provide level selection and difficulty scaling.

---

This structure gives you a foundation for creating a KenKen puzzle game. You can expand upon it by dynamically generating puzzles, adding a puzzle solver, or including more sophisticated UI/UX elements as needed.

You can achieve that by introducing a flag (or using an index) so that the text setting occurs only for the first cell. For example, using a boolean flag:

```csharp
foreach (var cage in cages) {
    bool isFirst = true;
    foreach (var pos in cage.cellPositions) {
        cells[pos.x, pos.y].gameObject.GetComponent<Image>().color = Color.green;
        if (isFirst) {
            cells[pos.x, pos.y].transform.Find("Target").GetComponent<TMP_Text>().text = cage.targetValue.ToString() + cage.OperationString();
            isFirst = false;
        }
    }
}
```

Alternatively, you could use a for-loop with an index:

```csharp
foreach (var cage in cages) {
    for (int i = 0; i < cage.cellPositions.Count; i++) {
        Vector2Int pos = cage.cellPositions[i];
        cells[pos.x, pos.y].gameObject.GetComponent<Image>().color = Color.green;
        if (i == 0) {
            cells[pos.x, pos.y].transform.Find("Target").GetComponent<TMP_Text>().text = cage.targetValue.ToString() + cage.OperationString();
        }
    }
}
```

Both approaches ensure that the target text is applied only to the first position in each cage's cellPositions list.

You can generate a random color for each cage and then assign that color to every cell in the cage. Here's how you can modify your loop:

```csharp
foreach (var cage in cages)
{
    // Generate a random color for this cage.
    Color randomColor = new Color(Random.value, Random.value, Random.value);

    // Iterate over each cell position in the cage.
    for (int i = 0; i < cage.cellPositions.Count; i++)
    {
        Vector2Int pos = cage.cellPositions[i];
        cells[pos.x, pos.y].gameObject.GetComponent<Image>().color = randomColor;

        // Only set the target text on the first cell of the cage.
        if (i == 0)
        {
            cells[pos.x, pos.y].transform.Find("Target")
                .GetComponent<TMP_Text>().text = cage.targetValue.ToString() + cage.OperationString();
        }
    }
}
```

In this snippet, a random color is created using `Random.value` for the red, green, and blue channels. Then, for every cell in the cage, the `Image` component's color is set to this random color. The target text is only set for the first cell (when `i == 0`) of each cage.
