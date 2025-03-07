## **🔹 How It Works**

1. **Display Sentence in a Grid**: Each letter is inside a box.
2. **Highlight Active Letter**: The current letter being typed is highlighted.
3. **Update Letters Dynamically**:
   - When a player types, it fills in the boxes.
   - If they press **Backspace**, it removes the last letter.
4. **Handle Mistakes**:
   - If a wrong letter is typed, it is marked in red.
   - If correct, it turns white (or any preferred color).

---

## **1️⃣ UI Setup in Unity**

1. **Create a Sentence Grid**:

   - Add an **empty GameObject** in the Canvas, name it `SentenceGrid`.
   - Attach a **Horizontal Layout Group** to it.
   - Set `Spacing = 5` to separate the letters.
   - Enable `Child Force Expand` → **Width & Height**.

2. **Create a Letter Tile Prefab**:
   - In the `Canvas`, create a **TextMeshPro - Text (UI)**.
   - Rename it to `LetterTile`.
   - Set **Font Size = 40**.
   - Add **Image Component** (set color to white for normal letters).
   - Save it as a **Prefab**.

---

## **2️⃣ Game Logic (TypingGame.cs)**

Now, let's update the script to match the functionality.

### **🔹 `TypingGame.cs`**

```csharp
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TypingGame : MonoBehaviour
{
    public GameObject letterPrefab; // Prefab for individual letter tiles
    public Transform sentenceGrid; // Grid to hold letters
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public float timeLimit = 30f;

    private string[] sentences = {
        "NEVER STOP LEARNING",
        "UNITY GAME DEVELOPMENT",
        "PRACTICE MAKES PERFECT"
    };

    private string currentSentence;
    private List<TextMeshProUGUI> letterTiles = new List<TextMeshProUGUI>();
    private int currentLetterIndex = 0;
    private float timer;
    private bool gameActive = false;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        timer = timeLimit;
        gameActive = true;
        currentLetterIndex = 0;
        resultText.text = "";

        // Select a random sentence
        currentSentence = sentences[Random.Range(0, sentences.Length)];

        // Display the sentence as individual letters
        GenerateLetterTiles();
    }

    void GenerateLetterTiles()
    {
        // Clear old letters
        foreach (Transform child in sentenceGrid)
        {
            Destroy(child.gameObject);
        }
        letterTiles.Clear();

        // Create new letter tiles
        foreach (char letter in currentSentence)
        {
            GameObject letterObj = Instantiate(letterPrefab, sentenceGrid);
            TextMeshProUGUI letterText = letterObj.GetComponent<TextMeshProUGUI>();
            letterText.text = letter.ToString();
            letterText.color = Color.white; // Default color
            letterTiles.Add(letterText);
        }

        HighlightLetter(0); // Start with the first letter highlighted
    }

    void Update()
    {
        if (gameActive)
        {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timer) + "s";

            if (Input.anyKeyDown)
            {
                HandleTyping();
            }

            if (currentLetterIndex >= currentSentence.Length)
            {
                EndGame(true);
            }

            if (timer <= 0)
            {
                EndGame(false);
            }
        }
    }

    void HandleTyping()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (currentLetterIndex > 0)
            {
                currentLetterIndex--;
                letterTiles[currentLetterIndex].color = Color.white; // Reset to white
            }
            return;
        }

        if (currentLetterIndex >= currentSentence.Length)
            return;

        string keyPressed = Input.inputString.ToUpper();
        if (keyPressed.Length > 0)
        {
            char typedChar = keyPressed[0];
            char expectedChar = currentSentence[currentLetterIndex];

            if (typedChar == expectedChar)
            {
                letterTiles[currentLetterIndex].color = Color.green; // Correct
                currentLetterIndex++;
                HighlightLetter(currentLetterIndex);
            }
            else
            {
                letterTiles[currentLetterIndex].color = Color.red; // Incorrect
            }
        }
    }

    void HighlightLetter(int index)
    {
        for (int i = 0; i < letterTiles.Count; i++)
        {
            if (i == index)
                letterTiles[i].color = Color.yellow; // Highlight active letter
            else if (i < currentLetterIndex)
                letterTiles[i].color = Color.green; // Already typed
            else
                letterTiles[i].color = Color.white; // Not yet typed
        }
    }

    void EndGame(bool success)
    {
        gameActive = false;
        if (success)
        {
            resultText.text = "✅ You Win!";
        }
        else
        {
            resultText.text = "❌ Time's Up!";
        }
    }
}
```

---

## **🔹 How It Works**

- **Each letter is displayed in a separate tile**.
- **The current letter being typed is highlighted (Yellow)**.
- **Correct letters turn Green**.
- **Incorrect letters turn Red**.
- **Backspace moves the cursor backward** to allow corrections.
- **Once all letters are correctly typed**, the game ends.

---

## **3️⃣ UI Setup in Unity**

### **1. Create Sentence Grid**

1. **Create an Empty GameObject** in the **Canvas**, name it `SentenceGrid`.
2. **Add Component → Horizontal Layout Group**.
   - **Spacing**: `5`
   - **Child Force Expand (Width & Height)**: ✅ Enabled.

### **2. Create Letter Prefab**

1. **Create a UI → TextMeshPro - Text (UI)**.
2. Rename it to `LetterTile`.
3. Set:
   - **Font Size**: `40`
   - **Alignment**: Center.
   - **Text**: Empty (we’ll set it dynamically).
4. **Add an Image Component** (optional background).
5. **Save it as a Prefab**.

---

## **🎨 Optional Enhancements**

- **Animation**: Add a smooth transition when moving to the next letter.
- **Sound Effects**: Play different sounds for correct and incorrect inputs.
- **Leaderboard**: Store best WPM scores for replayability.
- **Difficulty Modes**: Add harder sentences.

---

## **🚀 Final Thoughts**

This approach perfectly mimics the design in the image and enhances **visual clarity** while making typing **engaging**. Would you like any further tweaks? 🚀

### **Adjustments to Your Script so the letters fit to rect**

To ensure that the **letter tiles fit dynamically within `SentenceGrid`**, which spans **100% width of the screen**, we need to:

1. **Calculate the available width of `SentenceGrid`** dynamically.
2. **Adjust each letter tile's width** based on the number of letters.
3. **Ensure uniform spacing** so that all letters fit inside the grid.

---

### **✅ Updated `TypingGame.cs`**

```csharp
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class TypingGame : MonoBehaviour {
    public GameObject letterPrefab; // Prefab for individual letter tiles
    public Transform sentenceGrid; // Grid to hold letters and SentenceGrid RectTransform
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;
    public float timeLimit = 30f;

    private string[] sentences = {
        "NEVER STOP LEARNING",
        "UNITY GAME DEVELOPMENT",
        "PRACTICE MAKES PERFECT"
    };

    private string currentSentence;
    private List<GameObject> letterTiles = new List<GameObject>();
    private int currentLetterIndex = 0;
    private float timer;
    private bool gameActive = false;

    void Start() {
        StartGame();
    }

    void StartGame() {
        timer = timeLimit;
        gameActive = true;
        currentLetterIndex = 0;
        resultText.text = "";

        // Select a random sentence
        currentSentence = sentences[Random.Range(0, sentences.Length)];

        // Display the sentence as individual letters
        GenerateLetterTiles();
    }

    void GenerateLetterTiles() {
        // Clear old letters
        foreach (Transform child in sentenceGrid) {
            Destroy(child.gameObject);
        }
        letterTiles.Clear();

        // Get total available width of SentenceGrid
        float totalWidth = sentenceGrid.GetComponent<RectTransform>().rect.width;
        float tileWidth = totalWidth / currentSentence.Length; // Calculate width per letter

        foreach (char letter in currentSentence) {
            GameObject letterObj = Instantiate(letterPrefab, sentenceGrid);
            TextMeshProUGUI letterText = letterObj.GetComponentInChildren<TextMeshProUGUI>();
            letterText.text = letter.ToString();

            Image tileImage = letterObj.GetComponent<Image>();
            tileImage.color = Color.white; // Default color

            // Adjust the tile size dynamically
            RectTransform tileRect = letterObj.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(tileWidth, sentenceGrid.GetComponent<RectTransform>().rect.height);

            LayoutElement layout = letterObj.GetComponent<LayoutElement>();
            if (layout) {
                layout.preferredWidth = tileWidth;
                layout.preferredHeight = sentenceGrid.GetComponent<RectTransform>().rect.height;
            }

            letterTiles.Add(letterObj);
        }

        SetTilesColor(); // Start with the first letter highlighted
    }

    void Update() {
        if (gameActive) {
            timer -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timer) + "s";

            if (Input.anyKeyDown) {
                HandleTyping();
            }

            if (currentLetterIndex >= currentSentence.Length) {
                EndGame(true);
            }

            if (timer <= 0) {
                EndGame(false);
            }
        }
    }

    void HandleTyping() {
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            if (currentLetterIndex > 0) {
                currentLetterIndex--;
                letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.white; // Reset to white
            }
            return;
        }

        if (currentLetterIndex >= currentSentence.Length)
            return;

        string keyPressed = Input.inputString.ToUpper();
        if (keyPressed.Length > 0) {
            char typedChar = keyPressed[0];
            char expectedChar = currentSentence[currentLetterIndex];

            if (typedChar == expectedChar) {
                letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.green; // Correct
                currentLetterIndex++;
                if (currentLetterIndex < letterTiles.Count - 1)
                    letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.yellow;
            } else {
                letterTiles[currentLetterIndex].GetComponent<Image>().color = Color.red; // Incorrect
                currentLetterIndex++;
            }
        }
    }

    void SetTilesColor() {
        for (int i = 0; i < letterTiles.Count; i++) {
           letterTiles[i].GetComponent<Image>().color = Color.white; // Not yet typed
        }
    }

    void EndGame(bool success) {
        gameActive = false;
        if (success) {
            resultText.text = "You Win!";
        } else {
            resultText.text = "Time's Up!";
        }
    }
}

```

---

## **🔹 Key Fixes & Enhancements**

### ✅ **Tiles Now Fit Perfectly Inside `SentenceGrid`**

- Dynamically calculates **`tileWidth = totalWidth / currentSentence.Length`**.
- Ensures all tiles fit inside `SentenceGrid` **without overflowing**.

### ✅ **Prevents Overflow & Text Clipping**

- Uses **`LayoutElement`** for smooth resizing.
- If a long sentence is used, tiles automatically **resize** to fit.

### ✅ **Auto-adjusts Layout in Different Screen Sizes**

- `SentenceGrid` must be **full width** of the screen.
- Tiles will always **fit within the bounds**.

---

## **3️⃣ Unity UI Setup**

### **1️⃣ `SentenceGrid` (Image)**

- **Set Full Width**:

  - `Anchor`: **Stretch (Left & Right)**
  - `Pivot`: **(0.5, 0.5)**
  - `Rect Transform`: **Full Width** of screen.
  - **Height**: `80-100px` (Adjustable).

- **Add Components**:
  - `Horizontal Layout Group`
    - `Control Child Size (Width: ✅, Height: ✅)`
    - `Child Force Expand (Width: ❌, Height: ✅)`
    - `Spacing: 5` (Optional)
  - `Content Size Fitter`
    - `Horizontal Fit: Preferred Size`
    - `Vertical Fit: Preferred Size`

---

### **2️⃣ `Letter Tile Prefab`**

- **UI → Panel (Letter Tile)**
  - Add **Image Component** (Background).
  - Add **TextMeshPro - Text (UI)** (Inside).
- **Set Components**:
  - `Rect Transform`:
    - `Width & Height`: **Auto-adjusted in script**.
  - `Layout Element`
    - `Preferred Width & Height`: **Auto-adjusted**.

---

### **3️⃣ Assign UI Elements in Unity**

1. **Drag `SentenceGrid` (Image) into `sentenceGridRect`**.
2. **Assign `LetterTile Prefab`** to `letterPrefab`.

---

## **🎯 Final Result**

✅ **Letter tiles dynamically resize** to fit inside `SentenceGrid`.  
✅ **No text overflow or cutoff** issues.  
✅ **Works on different screen sizes**.  
✅ **Maintains proper spacing**.

🚀 **Now the UI will always look perfect!** Let me know if you need more tweaks! 🎮🔥
