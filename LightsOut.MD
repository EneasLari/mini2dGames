Sure! Here’s how we’ll go about creating a **Lights Out** game in **Unity (2D)**:

### 🔹 **Steps to Implement Lights Out in Unity**

1. **Create the Grid UI** (using Unity’s UI buttons)
2. **Handle Click Interactions** (toggle lights and adjacent ones)
3. **Check for Win Condition** (all lights turned off)
4. **Add Some Effects** (for better visual feedback)
5. **Optimize the Code** (make it flexible for different grid sizes)

---

### 🔸 **1. Setting Up the Unity Scene**

- Open **Unity** and create a **2D project**.
- In the **Canvas**, add a **Grid Layout Group** to arrange buttons dynamically.
- Set up a **Panel** (inside Canvas) and attach **Grid Layout Group** to it.

📌 **Inspector Settings for Grid Layout Group**

- Set **Cell Size** (e.g., `100x100`).
- Set **Spacing** (`5x5` for gaps).
- Align **child alignment** to `Middle Center`.

---

### 🔸 **2. Creating a Light Button Prefab**

1. **Create a Button** (`UI > Button`) inside the Panel.
2. Change its color to **yellow** (indicating "ON").
3. Rename it to **LightButton**.
4. Attach a **script** to it (`LightButton.cs`).
5. Convert it into a **Prefab** (`Right-click > Prefab`).

---

### 🔹 **3. Writing the LightButton Script (`LightButton.cs`)**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class LightButton : MonoBehaviour
{
    public int x, y; // Position in grid
    private LightsOutGame gameManager;
    private bool isOn = true; // Light state
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleLights);
        gameManager = FindObjectOfType<LightsOutGame>();
        UpdateColor();
    }

    void ToggleLights()
    {
        gameManager.ToggleLight(x, y);
    }

    public void SetState(bool state)
    {
        isOn = state;
        UpdateColor();
    }

    private void UpdateColor()
    {
        GetComponent<Image>().color = isOn ? Color.yellow : Color.black;
    }

    public bool IsOn()
    {
        return isOn;
    }
}
```

📌 **What This Does:**

- Stores **position (`x, y`)** in the grid.
- Toggles **state** and updates color.
- Calls the **game manager** to toggle adjacent lights.

---

### 🔸 **4. Creating the Game Manager Script (`LightsOutGame.cs`)**

1. Create an **empty GameObject** (`GameManager`).
2. Attach a script called **LightsOutGame.cs**.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class LightsOutGame : MonoBehaviour
{
    public int gridSize = 5; // Grid size (5x5)
    public GameObject lightPrefab; // Light button prefab
    public Transform gridPanel; // UI Panel for the grid
    private LightButton[,] grid;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        grid = new LightButton[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject lightObj = Instantiate(lightPrefab, gridPanel);
                LightButton light = lightObj.GetComponent<LightButton>();

                light.x = x;
                light.y = y;
                light.SetState(Random.value > 0.5f); // Randomly set initial state

                grid[x, y] = light;
            }
        }
    }

    public void ToggleLight(int x, int y)
    {
        if (grid == null) return;

        ToggleSingleLight(x, y); // The clicked light
        ToggleSingleLight(x - 1, y); // Left
        ToggleSingleLight(x + 1, y); // Right
        ToggleSingleLight(x, y - 1); // Down
        ToggleSingleLight(x, y + 1); // Up

        CheckWinCondition();
    }

    void ToggleSingleLight(int x, int y)
    {
        if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
        {
            grid[x, y].SetState(!grid[x, y].IsOn());
        }
    }

    void CheckWinCondition()
    {
        foreach (var light in grid)
        {
            if (light.IsOn()) return; // If any light is still on, game continues
        }
        Debug.Log("You Win!");
    }
}
```

📌 **What This Does:**

- **Generates the grid** dynamically.
- **Randomly initializes** lights (ON/OFF).
- Handles **light toggling** (including adjacent lights).
- Checks for the **win condition**.

---

### 🔹 **5. Final Touches**

- **Winning Effect:** Add a simple UI text `"You Win!"` when all lights are off.
- **Reset Button:** To restart the game.
- **Difficulty Options:** Allow changing grid size (e.g., `3x3`, `4x4`, `5x5`).

---

### 🔸 **Next Steps**

- **UI animations** (like smooth color fading)?
- **Difficulty setting** for different grid sizes?
- **Sound effects** (click, win sound)?
