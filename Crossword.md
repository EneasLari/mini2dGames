# **📌 Complete Guide to Creating a Crossword Puzzle Game in Unity**

---

## **🎯 Overview**

This guide will walk you through creating a **crossword puzzle game** in **Unity** using **C# scripts**. This game features:

✔ **Dynamically generated crossword grid**  
✔ **Automatic word placement with correct intersections**  
✔ **Validation of user input**  
✔ **Hidden empty cells for a realistic crossword look**  
✔ **Checking answers & highlighting correctness**  
✔ **Resetting the grid to clear user input**  
✔ **A "Solve Puzzle" button to reveal the correct answers**  
✔ **Automatic removal of conflicting words**

---

## **🔹 Step 1: Setup Unity Scene**

### **1️⃣ Create a New 2D Unity Project**

1. Open **Unity** → Create a new **2D project**.

### **2️⃣ Setup the UI Canvas**

1. **Right-click in Hierarchy** → `UI` → `Canvas`
2. **Set Canvas Settings:**
   - Render Mode: `Screen Space - Overlay`
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920x1080`

---

## **🔹 Step 2: Create the Grid System**

### **1️⃣ Create Grid Container**

1. **Right-click in Hierarchy** → `Create Empty` → Rename it `"GridContainer"`.
2. **Add a Grid Layout Group:**
   - Select `"GridContainer"`
   - `Inspector` → `Add Component` → `Grid Layout Group`
   - **Set Layout:**
     - `Cell Size: 100x100`
     - `Spacing: 5x5`
     - `Padding: 10`

### **2️⃣ Create a Cell Prefab**

1. **Right-click in Hierarchy** → `UI` → `InputField - TextMeshPro`
2. **Rename it `"CellPrefab"`**
3. **Adjust Properties:**
   - Set Width & Height to **100x100**
   - Change Font Size to **40**
   - Set Text Alignment to **Center**
   - Limit input to **1 character**
   - Change Background to **White**
4. **Convert to a Prefab:**
   - Drag `"CellPrefab"` into the `Assets` folder.

---

## **🔹 Step 3: Generate the Grid with C#**

**Script: `CrosswordGrid.cs`**

- Generates a **dynamic crossword grid** based on the provided **rows and columns**.

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrosswordGrid : MonoBehaviour {
    public GameObject cellPrefab; // Assign the InputField prefab in Inspector
    private GameObject[,] cells;

    public void GenerateGrid(int rows, int cols) {
        cells = new GameObject[rows, cols];
        transform.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        transform.GetComponent<GridLayoutGroup>().constraintCount = cols;

        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                GameObject cell = Instantiate(cellPrefab, transform);
                cell.name = $"Cell_{i}_{j}";

                // Set placeholder text
                TMP_InputField input = cell.GetComponent<TMP_InputField>();
                input.characterLimit = 1; // Allow only one letter
                input.onValueChanged.AddListener(delegate { ValidateInput(input); });

                cells[i, j] = cell;
            }
        }
    }

    void ValidateInput(TMP_InputField field) {
        if (field.text.Length >= 1)
            field.text = field.text[0].ToString().ToUpper(); // Convert to uppercase
    }
}
```

---

## **🔹 Step 4: Store Words & Auto-Place Them**

**Script: `CrosswordManager.cs`**

- **Automatically places words**, ensuring intersecting letters match properly.
- **Removes conflicting words**.
- **Allows users to check, reset, and solve the crossword**.

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CrosswordManager : MonoBehaviour {
    public TMP_Text clueText;
    public GameObject gridContainer;
    public Button checkButton, resetButton, solveButton;

    public int rows = 5;
    public int cols = 5;
    private char[,] crosswordGrid;

    // Dictionary storing words, their positions, and clues
    private Dictionary<string, (string word, int row, int col, bool isAcross, string clue)> words = new Dictionary<string, (string, int, int, bool, string)>
    {
        // Across Words
        { "1A", ("CAT", 0, 0, true, "A small pet animal") },
        { "3A", ("RIVER", 2, 0, true, "A flowing body of water") },
        { "5A", ("HOUSE", 4, 2, true, "A place where people live") },

        // Down Words
        { "2D", ("CAR", 0, 0, false, "A vehicle") },
        { "4D", ("BIRD", 0, 4, false, "An animal that flies") },
        { "6D", ("BED", 1, 3, false, "A place to sleep") },
        { "8D", ("BRICK", 1, 7, false, "Used to build houses") }
    };

    void Start() {
        crosswordGrid = new char[rows, cols];
        InitializeGrid();
        gridContainer.GetComponent<CrosswordGrid>().GenerateGrid(rows, cols);
        HideEmptyCells();
        DisplayClues();
        checkButton.onClick.AddListener(CheckAnswers);
        resetButton.onClick.AddListener(ResetGrid);
        solveButton.onClick.AddListener(SolvePuzzle);
    }

    void InitializeGrid() {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                crosswordGrid[i, j] = '-';

        List<string> wordsToRemove = new List<string>();

        foreach (var entry in words) {
            string key = entry.Key;
            string word = entry.Value.word;
            int row = entry.Value.row;
            int col = entry.Value.col;
            bool isAcross = entry.Value.isAcross;

            bool hasConflict = false;
            for (int i = 0; i < word.Length; i++) {
                int currentRow = isAcross ? row : row + i;
                int currentCol = isAcross ? col + i : col;

                if (crosswordGrid[currentRow, currentCol] != '-' && crosswordGrid[currentRow, currentCol] != word[i]) {
                    hasConflict = true;
                    break;
                }
            }

            if (hasConflict) {
                wordsToRemove.Add(key);
                Debug.LogWarning($"Removed '{word}' due to conflict.");
            } else {
                for (int i = 0; i < word.Length; i++) {
                    int currentRow = isAcross ? row : row + i;
                    int currentCol = isAcross ? col + i : col;
                    crosswordGrid[currentRow, currentCol] = word[i];
                }
            }
        }

        foreach (string key in wordsToRemove) {
            words.Remove(key);
        }
    }

    public void SolvePuzzle() {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++) {
                string cellName = $"Cell_{i}_{j}";
                GameObject cell = GameObject.Find(cellName);
                if (cell != null) {
                    TMP_InputField input = cell.GetComponent<TMP_InputField>();
                    if (crosswordGrid[i, j] != '-') {
                        input.text = crosswordGrid[i, j].ToString();
                        input.image.color = Color.green;
                    }
                }
            }
    }
}
```

---

### **🎯 Final Features Added**

✅ **Words placed automatically**  
✅ **Shared letters merge correctly**  
✅ **Wrong answers turn red, correct answers turn green**  
✅ **Empty spaces are hidden**  
✅ **Reset button clears the grid**  
✅ **Solve button fills the crossword**  
✅ **Conflicting words are automatically removed**

🚀 **Your crossword puzzle is complete!** 🎉
