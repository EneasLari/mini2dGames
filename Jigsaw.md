**Step 1: Import and Slice the Image in Unity**

1. **Import Image**:

   - Drag your image file into Unity’s **Assets** folder.

2. **Set Texture as Sprite**:

   - Select your image from the Project panel.
   - In Inspector panel, set:
     - `Texture Type`: **Sprite (2D and UI)**
     - `Sprite Mode`: **Multiple**
   - Click **Apply**.

3. **Slice the Image**:
   - Click **Sprite Editor** (still in Inspector).
   - In Sprite Editor window:
     - Click **Slice** at the top-left.
     - Choose **Type**: **Grid by Cell Count** (e.g., 3x3 for a simple puzzle).
     - Click **Slice**, then **Apply**.

Once sliced, you'll see your puzzle pieces in the Project panel.

Good so far, puzzle master, or stuck on something?

Alright puzzle master, here’s a no-nonsense step-by-step guide to making your jigsaw game in Unity:

### **Step 2: Create Puzzle Pieces on Canvas**

- **Hierarchy → UI → Canvas** (creates canvas)
- Right-click Canvas → **Create Empty** named `PuzzleBoard`.
- Drag your sliced sprites from Project into the `PuzzleBoard` object. Each piece becomes a child object.

### **Step 3: Add Drag-and-Drop (with Snapping)**

- **Create C# Script** named `PuzzlePiece`:

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    Vector2 startPos;
    RectTransform rect;

    void Start() {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        transform.SetAsLastSibling(); // Moves dragged piece on top
    }

    public void OnDrag(PointerEventData eventData) {
        rect.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (Vector2.Distance(rect.anchoredPosition, startPos) < 30) // 30 pixels snapping threshold
            rect.anchoredPosition = startPos;
    }
}
```

- **Attach** this script to each puzzle piece (`Image` component).

### **Step 4: Set Correct Positions**

- Arrange puzzle pieces manually on the canvas to their correct positions.
- When done, note each piece’s anchored position (Inspector panel).
- Script above already stores the correct positions at the start of the game.

### **Step 5: Shuffle Pieces on Start**

- Add a shuffle function to `PuzzlePiece` script:

```csharp
void Awake() {
    rect = GetComponent<RectTransform>();
    startPos = rect.anchoredPosition;
}

void Start() {
    rect.anchoredPosition = new Vector2(Random.Range(-300, 300), Random.Range(-300, 300));
}
```

### **Step 6: Win Condition Check**

- Create an empty object `PuzzleManager`, attach this script:

```csharp
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour {
    public PuzzlePiece[] pieces;
    public TextMeshProUGUI winText;

    void Update() {
        foreach (var piece in pieces) {
            if (Vector2.Distance(piece.GetComponent<RectTransform>().anchoredPosition, piece.startPos) > 1)
                return; // exit if any piece is misplaced
        }
        winText.text = "You Win! 🥳";
    }
}
```

- Assign puzzle pieces and a `TextMeshProUGUI` text object to `PuzzleManager` in the Inspector.

### **Step 7: TextMesh Pro**

- If not imported yet: **Window → TextMeshPro → Import TMP Essentials**.
- Add `TextMeshProUGUI` object to Canvas for messages like "You Win! 🥳".

Test and run your masterpiece. If it blows up, let’s debug.

Here’s your clean, straightforward Unity puzzle scripts:

### `PuzzlePiece.cs`

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Vector2 correctPosition;
    RectTransform rect;
    Canvas canvas;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        correctPosition = rect.anchoredPosition;
    }

    void Start()
    {
        // Random shuffle at start
        rect.anchoredPosition = new Vector2(Random.Range(-300, 300), Random.Range(-300, 300));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling(); // Moves piece to the top
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(rect.anchoredPosition, correctPosition) < 30f)
        {
            rect.anchoredPosition = correctPosition; // Snap into place
        }
    }

    public bool IsPlacedCorrectly()
    {
        return Vector2.Distance(rect.anchoredPosition, correctPosition) < 1f;
    }
}
```

---

### `PuzzleManager.cs`

```csharp
using UnityEngine;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public PuzzlePiece[] pieces;
    public TextMeshProUGUI winText;

    void Start()
    {
        winText.text = "";
    }

    void Update()
    {
        foreach (var piece in pieces)
        {
            if (!piece.IsPlacedCorrectly())
                return;
        }

        winText.text = "You Win! 🎉";
    }
}
```

---

### Quick Setup Reminder:

- Attach `PuzzlePiece.cs` to each puzzle piece (each UI image).
- Create an empty GameObject (`PuzzleManager`) and attach `PuzzleManager.cs`.
- Assign all puzzle pieces and your TextMeshProUGUI object to `PuzzleManager` via Inspector.
- Don't forget to import TextMesh Pro essentials.

Now test it—good luck, puzzle maestro! If it misbehaves, let’s blame Unity first.

Here's a straightforward way to use **ScriptableObjects** for managing puzzle pieces:

### **1. Create a ScriptableObject definition**

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzlePieceSet", menuName = "Puzzle/PieceSet")]
public class PuzzlePieceSet : ScriptableObject
{
    public Sprite[] pieces;
}
```

### **2. Create an instance in Unity:**

- Right-click in your Project → **Create → Puzzle → PieceSet**.
- Drag your sliced sprites into the `pieces` array.

### **3. Update your loader script:**

```csharp
using UnityEngine;
using UnityEngine.UI;

public class PieceLoader : MonoBehaviour
{
    public PuzzlePieceSet pieceSet; // Reference the ScriptableObject
    public GameObject piecePrefab;
    public Transform parentCanvas;

    void Start()
    {
        foreach (Sprite sprite in pieceSet.pieces)
        {
            GameObject piece = Instantiate(piecePrefab, parentCanvas);
            piece.GetComponent<Image>().sprite = sprite;
        }
    }
}
```

### **Quick Setup:**

- Assign your newly created ScriptableObject to `PieceLoader`.
- Run it—clean, efficient, and neatly organized.
