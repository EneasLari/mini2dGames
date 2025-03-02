## **🔹 Unity Drag-and-Drop Unscramble Game - Complete Guide** 🎮

This step-by-step guide will walk you through **creating a Drag-and-Drop Unscramble Word Game in Unity** using **UI elements and C# scripts**. By the end, you'll have a functional game where players **drag letters into slots** to form the correct word.

---

## **📌 1. Project Setup**

### **Step 1: Create a New Unity Project**

1. Open **Unity Hub** → Click **New Project**.
2. Select the **2D Template**.
3. Name it **"UnscrambleGame"**.
4. Click **Create**.

---

## **📌 2. Setting Up UI Elements**

### **Step 2: Create a Canvas for UI**

1. **Right-click in the Hierarchy** → `UI` → `Canvas`.
2. Select **Canvas**:
   - Change **Render Mode** to `Screen Space - Overlay`.
   - Set **UI Scale Mode** to `Scale with Screen Size`.
   - Adjust the **Reference Resolution** (e.g., `1080x1920` for mobile).

---

### **Step 3: Add UI Components**

| **UI Element**                              | **Purpose**                                       |
| ------------------------------------------- | ------------------------------------------------- |
| **Panel (LetterContainer)**                 | Holds scrambled draggable letters.                |
| **Panel (DropZoneContainer)**               | Holds empty slots where players drop letters.     |
| **TextMeshPro - Text (ScoreText)**          | Displays the player's score.                      |
| **TextMeshPro - Text (TimerText)**          | Shows the countdown timer.                        |
| **TextMeshPro - Text (StatusText)**         | Displays messages like "Correct!" or "Game Over". |
| **Image (GameStatusPanel)**                 | A semi-transparent background for messages.       |
| **Buttons (Check, Shuffle, Hint, Restart)** | Allow interactions.                               |

---

### **Step 4: Creating the Letter Container (Scrambled Letters)**

1. `Right-click on Canvas` → `UI` → `Panel`.
2. Rename it **"LetterContainer"**.
3. **Add Grid Layout Group**:
   - Click **"Add Component"** → `Grid Layout Group`.
   - Set **Child Alignment** to `Middle Center`.
   - Adjust **Cell Size** (`100x100` for square letters).

---

### **Step 5: Creating the Drop Zone (Drop Slots)**

1. `Right-click on Canvas` → `UI` → `Panel`.
2. Rename it **"DropZoneContainer"**.
3. **Add Horizontal Layout Group**:
   - Click **"Add Component"** → `Horizontal Layout Group`.
   - Set **Child Alignment** to `Middle Center`.
   - Adjust **Spacing** to fit word length.

---

### **Step 6: Creating UI Text & Buttons**

1. `Right-click on Canvas` → `UI` → `Text - TextMeshPro`.
   - Rename it **"ScoreText"** (`Text: Score: 0`).
   - Rename another **"TimerText"** (`Text: Time Left: 30s`).
   - Rename another **"StatusText"** (Hidden at start).
2. `Right-click on Canvas` → `UI` → `Image`.
   - Rename it **"GameStatusPanel"**.
   - Set **Alpha** to 50% (Semi-transparent background for messages).
   - **Disable it initially** (`SetActive(false)`).
3. `Right-click on Canvas` → `UI` → `Button`.
   - Rename it **"CheckButton"** (`Text: Check`).
   - Rename another **"ShuffleButton"** (`Text: Shuffle`).
   - Rename another **"HintButton"** (`Text: Hint`).
   - Rename another **"RestartButton"** (`Text: Restart`, **Initially Disabled**).

---

## **📌 3. Creating Prefabs**

### **Step 7: Create Letter Prefab (Draggable Letter)**

1. `Right-click on Hierarchy` → `UI` → `Button`.
2. Rename it **"LetterPrefab"**.
3. Inside, rename the text object **"LetterText"**.
4. Click `LetterPrefab` → **"Add Component"** → `Canvas Group`.
5. Click `LetterPrefab` → **"Add Component"** → `DraggableLetter.cs` (**Script Below**).
6. **Save it as a Prefab**: Drag it into `Assets`.

---

### **Step 8: Create DropSlot Prefab (Letter Slots)**

1. `Right-click on Hierarchy` → `UI` → `Panel`.
2. Rename it **"DropSlot"**.
3. Click `DropSlot` → **"Add Component"** → `DropSlot.cs` (**Script Below**).
4. Click `DropSlot` → **"Add Component"** → `Canvas Group`.
5. **Save it as a Prefab**: Drag it into `Assets`.

---

## **📌 4. Writing the Scripts**

### **Step 9: Draggable Letter Script (`DraggableLetter.cs`)**

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    private Transform parentAfterDrag;
    private CanvasGroup canvasGroup;

    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == transform.root) {
            transform.SetParent(parentAfterDrag);
        }
    }
}
```

---

### **Step 10: Drop Slot Script (`DropSlot.cs`)**

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler {
    public void OnDrop(PointerEventData eventData) {
        GameObject dropped = eventData.pointerDrag;

        if (dropped != null) {
            dropped.transform.SetParent(transform);
            dropped.transform.position = transform.position;
        }
    }
}
```

---

### **Step 11: Game Manager Script (`DragDropManager.cs`)**

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DragDropManager : MonoBehaviour {
    public GameObject letterPrefab, dropSlotPrefab;
    public Transform letterContainer, dropZoneContainer;
    public Button checkButton, shuffleButton, hintButton, restartButton;
    public TextMeshProUGUI timerText, scoreText, statusText;
    public Image GameStatusPanel;

    private string correctWord;
    private int score = 0;
    private float timeLeft = 30f;
    private bool isGameActive = true, isPaused = false;

    private List<string> wordList = new List<string> { "UNITY", "SCRIPT", "BUTTON", "TEXT", "GAME", "CANVAS" };

    void Start() {
        restartButton.gameObject.SetActive(false);
        GameStatusPanel.gameObject.SetActive(false);
        statusText.gameObject.SetActive(false);
        restartButton.onClick.AddListener(RestartGame);
        GenerateNewWord();
        StartCoroutine(TimerCountdown());
    }

    void CheckAnswer() {
        string playerWord = "";
        foreach (Transform slot in dropZoneContainer) {
            if (slot.childCount > 0)
                playerWord += slot.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text;
        }
        if (playerWord == correctWord) {
            score += 10;
            scoreText.text = "Score: " + score;
            StartCoroutine(DisplayStatusMessage("🎉 Correct! Well Done! 🎉"));
        }
    }

    IEnumerator DisplayStatusMessage(string message) {
        isPaused = true;
        statusText.text = message;
        GameStatusPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        GameStatusPanel.gameObject.SetActive(false);
        isPaused = false;
    }
}
```

---

## **✅ Conclusion**

🎉 **Now you have a complete Drag-and-Drop Unscramble Word Game!**  
Would you like to add **animations or sound effects?** 🚀😃
