using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridManager : MonoBehaviour {

    [Header("📦 Grid Setup")]
    public Transform gridParent;
    public int gridSize = 4;

    [HideInInspector]
    public List<string> placedWords = new();

    private LetterData[,] letterGrid;
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int maxGridAttempts = 10;

    [Header("🎨 Tile Colors")]
    public Color baseColor = Color.white;
    public Color correctColor = Color.green;
    public Color selectedColor = Color.yellow;

    [Header("🛠 Gameplay Settings")]
    public bool highlightWordTiles = true;
    public Color wordTileColor = new Color(0.5f, 0f, 0.8f, 1f);

    public HashSet<string> validWords = new HashSet<string>();
    public HashSet<string> foundWords = new HashSet<string>();

    // List of words to exclude (case-insensitive)
    public HashSet<string> bannedWords = new HashSet<string> {
            // Violence / Weapons
            "KILL", "GUN", "BOMB", "WAR", "FIGHT", "SHOOT", "MURDER", "DEATH", "VIOLENCE", "TERROR", "ATTACK",

            // Drugs / Substances
            "DRUG", "WEED", "COCAINE", "HEROIN", "ALCOHOL", "VODKA", "BEER", "METH", "CIGARETTE", "SMOKE", "POT",

            // Sexual Content
            "SEX", "NUDE", "PORN", "NAKED", "RAPE", "MOLEST", "ORGASM", "BDSM", "XXX", "VAGINA", "PENIS", "BREAST",

            // Hate / Discrimination
            "NAZI", "HITLER", "RACIST", "RACISM", "HATE", "KLAN", "BIGOT", "SLAVE",

            // Suicide / Self-harm
            "SUICIDE", "SELFHARM", "DEPRESS", "CUTTING", "OVERDOSE", "HANG", "DIE",

            // Profanity / Inappropriate language
            "HELL", "DAMN", "CRAP", "SHIT", "FUCK", "BITCH", "BASTARD", "ASS", "DICK", "PISS", "COCK", "CUM",

            // Other (contextually sensitive)
            "GAMBLE", "CASINO", "SATAN", "DEVIL", "OCCULT", "WITCH", "CURSE"
    };


    private void Awake() {
    }

    private void Start() {
        

    }

    public void StartGridManager() {
        LoadDictionary();
        foundWords.Clear();
        UpdateGridLayout();
        ClearGridToPool();
        GenerateGrid();
    }

    private void OnRectTransformDimensionsChange() {
        UpdateGridLayout();
    }


    public void UpdateGridLayout() {
        GridLayoutGroup gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        RectTransform panelRect = gridParent.parent.GetComponent<RectTransform>();

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridSize;

        float width = panelRect.rect.width;
        float height = panelRect.rect.height;

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        float totalSpacingX = spacingX * (gridSize - 1);
        float totalSpacingY = spacingY * (gridSize - 1);

        float availableWidth = width - gridLayout.padding.left - gridLayout.padding.right - totalSpacingX;
        float availableHeight = height - gridLayout.padding.top - gridLayout.padding.bottom - totalSpacingY;

        float cellSize = Mathf.Min(availableWidth, availableHeight) / gridSize;

        gridLayout.cellSize = new Vector2(cellSize, cellSize);
    }

    private void GenerateGrid() {
        letterGrid = new LetterData[gridSize, gridSize];
        // Initialize grid with empty characters
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                letterGrid[i, j] = new LetterData(i,j);
            }
        }


        // 🧩 Pick valid words and try to place them
        List<string> wordsToPlace = GetWordsForGrid(LetterGridGameManager.Instance.minWordsToPlace, LetterGridGameManager.Instance.maxWordsToPlace);
        List<string> successfullyPlaced = new();

        foreach (string word in wordsToPlace) {
            if (TryPlaceWord(word.ToUpper())) {
                successfullyPlaced.Add(word);
            }
        }

        placedWords = new List<string>(successfullyPlaced);

        // 🔠 Fill empty spots with random letters
        // Place words as usual, then:
        bool safe = false;
        int attempts = 0;

        while (!safe && attempts < maxGridAttempts) {
            // Fill random letters only
            for (int i = 0; i < gridSize; i++) {
                for (int j = 0; j < gridSize; j++) {
                    if (letterGrid[i, j].Flag == LetterData.LetterFlag.Random) {
                        letterGrid[i, j].SetLetter(GetRandomLetter());
                    }
                }
            }

            if (!GridContainsBannedWord()) {
                safe = true;
            }
            else {
                attempts++;
                Debug.LogWarning($"⚠️ Banned word found in random fill. Retrying randoms... (Attempt {attempts}/{maxGridAttempts})");
            }
        }

        //if (!safe) {
        //    Debug.LogError("❌ Max attempts for safe random letter fill reached. Consider adjusting banned word list or grid size.");
        //    return;
        //}

        // 🧱 Instantiate letter tiles
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {           
                GameObject tile = LetterTilePool.Instance.GetTile(gridParent);
                CanvasGroup group = tile.GetComponent<CanvasGroup>();
                if (group == null) group = tile.AddComponent<CanvasGroup>();
                group.alpha = 0f;

                tile.GetComponentInChildren<TMP_Text>().text = letterGrid[i, j].TileLetter.ToString();

                LetterGridLetterTile letterTile = tile.GetComponent<LetterGridLetterTile>();
                letterTile.LetterData = letterGrid[i, j]; // Use prepared LetterData
                letterTile.SetCurrentColor(baseColor);

                if (highlightWordTiles &&
                    letterGrid[i,j].Flag==LetterData.LetterFlag.InWord) {
                    letterTile.SetCurrentColor(wordTileColor);
                }
            }
        }

        //Debug.Log("✅ Successfully placed words: " + string.Join(", ", successfullyPlaced));
    }

    public void ClearGridToPool() {
        // Create a temp list so we don't modify the collection while iterating
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in gridParent)
            children.Add(child.gameObject);

        foreach (var tile in children)
            LetterTilePool.Instance.ReturnTile(tile);
    }


    public void ResetTilesTriggerArea() {
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                letterGrid[i, j].ResetTriggerAreaPercentage();
            }
        }
    }

    public void SmallerTilesTriggerArea() {
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                letterGrid[i, j].SetSmallerTriggerAreaPercentage();
            }
        }
    }


    private bool TryPlaceWord(string word) {
        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(1, -1)
        };

        for (int attempt = 0; attempt < 100; attempt++) {
            Vector2Int startPos = new Vector2Int(Random.Range(0, gridSize), Random.Range(0, gridSize));
            List<Vector2Int> shuffledDirections = new(directions);
            Shuffle(shuffledDirections);

            foreach (Vector2Int dir in shuffledDirections) {
                Vector2Int endPos = startPos + dir * (word.Length - 1);
                if (endPos.x < 0 || endPos.x >= gridSize || endPos.y < 0 || endPos.y >= gridSize)
                    continue;

                Vector2Int pos = startPos;
                bool canPlace = true;

                foreach (char c in word) {
                    if (letterGrid[pos.x, pos.y].Flag != LetterData.LetterFlag.Random && letterGrid[pos.x, pos.y].TileLetter != c) {
                        canPlace = false;
                        break;
                    }
                    pos += dir;
                }

                if (!canPlace) continue;

                pos = startPos;
                foreach (char c in word) {
                    letterGrid[pos.x, pos.y].SetLetter(c);
                    letterGrid[pos.x, pos.y].Flag= LetterData.LetterFlag.InWord;
                    pos += dir;
                }
                return true;
            }
        }
        return false;
    }

    private void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    private List<string> GetWordsForGrid(int minWords, int maxWords) {
        List<string> words = new();
        int wordCount = Random.Range(minWords, maxWords + 1);

        List<string> candidates = new();
        foreach (string word in validWords) {
            if (word.Length >= LetterGridGameManager.Instance.minWordLength && word.Length <= LetterGridGameManager.Instance.maxWordLength) {
                candidates.Add(word);
            }
        }

        for (int i = 0; i < wordCount && candidates.Count > 0; i++) {
            int index = Random.Range(0, candidates.Count);
            words.Add(candidates[index]);
            candidates.RemoveAt(index);
        }
        return words;
    }
    public LetterGridLetterTile GetTileAt(int x, int y) {
        int index = x * gridSize + y;
        if (index < 0 || index >= gridParent.childCount)
            return null;
        return gridParent.GetChild(index).GetComponent<LetterGridLetterTile>();
    }

    private char GetRandomLetter() {
        return alphabet[Random.Range(0, alphabet.Length)];
    }

    private bool GridContainsBannedWord() {
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                for (int d = 0; d < 8; d++) {
                    string line = "";

                    int cx = x;
                    int cy = y;

                    for (int len = 0; len < gridSize; len++) {
                        if (cx < 0 || cx >= gridSize || cy < 0 || cy >= gridSize)
                            break;

                        line += letterGrid[cx, cy].TileLetter;

                        string upperLine = line.ToUpper();
                        string reversedLine = ReverseString(upperLine);

                        // For every banned word, check if the last N letters match, where N is the length of the banned word
                        foreach (string banned in bannedWords) {
                            int n = banned.Length;
                            if (line.Length >= n) {
                                string lastN = line.Substring(line.Length - n, n).ToUpper();
                                string lastNReversed = ReverseString(lastN);
                                if (lastN == banned || lastNReversed == banned) {
                                    Debug.LogWarning($"🚫 Found banned word '{banned}' in grid: {lastN}");
                                    return true;
                                }
                            }
                        }

                        cx += dx[d];
                        cy += dy[d];
                    }
                }
            }
        }

        return false;
    }

    private string ReverseString(string input) {
        char[] arr = input.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }
    //TO DO : Dont Call this every time you StartGridManager(only when validword has no elemnts and gridsize gets bigger )
    private void LoadDictionary() {
        TextAsset wordFile = Resources.Load<TextAsset>("LetterGrid/wordlist");
        if (wordFile == null) {
            Debug.LogError("wordlist.txt not found in Resources folder!");
            return;
        }


        string[] words = wordFile.text.Split('\n');
        HashSet<string> filteredWords = new HashSet<string>();

        foreach (string word in words) {
            string cleanWord = word.Trim().ToUpper();

            if (cleanWord.Length >= 3 &&
                cleanWord.Length <= gridSize &&
                !bannedWords.Contains(cleanWord)) {

                filteredWords.Add(cleanWord);
            }
        }

        validWords = filteredWords;
        Debug.Log($"[Dictionary] Loaded {validWords.Count} valid family-safe words.");
    }

}
