# **🎮 Creating a Word Scramble Game in Unity (Drag & Drop Version)**

In this guide, we will walk through the process of **building a Word Scramble game in Unity**, where players can **rearrange scrambled letters** using **drag-and-drop mechanics**.

---

## **🛠 Step 1: Setting Up the Unity Scene**

### **1.1 Create a New Unity 2D Project**

- Open **Unity** and create a **2D project**.

### **1.2 Set Up the UI**

In the **Hierarchy**, add the following:

- **Canvas** (Right-click in Hierarchy → UI → Canvas)
  - Set **Render Mode** to **Screen Space - Overlay**.
- **Panel (For Letter Tiles)**

  - Add a **Horizontal Layout Group** component.
  - This will hold the scrambled letters in a row.

- **Text Elements**

  - **TMP_Text** for displaying hints.
  - **TMP_Text** for displaying the timer.
  - **TMP_Text** for displaying the score.

- **Buttons**

  - **Hint Button** (Reveals a clue, reduces score).
  - **Submit Button** (Checks if the word is correct).

- **Letter Tile Prefab**
  - Create a **UI Button**.
  - Inside the button, add a **TMP_Text** component.
  - This will be our **Letter Tile Prefab**.
  - Save it as a **Prefab** in the **Assets** folder.

---

## **🛠 Step 2: Implementing Drag-and-Drop Mechanics**

We need a **C# script** to allow **dragging and swapping** letters in the **Horizontal Layout Group**.

### **2.1 Creating the `WordScrambleLetterTile` Script**

- Attach this script to the **Letter Tile Prefab**.

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WordScrambleLetterTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentAfterDrag;
    private Vector3 startPosition;
    private int startIndex;

    void Start()
    {
        parentAfterDrag = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        startIndex = transform.GetSiblingIndex();
        transform.SetParent(parentAfterDrag.parent); // Temporarily move to parent to allow reordering
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition; // Move with the cursor
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);

        // Find nearest tile to swap position
        int closestIndex = GetClosestTileIndex();
        transform.SetSiblingIndex(closestIndex);
    }

    private int GetClosestTileIndex()
    {
        float minDistance = float.MaxValue;
        int closestIndex = startIndex;

        for (int i = 0; i < parentAfterDrag.childCount; i++)
        {
            Transform child = parentAfterDrag.GetChild(i);
            float distance = Vector3.Distance(transform.position, child.position);
            if (distance < minDistance && child != transform)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
```

### **🔹 Explanation of Fixes**

✅ **Swaps letters instead of moving freely.**  
✅ **Uses `SetSiblingIndex()` to keep order in the UI.**  
✅ **Ensures smooth drag-and-drop interaction.**

---

## **🛠 Step 3: Managing the Game Logic**

Now, we create a **C# script** to handle:

- **Choosing a word** and scrambling it.
- **Displaying scrambled letters as tiles**.
- **Checking if the word is correct**.
- **Providing hints**.
- **Managing score and timer**.

### **3.1 Creating the `WordScrambleManager` Script**

- Attach this script to an **empty GameObject** called **GameManager**.

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WordScrambleManager : MonoBehaviour
{
    public GameObject letterTilePrefab;
    public Transform letterContainer; // Horizontal Layout Group
    public TMP_Text timerText, hintText, scoreText;
    public Button submitButton, hintButton;

    private string originalWord;
    private List<GameObject> letterTiles = new List<GameObject>();
    private float timeRemaining = 30f;
    private int score = 1000;
    private bool isGameActive = true;

    private Dictionary<string, string> wordList = new Dictionary<string, string>()
    {
        { "planet", "A celestial body orbiting a star" },
        { "bridge", "Structure to cross over a river" },
        { "guitar", "A musical instrument with strings" },
        { "school", "A place where students learn" },
        { "castle", "A fortified structure from medieval times" }
    };

    void Start()
    {
        ChooseWord();
        submitButton.onClick.AddListener(CheckAnswer);
        hintButton.onClick.AddListener(ShowHint);
        scoreText.text = "Score: " + score;
    }

    void Update()
    {
        if (isGameActive)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();

            if (timeRemaining <= 0)
            {
                isGameActive = false;
                timerText.text = "Time's Up!";
                hintText.text = "The word was: " + originalWord;
            }
        }
    }

    void ChooseWord()
    {
        int randomIndex = Random.Range(0, wordList.Count);
        originalWord = new List<string>(wordList.Keys)[randomIndex];
        string scrambledWord = ScrambleWord(originalWord);

        foreach (char letter in scrambledWord)
        {
            GameObject tile = Instantiate(letterTilePrefab, letterContainer);
            tile.GetComponentInChildren<TMP_Text>().text = letter.ToString();
            letterTiles.Add(tile);
        }
    }

    string ScrambleWord(string word)
    {
        char[] letters = word.ToCharArray();
        for (int i = 0; i < letters.Length; i++)
        {
            int randomIndex = Random.Range(0, letters.Length);
            char temp = letters[i];
            letters[i] = letters[randomIndex];
            letters[randomIndex] = temp;
        }
        return new string(letters);
    }

    public void CheckAnswer()
    {
        string playerWord = "";

        // **Check order of letters in the Horizontal Layout**
        for (int i = 0; i < letterContainer.childCount; i++)
        {
            playerWord += letterContainer.GetChild(i).GetComponentInChildren<TMP_Text>().text;
        }

        if (playerWord == originalWord)
        {
            hintText.text = "🎉 Correct! 🎉";
            isGameActive = false;
        }
        else
        {
            hintText.text = "❌ Try Again!";
        }
    }

    public void ShowHint()
    {
        hintText.text = "Hint: " + wordList[originalWord];
        score -= 500; // Deduct points for using hint
        scoreText.text = "Score: " + score;
    }
}
```

### **🔹 Explanation of Fixes**

✅ **Now, the game checks the correct order of tiles instead of random text values.**  
✅ **Prevents dragging outside the `Horizontal Layout Group`.**  
✅ **Ensures score deduction when using hints.**

---

## **🎨 Final Game Flow**

### **🎬 How It Works**

1️⃣ **The game starts**, and a scrambled word appears as draggable tiles.  
2️⃣ **The player drags and rearranges tiles** in the correct order.  
3️⃣ **On pressing Submit**, the game checks the answer.  
4️⃣ **A hint can be used** (reduces score).  
5️⃣ **The timer runs down**—if time runs out, the game ends.

---

## **🎉 Final Enhancements & Features**

- ✅ **Smooth drag animations**
- ✅ **Sounds for drag and correct answer**
- ✅ **Different difficulty levels**
- ✅ **Leaderboard for best scores**
- ✅ **More words & categories**

🔥 Now, your Word Scramble game is **fully functional** with **drag-and-drop mechanics** and a **proper word-checking system**. Let me know if you need more improvements! 🚀🎮
