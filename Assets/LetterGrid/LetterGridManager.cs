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

    [Header("⚖️ Placement Balancing")]
    [Tooltip("Minimum number of words that must be placed on diagonals (↘ ↖ ↗ ↙) each round, if possible.")]
    [SerializeField] private int minDiagonalWords = 2;

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

    // 8 directions in row-major indexing (x=row increases downward, y=col increases rightward)
    private static readonly Vector2Int[] AllDirs = {
        new Vector2Int( 0,  1), // →  right
        //new Vector2Int( 0, -1), // ←  left
        new Vector2Int( 1,  0), // ↓  down
        //new Vector2Int(-1,  0), // ↑  up
        new Vector2Int( 1,  1), // ↘  down-right
        //new Vector2Int(-1, -1), // ↖  up-left
        new Vector2Int(-1,  1), // ↗  up-right
        //new Vector2Int( 1, -1), // ↙  down-left
    };

    private void Awake() { }

    private void Start() { }

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
                letterGrid[i, j] = new LetterData(i, j);
            }
        }

        // 🧩 Pick valid words we want to place this round
        int targetCount = Random.Range(LetterGridGameManager.Instance.minWordsToPlace,
                                       LetterGridGameManager.Instance.maxWordsToPlace + 1);

        // Filter candidates by length
        List<string> candidates = new();
        foreach (var w in validWords) {
            if (w.Length >= LetterGridGameManager.Instance.minWordLength &&
                w.Length <= LetterGridGameManager.Instance.maxWordLength) {
                candidates.Add(w.ToUpper());
            }
        }
        Shuffle(candidates);

        // Even split per direction across all 8 directions
        int numDirs = AllDirs.Length;
        int basePerDir = targetCount / numDirs;
        int extra = targetCount % numDirs; // distribute the remainder

        int[] quotas = new int[numDirs];
        for (int i = 0; i < numDirs; i++)
            quotas[i] = basePerDir + (i < extra ? 1 : 0);

        // 👉 Enforce a minimum number of diagonal words (indices 4..7)
        EnforceMinimumDiagonal(AllDirs, quotas, Mathf.Min(minDiagonalWords, targetCount), targetCount);

        HashSet<string> used = new();
        List<string> successfullyPlaced = new();

        // Direction debug counters
        int cRight = 0, cLeft = 0, cDown = 0, cUp = 0, cDR = 0, cUL = 0, cUR = 0, cDL = 0;

        void BumpDirCounter(Vector2Int dir) {
            if (dir == new Vector2Int(0, 1)) cRight++;
            else if (dir == new Vector2Int(0, -1)) cLeft++;
            else if (dir == new Vector2Int(1, 0)) cDown++;
            else if (dir == new Vector2Int(-1, 0)) cUp++;
            else if (dir == new Vector2Int(1, 1)) cDR++;
            else if (dir == new Vector2Int(-1, -1)) cUL++;
            else if (dir == new Vector2Int(-1, 1)) cUR++;
            else if (dir == new Vector2Int(1, -1)) cDL++;
        }

        // Pass 1: meet quotas per direction — overlap-first, then empty-path
        for (int d = 0; d < numDirs; d++) {
            int need = quotas[d];
            if (need <= 0) continue;

            Vector2Int dir = AllDirs[d];

            for (int i = 0; i < candidates.Count && need > 0; i++) {
                string word = candidates[i];
                if (used.Contains(word)) continue;

                bool placed =
                    TryPlaceWordWithOverlapInDirection(word, dir) ||   // prefer crossing
                    TryPlaceWordSpecificDirection(word, dir);          // fall back to empty path

                if (placed) {
                    used.Add(word);
                    successfullyPlaced.Add(word);
                    need--;
                    BumpDirCounter(dir);
                }
            }
        }

        // Pass 2 (optional): if we didn’t meet targetCount (tight boards), try any direction
        for (int i = 0; i < candidates.Count && successfullyPlaced.Count < targetCount; i++) {
            string word = candidates[i];
            if (used.Contains(word)) continue;

            bool placed = false;
            Vector2Int placedDir = default;

            // overlap-first across all dirs
            foreach (var dir in AllDirs) {
                if (TryPlaceWordWithOverlapInDirection(word, dir)) { placed = true; placedDir = dir; break; }
            }
            // then empty-path across all dirs
            if (!placed) {
                foreach (var dir in AllDirs) {
                    if (TryPlaceWordSpecificDirection(word, dir)) { placed = true; placedDir = dir; break; }
                }
            }

            if (placed) {
                used.Add(word);
                successfullyPlaced.Add(word);
                BumpDirCounter(placedDir);
            }
        }

        placedWords = new List<string>(successfullyPlaced);

        // ✅ Debug: words + per-direction counts
        Debug.Log($"✅ Placed ({placedWords.Count}): {string.Join(", ", placedWords)}");
        Debug.Log($"Directions →{cRight} ←{cLeft} ↓{cDown} ↑{cUp} ↘{cDR} ↖{cUL} ↗{cUR} ↙{cDL}");

        // 🔠 Fill empty spots with random letters (retry if a banned word sneaks in)
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
                    letterGrid[i, j].Flag == LetterData.LetterFlag.InWord) {
                    letterTile.SetCurrentColor(wordTileColor);
                }
            }
        }
    }

    /// <summary>
    /// Adjusts quotas so that the total assigned to diagonal directions (abs(dx)==1 && abs(dy)==1)
    /// is at least minDiag, without changing the overall sum. It pulls from non-diagonal buckets
    /// with the largest quotas. Safe for any size/contents of AllDirs (0..8).
    /// </summary>
    private void EnforceMinimumDiagonal(Vector2Int[] dirs, int[] quotas, int minDiag, int totalTarget) {
        if (dirs == null || quotas == null || quotas.Length != dirs.Length) return;
        if (totalTarget <= 0 || minDiag <= 0) return;

        // Identify diagonal and non-diagonal indices dynamically.
        List<int> diag = new List<int>();
        List<int> nonDiag = new List<int>();
        for (int i = 0; i < dirs.Length; i++) {
            bool isDiagonal = Mathf.Abs(dirs[i].x) == 1 && Mathf.Abs(dirs[i].y) == 1;
            if (isDiagonal) diag.Add(i); else nonDiag.Add(i);
        }

        // If there are no diagonals in the active set, nothing to enforce.
        if (diag.Count == 0) return;

        // Clamp requested minimum to overall target.
        minDiag = Mathf.Min(minDiag, totalTarget);

        // Current diagonal sum
        int diagSum = 0;
        foreach (var idx in diag) diagSum += quotas[idx];
        if (diagSum >= minDiag) return; // already satisfied

        int deficit = minDiag - diagSum;

        // Move quota from non-diagonals with largest buckets to diagonals with smallest buckets.
        while (deficit > 0) {
            // Find best donor among non-diagonals
            int donor = -1; int donorVal = -1;
            foreach (var i in nonDiag) {
                if (quotas[i] > donorVal) { donorVal = quotas[i]; donor = i; }
            }
            if (donor == -1 || donorVal <= 0) break; // nothing left to steal

            // Find best recipient among diagonals (the one with smallest current quota)
            int recipient = -1; int recipientVal = int.MaxValue;
            foreach (var i in diag) {
                if (quotas[i] < recipientVal) { recipientVal = quotas[i]; recipient = i; }
            }
            if (recipient == -1) break; // should not happen

            quotas[donor]--;
            quotas[recipient]++;
            deficit--;
        }

        // Safety: keep total equal to target if rounding/stealing failed to match perfectly.
        int sum = 0; for (int i = 0; i < quotas.Length; i++) sum += quotas[i];
        int diff = totalTarget - sum;
        if (diff != 0) {
            if (diff > 0) {
                // Add leftover to diagonals first (or anywhere if no diagonals somehow)
                var pool = diag.Count > 0 ? diag : nonDiag;
                int p = 0;
                while (diff > 0 && pool.Count > 0) { quotas[pool[p % pool.Count]]++; diff--; p++; }
            }
            else {
                // Remove extras from non-diagonals first (or diagonals if none)
                var pool = nonDiag.Count > 0 ? nonDiag : diag;
                int p = 0;
                while (diff < 0 && pool.Count > 0) {
                    int idx = pool[p % pool.Count];
                    if (quotas[idx] > 0) { quotas[idx]--; diff++; }
                    p++;
                }
            }
        }
    }


    private bool TryPlaceWordWithOverlapInDirection(string word, Vector2Int dir) {
        // Scan the board for existing letters that can be used as an anchor
        // Only consider cells that are NOT random (i.e., already set by previous words)
        List<(Vector2Int start, int k)> starts = new();

        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                var cell = letterGrid[x, y];
                if (cell.Flag == LetterData.LetterFlag.Random) continue; // nothing to overlap with
                char gridC = cell.TileLetter;

                // Try to align each matching letter index of the word to this cell
                for (int k = 0; k < word.Length; k++) {
                    if (word[k] != gridC) continue;

                    int sx = x - dir.x * k;
                    int sy = y - dir.y * k;

                    if (IsPathValid(word, sx, sy, dir, requireAtLeastOneOverlap: true)) {
                        starts.Add((new Vector2Int(sx, sy), k));
                    }
                }
            }
        }

        if (starts.Count == 0) return false;
        Shuffle(starts);

        // Place at the first valid start
        foreach (var (start, _) in starts) {
            if (PlaceWord(word, start, dir)) {
                return true;
            }
        }
        return false;
    }

    private bool TryPlaceWordSpecificDirection(string word, Vector2Int dir) {
        // Generate all valid start positions for this direction
        List<Vector2Int> starts = new();
        GetValidStartsForDirection(word.Length, dir, starts);
        Shuffle(starts);

        foreach (var start in starts) {
            if (IsPathValid(word, start.x, start.y, dir, requireAtLeastOneOverlap: false)) {
                if (PlaceWord(word, start, dir)) return true;
            }
        }
        return false;
    }

    // ✔ Handles negative steps (dx/dy = -1) correctly
    private void GetValidStartsForDirection(int wordLen, Vector2Int dir, List<Vector2Int> outList) {
        outList.Clear();
        for (int x = 0; x < gridSize; x++) {
            for (int y = 0; y < gridSize; y++) {
                int ex = x + dir.x * (wordLen - 1);
                int ey = y + dir.y * (wordLen - 1);
                if (ex >= 0 && ex < gridSize && ey >= 0 && ey < gridSize) {
                    outList.Add(new Vector2Int(x, y));
                }
            }
        }
    }


    private bool IsPathValid(string word, int sx, int sy, Vector2Int dir, bool requireAtLeastOneOverlap) {
        bool overlapped = false;
        int x = sx, y = sy;

        for (int i = 0; i < word.Length; i++) {
            // bounds
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) return false;

            var cell = letterGrid[x, y];
            if (cell.Flag != LetterData.LetterFlag.Random) {
                if (cell.TileLetter != word[i]) return false; // conflicting letter
                overlapped = true;
            }

            x += dir.x; y += dir.y;
        }
        return !requireAtLeastOneOverlap || overlapped;
    }

    private bool PlaceWord(string word, Vector2Int start, Vector2Int dir) {
        // After validation, actually stamp letters & flags
        int x = start.x, y = start.y;
        for (int i = 0; i < word.Length; i++) {
            letterGrid[x, y].SetLetter(word[i]);
            letterGrid[x, y].Flag = LetterData.LetterFlag.InWord;
            x += dir.x; y += dir.y;
        }
        return true;
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

    private void Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
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

    // TO DO : Dont Call this every time you StartGridManager (only when validWords has no elements and gridSize gets bigger)
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
