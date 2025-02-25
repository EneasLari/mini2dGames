# **📝 How to Make a Word Search Game in Unity (Step-by-Step Guide)**

This guide will walk you through **creating a fully functional Word Search game** in Unity, covering **grid generation, word placement, UI interaction, and winning conditions**.

---

# **📌 1. Setting Up the Unity Project**

### **✅ Step 1: Create a 2D Unity Project**

1. Open **Unity Hub** and click **New Project**.
2. Select **2D Template** and name it `"WordSearchGame"`.
3. Click **Create Project**.

### **✅ Step 2: Set Up the Canvas for UI**

1. **Right-click in the Hierarchy** → `UI` → `Canvas`.
2. Select the **Canvas** and set:
   - **Render Mode:** `Screen Space - Overlay`
3. Add a **Panel** inside the Canvas (this will contain the grid).

### **✅ Step 3: Add UI Elements**

1. Add a **Grid Layout Group** for the word grid:
   - Right-click **Canvas** → `UI` → `Panel`
   - Rename it `GridPanel`
   - **Add Component:** `Grid Layout Group`
   - Set:
     - **Cell Size:** `(50, 50)`
     - **Spacing:** `(5, 5)`
     - **Constraint:** `Fixed Column Count` (set to grid size)
2. Add a **TextMeshPro UI Element** to display selected words:
   - `UI` → `TextMeshPro - Text`
   - Rename it `SelectedWordText`
3. Add an **InputField (TMP)** and a **Button** for adding custom words.

---

# **📌 2. Create the Word Search Grid System**

### **✅ Step 1: Create the Grid Manager Script**

1. **Right-click in the Project window** → `Create` → `C# Script`
2. Name it **`WordSearchGridManager.cs`**
3. Attach it to an **Empty GameObject** in the scene (rename it `GameManager`).

---

## **🎮 Word Search Grid Script**

Below is the **full Word Search Grid system** that:

- **Creates a letter grid**
- **Places words**
- **Fills empty spaces**
- **Generates UI elements**

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WordSearchGridManager : MonoBehaviour
{
    public GameObject letterButtonPrefab; // UI button prefab for letters
    public Transform gridParent; // Parent for the grid UI
    public int gridSize = 12; // 12x12 grid size
    public List<string> wordsToPlace = new List<string>
    {
        "UNITY", "GAME", "CODE", "SCRIPT", "DEBUG", "PLAYER", "LEVEL",
        "REWARD", "OBJECT", "SYSTEM", "ENGINE", "VECTOR", "PHYSICS"
    };

    private char[,] letterGrid; // 2D letter array
    private List<Vector2Int> usedPositions = new List<Vector2Int>();

    void Start()
    {
        letterGrid = new char[gridSize, gridSize]; // Initialize the letter grid
        PlaceWords();      // Place words first
        FillEmptySpaces(); // Fill the rest with random letters
        GenerateGridUI();  // Generate UI elements for the grid
    }

    /// <summary>
    /// Places words randomly in the grid.
    /// </summary>
    void PlaceWords()
    {
        List<Vector2Int> directions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),  // Right
            new Vector2Int(-1, 0), // Left
            new Vector2Int(0, 1),  // Down
            new Vector2Int(0, -1), // Up
            new Vector2Int(1, 1),  // Diagonal Down-Right
            new Vector2Int(-1, -1),// Diagonal Up-Left
            new Vector2Int(-1, 1), // Diagonal Down-Left
            new Vector2Int(1, -1)  // Diagonal Up-Right
        };

        foreach (string word in wordsToPlace)
        {
            bool placed = false;
            int attempts = 0, maxAttempts = 100;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;
                int startX = Random.Range(0, gridSize);
                int startY = Random.Range(0, gridSize);
                Vector2Int direction = directions[Random.Range(0, directions.Count)];

                if (CanPlaceWord(word, startX, startY, direction.x, direction.y))
                {
                    for (int i = 0; i < word.Length; i++)
                    {
                        int newX = startX + i * direction.x;
                        int newY = startY + i * direction.y;

                        letterGrid[newX, newY] = word[i]; // ✅ Assign word letter
                        usedPositions.Add(new Vector2Int(newX, newY));
                    }
                    placed = true;
                }
            }
        }
    }

    /// <summary>
    /// Checks if a word can be placed at a given position.
    /// </summary>
    bool CanPlaceWord(string word, int startX, int startY, int directionX, int directionY)
    {
        if (word.Length > gridSize) return false; // Prevent too long words

        for (int i = 0; i < word.Length; i++)
        {
            int newX = startX + i * directionX;
            int newY = startY + i * directionY;

            if (newX < 0 || newX >= gridSize || newY < 0 || newY >= gridSize)
                return false; // Prevent out-of-bounds placement

            if (usedPositions.Contains(new Vector2Int(newX, newY)) && letterGrid[newX, newY] != word[i])
                return false; // Prevent incorrect overlap
        }
        return true;
    }

    /// <summary>
    /// Fills remaining spaces with random letters.
    /// </summary>
    void FillEmptySpaces()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                Vector2Int position = new Vector2Int(row, col);
                if (!usedPositions.Contains(position))
                {
                    letterGrid[row, col] = (char)('A' + Random.Range(0, 26)); // ✅ Random letter
                }
            }
        }
    }

    /// <summary>
    /// Creates UI buttons for the grid.
    /// </summary>
    void GenerateGridUI()
    {
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                GameObject letterButton = Instantiate(letterButtonPrefab, gridParent);
                letterButton.GetComponentInChildren<TMP_Text>().text = letterGrid[row, col].ToString();
                letterButton.name = $"Letter_{row}_{col}";
            }
        }
    }
}
```

---

# **📌 3. Making the Game Interactive**

✅ **Detect player input**  
✅ **Allow selection of letters**  
✅ **Check if selected words are correct**  
✅ **Highlight correct words**  
✅ **Track found words and display progress**

Would you like me to add the **word selection system** next? 🚀🎮
