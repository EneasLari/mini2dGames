using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridManager : MonoBehaviour {

    [Header("📦 Grid Setup")]
    public int GridSizeX = 4;
    public int GridSizeY = 4;

    [System.Flags]
    public enum DirectionMask {
        Right = 1 << 0,  // (0,  1)
        Left = 1 << 1,  // (0, -1)
        Down = 1 << 2,  // (1,  0)
        Up = 1 << 3,  // (-1, 0)
        DownRight = 1 << 4,  // (1,  1)
        UpLeft = 1 << 5,  // (-1, -1)
        UpRight = 1 << 6,  // (-1, 1)
        DownLeft = 1 << 7,  // (1, -1)
    }

    [SerializeField]
    private DirectionMask enabledDirections =  DirectionMask.Right | DirectionMask.Down | DirectionMask.DownRight | DirectionMask.UpRight;



    [HideInInspector]
    public List<string> placedWords = new();

    private LetterData[,] letterGrid;
    private readonly string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int maxGridAttempts = 10;

    [Header("⚖️ Placement Balancing")]
    [Tooltip("Minimum number of words that must be placed on diagonals (↘ ↖ ↗ ↙) each round, if possible.")]
    [SerializeField] private int minDiagonalWords = 2;

    public HashSet<string> validWords = new HashSet<string>();
    public HashSet<string> foundWords = new HashSet<string>();

    // Cache uppercased banned words and max length once.
    private HashSet<string> bannedUpper = new();
    private int maxBannedLen = 0;

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


    private static readonly Dictionary<DirectionMask, Vector2Int> DirMap = new() {
        { DirectionMask.Right,     new Vector2Int( 0,  1) },
        { DirectionMask.Left,      new Vector2Int( 0, -1) },
        { DirectionMask.Down,      new Vector2Int( 1,  0) },
        { DirectionMask.Up,        new Vector2Int(-1,  0) },
        { DirectionMask.DownRight, new Vector2Int( 1,  1) },
        { DirectionMask.UpLeft,    new Vector2Int(-1, -1) },
        { DirectionMask.UpRight,   new Vector2Int(-1,  1) },
        { DirectionMask.DownLeft,  new Vector2Int( 1, -1) },
    };

    private Vector2Int[] GetActiveDirections() {
        var list = new List<Vector2Int>(8);
        foreach (var kv in DirMap) {
            if ((enabledDirections & kv.Key) != 0)
                list.Add(kv.Value);
        }
        return list.ToArray();
    }



    private void Awake() { }

    private void Start() { }



    public LetterData[,] SetupGrid() {
        LoadDictionary();
        BuildBannedCaches();
        foundWords.Clear();
        return GenerateGrid();
    }

    private LetterData[,] GenerateGrid() {
        // --- set up board ---
        letterGrid = new LetterData[GridSizeX, GridSizeY];
        for (int i = 0; i < GridSizeX; i++) {
            for (int j = 0; j < GridSizeY; j++) {
                letterGrid[i, j] = new LetterData(i, j);
            }
        }

        // --- pick how many words we want this round ---
        int targetCount = Random.Range(
            LetterGridGameManager.Instance.minWordsToPlace,
            LetterGridGameManager.Instance.maxWordsToPlace + 1
        );

        // --- candidate pool filtered by length ---
        List<string> candidates = new();
        foreach (var w in validWords) {
            if (w.Length >= LetterGridGameManager.Instance.minWordLength &&
                w.Length <= LetterGridGameManager.Instance.maxWordLength) {
                candidates.Add(w.ToUpperInvariant());
            }
        }
        Shuffle(candidates);

        // If we somehow have fewer candidates than target, cap it.
        targetCount = Mathf.Min(targetCount, candidates.Count);

        // --- active directions from inspector ---
        var activeDirs = GetActiveDirections();
        if (activeDirs == null || activeDirs.Length == 0) {
            Debug.LogWarning("[Grid] No active directions selected – falling back to → (right).");
            activeDirs = new[] { new Vector2Int(0, 1) };
        }

        // --- split a quota per direction ---
        int numDirs = activeDirs.Length;
        int basePerDir = (numDirs > 0) ? targetCount / numDirs : 0;
        int extra = (numDirs > 0) ? targetCount % numDirs : 0;

        int[] quotas = new int[numDirs];
        for (int i = 0; i < numDirs; i++)
            quotas[i] = basePerDir + (i < extra ? 1 : 0);

        // Ensure a minimum number of diagonals from the actually active set.
        EnforceMinimumDiagonal(activeDirs, quotas, Mathf.Min(minDiagonalWords, targetCount), targetCount);

        // bookkeeping
        HashSet<string> used = new();
        List<string> successfullyPlaced = new();

        // per-direction debug counts
        var dirCounts = new Dictionary<Vector2Int, int>();
        foreach (var d in activeDirs) dirCounts[d] = 0;

        void BumpDirCounter(Vector2Int d) {
            if (!dirCounts.ContainsKey(d)) dirCounts[d] = 0;
            dirCounts[d]++;
        }

        // --- Pass 1: satisfy per-direction quotas (prefer overlaps, then empty paths) ---
        for (int d = 0; d < numDirs; d++) {
            int need = quotas[d];
            if (need <= 0) continue;

            Vector2Int dir = activeDirs[d];

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

        // --- Pass 2: if still short, try any direction (same overlap-first policy) ---
        for (int i = 0; i < candidates.Count && successfullyPlaced.Count < targetCount; i++) {
            string word = candidates[i];
            if (used.Contains(word)) continue;

            bool placed = false;
            Vector2Int placedDir = default;

            foreach (var dir in activeDirs) {
                if (TryPlaceWordWithOverlapInDirection(word, dir)) {
                    placed = true; placedDir = dir; break;
                }
            }
            if (!placed) {
                foreach (var dir in activeDirs) {
                    if (TryPlaceWordSpecificDirection(word, dir)) {
                        placed = true; placedDir = dir; break;
                    }
                }
            }

            if (placed) {
                used.Add(word);
                successfullyPlaced.Add(word);
                BumpDirCounter(placedDir);
            }
        }

        placedWords = new List<string>(successfullyPlaced);

        // --- Debug summary ---
        Debug.Log($"✅ Placed ({placedWords.Count}): {string.Join(", ", placedWords)}");
        if (dirCounts.Count > 0) {
            // compact per-direction dump
            var parts = new List<string>();
            foreach (var kv in dirCounts) {
                var d = kv.Key;
                parts.Add($"({d.x},{d.y})={kv.Value}");
            }
            Debug.Log("[Dirs] " + string.Join("  ", parts));
        }

        // --- Fill remaining cells with random letters, retry if a banned word appears ---
        bool safe = false;
        int attempts = 0;
        while (!safe && attempts < maxGridAttempts) {
            for (int i = 0; i < GridSizeX; i++) {
                for (int j = 0; j < GridSizeY; j++) {
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

        return letterGrid;
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

        for (int x = 0; x < GridSizeX; x++) {
            for (int y = 0; y < GridSizeY; y++) {
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
            // ✅ new guard before placement
            if (PlacementIntroducesBanned(word, start, dir))
                continue; // skip this start if it would create a banned substring
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
            if (!IsPathValid(word, start.x, start.y, dir, requireAtLeastOneOverlap: false))
                continue;
            if (PlacementIntroducesBanned(word, start, dir))
                continue;
            if (PlaceWord(word, start, dir)) return true;
        }
        return false;
    }

    // ✔ Handles negative steps (dx/dy = -1) correctly
    private void GetValidStartsForDirection(int wordLen, Vector2Int dir, List<Vector2Int> outList) {
        outList.Clear();
        for (int x = 0; x < GridSizeX; x++) {
            for (int y = 0; y < GridSizeY; y++) {
                int ex = x + dir.x * (wordLen - 1);
                int ey = y + dir.y * (wordLen - 1);
                if (ex >= 0 && ex < GridSizeX && ey >= 0 && ey < GridSizeY) {
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
            if (x < 0 || x >= GridSizeX || y < 0 || y >= GridSizeY) return false;

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

    private char GetCharAfterPlacement(int x, int y, string word, Vector2Int start, Vector2Int dir, out bool isInPlacement) {
        // Check if (x,y) lies on the proposed word path.
        int dx = x - start.x;
        int dy = y - start.y;

        // Same line? (parallel to dir) and aligned?
        isInPlacement = false;

        // Quick reject if not collinear with dir (avoid div-by-zero)
        if ((dir.x == 0 && dx != 0) || (dir.y == 0 && dy != 0)) {
            // if dir.x == 0, must move only in y; if dir.y == 0, must move only in x
        }
        else {
            int step = -1;
            if (dir.x != 0 && dx % dir.x == 0) step = dx / dir.x;
            else if (dir.y != 0 && dy % dir.y == 0) step = dy / dir.y;

            if (step >= 0 && step < word.Length) {
                // confirm other axis lines up too
                int expY = start.y + dir.y * step;
                int expX = start.x + dir.x * step;
                if (expX == x && expY == y) {
                    isInPlacement = true;
                    return word[step];
                }
            }
        }

        // Otherwise return what's already on the board (or '\0' if nothing meaningful yet)
        var cell = letterGrid[x, y];
        return cell.TileLetter; // You already store a char here
    }



    private bool PlacementIntroducesBanned(string word, Vector2Int start, Vector2Int dir) {
        int[] scanDX = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] scanDY = { -1, 0, 1, -1, 1, -1, 0, 1 };
        // For each cell the new word would occupy:
        for (int i = 0; i < word.Length; i++) {
            int cx = start.x + dir.x * i;
            int cy = start.y + dir.y * i;

            // Check 8 lines that pass through (cx,cy)
            for (int d = 0; d < 8; d++) {
                int dx = scanDX[d], dy = scanDY[d];

                // We want to read a contiguous line centered-ish at (cx,cy).
                // Build a window of up to maxBannedLen*2 to comfortably cover any substring that includes this cell.
                // Start by walking backwards up to maxBannedLen-1
                int bx = cx, by = cy;
                for (int k = 1; k < maxBannedLen; k++) {
                    int nx = bx - dx, ny = by - dy;
                    if (nx < 0 || nx >= GridSizeX || ny < 0 || ny >= GridSizeY) break;
                    bx = nx; by = ny;
                }

                // Now walk forward, collecting up to maxBannedLen*2 chars (enough to cover any banned substring)
                var buffer = new StringBuilder();
                int fx = bx, fy = by;
                int steps = maxBannedLen * 2;

                for (int step = 0; step < steps; step++) {
                    if (fx < 0 || fx >= GridSizeX || fy < 0 || fy >= GridSizeY) break;

                    bool _; // unused out
                    char ch = GetCharAfterPlacement(fx, fy, word, start, dir, out _);
                    if (ch == '\0') ch = ' '; // ignore empties

                    buffer.Append(char.ToUpperInvariant(ch));

                    // Check all suffixes up to maxBannedLen
                    int len = buffer.Length;
                    int maxCheck = Mathf.Min(len, maxBannedLen);
                    for (int n = 1; n <= maxCheck; n++) {
                        // skip trailing spaces
                        string sfx = buffer.ToString(len - n, n).Trim();
                        if (sfx.Length > 0 && bannedUpper.Contains(sfx)) {
                            return true; // would create a banned substring
                        }
                    }

                    fx += dx; fy += dy;
                }

                buffer.Clear();
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

    private char GetRandomLetter() {
        return alphabet[Random.Range(0, alphabet.Length)];
    }

    private bool GridContainsBannedWord() {
        int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
        int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

        int rayMax = Mathf.Max(GridSizeX, GridSizeY); // longest possible line
        var sb = new StringBuilder(rayMax);

        for (int x = 0; x < GridSizeX; x++) {
            for (int y = 0; y < GridSizeY; y++) {
                for (int d = 0; d < 8; d++) {
                    sb.Length = 0;
                    int cx = x, cy = y;

                    for (int step = 0; step < rayMax; step++) {
                        if (cx < 0 || cx >= GridSizeX || cy < 0 || cy >= GridSizeY) break;

                        // append next char uppercase
                        sb.Append(char.ToUpperInvariant(letterGrid[cx, cy].TileLetter));

                        // check suffixes up to cached max length
                        int curLen = sb.Length;
                        int maxCheck = Mathf.Min(curLen, maxBannedLen);
                        for (int n = 1; n <= maxCheck; n++) {
                            // forward suffix
                            string sfx = sb.ToString(curLen - n, n);
                            if (bannedUpper.Contains(sfx)) {
                                Debug.LogWarning($"🚫 Found banned word '{sfx}' in grid.");
                                return true;
                            }
                            // reversed suffix
                            bool matchRev = true;
                            for (int i = 0; i < n; i++) {
                                if (sb[curLen - 1 - i] != sfx[i]) { matchRev = false; break; }
                            }
                            if (matchRev && bannedUpper.Contains(sfx)) {
                                Debug.LogWarning($"🚫 Found banned word '{sfx}' in grid (reversed).");
                                return true;
                            }
                        }

                        // proper grouped early-exit:
                        int nx = cx + dx[d], ny = cy + dy[d];
                        if (sb.Length >= maxBannedLen && (nx < 0 || nx >= GridSizeX || ny < 0 || ny >= GridSizeY)) {
                            break;
                        }

                        cx = nx; cy = ny;
                    }
                }
            }
        }
        return false;
    }

    private void BuildBannedCaches() {
        bannedUpper.Clear();
        maxBannedLen = 0;
        foreach (var w in bannedWords) {
            var u = w.ToUpperInvariant().Trim();
            if (string.IsNullOrEmpty(u)) continue;
            bannedUpper.Add(u);
            if (u.Length > maxBannedLen) maxBannedLen = u.Length;
        }
        if (maxBannedLen == 0) maxBannedLen = 1;
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
            int boardMax = Mathf.Max(GridSizeX, GridSizeY);
            if (cleanWord.Length >= 3 && cleanWord.Length <= boardMax && !bannedWords.Contains(cleanWord)) {
                filteredWords.Add(cleanWord);
            }
        }

        validWords = filteredWords;
        Debug.Log($"[Dictionary] Loaded {validWords.Count} valid family-safe words.");
    }

}
