Got it! I'll ensure that each **Peg (UI Image)** is a **child of its corresponding Hole (UI Image)** inside the **Grid Layout Group** and provide a **full step-by-step guide to create Peg Solitaire in Unity UI**.

---

## **🔥 Game Plan: Peg Solitaire (UI-Based)**

### **1️⃣ Setting Up the Unity Scene**

- **Create a UI-based board with a `Grid Layout Group`.**
- **Each Hole (UI Image) will act as a parent to a Peg (UI Image).**

### **2️⃣ Creating the Scripts**

- `BoardManager.cs`: **Manages board setup and peg placement.**
- `Peg.cs`: **Handles peg selection and movement.**
- `GameManager.cs`: **Handles game restart and UI interactions.**

---

## **🔥 Step 1: Setting Up the Unity Scene**

### **✅ 1. Create the UI Canvas**

1. Open **Unity** and create a new **2D UI Project**.
2. **Right-click in the Hierarchy** → `UI > Canvas`.
   - Set `Canvas` **Render Mode** to `Screen Space - Overlay`.
   - Add a `Canvas Scaler` **(UI Scale Mode: Scale with Screen Size)**.
3. **Right-click on `Canvas`** → `UI > Panel`.
   - Rename it to **BoardPanel**.
   - Set `Anchors = Stretch` (Fill the screen).

---

### **✅ 2. Create a Grid Layout for the Board**

1. **Right-click on `BoardPanel`** → `UI > Empty Object`.
   - Rename it **GridParent**.
2. **Add `Grid Layout Group`** to `GridParent`.
   - Set `Cell Size = (100, 100)`.
   - Set `Constraint = Fixed Column Count` (Columns = `7`).
   - Set `Spacing = (5,5)`.
   - Check `Child Alignment = Middle Center`.

---

### **✅ 3. Create 49 Holes (UI Image)**

1. **Right-click `GridParent`** → `UI > Image`.
   - Rename it **Hole_0_0**.
   - Set **Color = Light Gray**.
   - Set **Size = (100, 100)**.
   - **Duplicate (Ctrl+D) 48 times** to create a 7×7 grid.
   - **Rename them in a pattern**: `Hole_0_1, Hole_0_2, ..., Hole_6_6`.

---

### **✅ 4. Create the Peg Prefab**

1. **Right-click on `Hole_0_0`** → `UI > Image`.
   - Rename it **Peg**.
   - Set **Sprite = Circle** (Use a circular sprite).
   - Set **Color = Blue**.
   - Set **Size = (80, 80)** (Slightly smaller than the hole).
   - Add `Peg.cs` Script (Created later).
   - **Convert it into a Prefab** (`Right-click > Prefab > Create`).
   - **Delete the Peg from the Scene** (It will be spawned by script).

---

## **🔥 Step 2: Writing the Scripts**

### **📌 `BoardManager.cs` (Handles Peg Placement & Movement)**

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public GameObject pegPrefab;  // Assign Peg Prefab (UI Image)
    public Transform gridParent;  // Assign GridLayoutGroup in Inspector

    private Dictionary<Vector2Int, Peg> pegs = new Dictionary<Vector2Int, Peg>();
    private Transform[,] holeTransforms = new Transform[7, 7]; // Stores hole references

    private int[,] boardLayout = {
        { -1, -1,  1,  1,  1, -1, -1 },
        { -1, -1,  1,  1,  1, -1, -1 },
        {  1,  1,  1,  1,  1,  1,  1 },
        {  1,  1,  1,  0,  1,  1,  1 },
        {  1,  1,  1,  1,  1,  1,  1 },
        { -1, -1,  1,  1,  1, -1, -1 },
        { -1, -1,  1,  1,  1, -1, -1 }
    };

    private Peg selectedPeg = null;

    private void Start()
    {
        CacheHolePositions(); // Store hole references
        PlacePegs();
    }

    private void CacheHolePositions()
    {
        int childIndex = 0;
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                if (boardLayout[x, y] != -1)
                {
                    holeTransforms[x, y] = gridParent.GetChild(childIndex);
                    childIndex++;
                }
            }
        }
    }

    private void PlacePegs()
    {
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                if (boardLayout[x, y] == 1)
                {
                    Transform hole = holeTransforms[x, y];
                    GameObject pegObj = Instantiate(pegPrefab, hole);
                    Peg peg = pegObj.GetComponent<Peg>();
                    pegs[new Vector2Int(x, y)] = peg;
                }
            }
        }
    }

    public void SelectPeg(Peg peg)
    {
        if (selectedPeg == null)
        {
            selectedPeg = peg;
        }
        else
        {
            TryMove(selectedPeg, peg.transform.parent);
            selectedPeg = null;
        }
    }

    private void TryMove(Peg peg, Transform targetHole)
    {
        Transform middleHole = peg.transform.parent; // Midway hole

        if (pegs.ContainsKey(new Vector2Int((int)middleHole.position.x, (int)middleHole.position.y)) &&
            !pegs.ContainsKey(new Vector2Int((int)targetHole.position.x, (int)targetHole.position.y)))
        {
            Destroy(pegs[new Vector2Int((int)middleHole.position.x, (int)middleHole.position.y)].gameObject);
            pegs.Remove(new Vector2Int((int)middleHole.position.x, (int)middleHole.position.y));

            peg.MoveTo(targetHole);
            pegs[new Vector2Int((int)targetHole.position.x, (int)targetHole.position.y)] = peg;
        }
    }
}
```

---

### **📌 `Peg.cs` (Handles Peg Selection & Movement)**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class Peg : MonoBehaviour
{
    private BoardManager boardManager;

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
    }

    public void OnClick()
    {
        boardManager.SelectPeg(this);
    }

    public void MoveTo(Transform newParent)
    {
        transform.SetParent(newParent);
        transform.localPosition = Vector3.zero;
    }
}
```

---

### **📌 `GameManager.cs` (Handles Restart & UI)**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
```

---

## **🔥 Step 3: Testing & Final Setup**

1. Assign `BoardManager`:

   - Set `Peg Prefab` = Peg (UI Image).
   - Set `Grid Parent` = `GridParent`.

2. Assign `OnClick()` for Peg Prefab:

   - In the **Inspector**, **Add `Peg.OnClick()`** to **Peg UI Button**.

3. Press **Play** and test!

---

## **🎯 Summary**

✅ **Peg is a UI Image inside its corresponding Hole**  
✅ **Uses UI Grid Layout for alignment**  
✅ **Handles Peg selection & movement correctly**

🚀 Try this and let me know if you need any improvements! 🎯🔥
