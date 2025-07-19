using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class JigsawManager : MonoBehaviour {
    public JigsawPieceSet pieceSet;
    public GameObject piecePrefab;
    public Transform puzzleBoard;      // Should have a GridLayoutGroup
    public TextMeshProUGUI winText;

    public AudioClip clickSound;
    public AudioClip correctSound;
    public AudioSource audioSource; // Assign via inspector or create at runtime


    [HideInInspector] public List<Vector2> gridPositions = new List<Vector2>();

    private JigsawPiece[] pieces;
    private bool puzzleStarted = false;
    private GridLayoutGroup gridLayout;

    void Start() {
        winText.text = "";

        gridLayout = puzzleBoard.GetComponent<GridLayoutGroup>();
        RectTransform pieceRect = piecePrefab.GetComponent<RectTransform>();

        if (gridLayout != null && pieceRect != null) {
            gridLayout.cellSize = new Vector2(pieceRect.rect.width, pieceRect.rect.height);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = pieceSet.columns;
        } else {
            Debug.LogError("Missing GridLayoutGroup or piecePrefab RectTransform!");
            return;
        }

        int totalPieces = pieceSet.pieces.Length;

        pieces = new JigsawPiece[totalPieces];
        for (int i = 0; i < totalPieces; i++) {
            GameObject piece = Instantiate(piecePrefab, puzzleBoard);
            piece.GetComponent<Image>().sprite = pieceSet.pieces[i];
            pieces[i] = piece.GetComponent<JigsawPiece>();
            pieces[i].manager = this; // Reference to the manager for position lookup
            pieces[i].correctGridIndex = i; // This is where the piece belongs (solved)
            pieces[i].currentGridIndex = i; // This is where it is now (will shuffle later)
        }

        StartCoroutine(SetupAndShuffle());
    }

    IEnumerator SetupAndShuffle() {
        yield return null; // Wait for layout system

        gridPositions.Clear();
        foreach (var piece in pieces)
            gridPositions.Add(piece.GetComponent<RectTransform>().anchoredPosition);

        yield return new WaitForSeconds(3f);

        if (gridLayout != null)
            gridLayout.enabled = false;

        for (int i = 0; i < pieces.Length; i++) {
            pieces[i].transform.localScale = pieces[i].smallScale;
            pieces[i].isLocked = false;
        }

        // Shuffle grid indices
        List<int> shuffledIndices = new List<int>();
        for (int i = 0; i < pieces.Length; i++) shuffledIndices.Add(i);

        do {
            // Fisher-Yates shuffle
            for (int i = 0; i < shuffledIndices.Count; i++) {
                int temp = shuffledIndices[i];
                int randomIndex = Random.Range(i, shuffledIndices.Count);
                shuffledIndices[i] = shuffledIndices[randomIndex];
                shuffledIndices[randomIndex] = temp;
            }
        } while (HasAnyCorrectlyPlaced(shuffledIndices));
        // Apply shuffled indices and set positions
        for (int i = 0; i < pieces.Length; i++) {
            pieces[i].currentGridIndex = shuffledIndices[i];
            pieces[i].GetComponent<RectTransform>().anchoredPosition = gridPositions[shuffledIndices[i]];
        }

        puzzleStarted = true;
    }

    bool HasAnyCorrectlyPlaced(List<int> indices) {
        for (int i = 0; i < indices.Count; i++)
            if (indices[i] == i)
                return true;
        return false;
    }

    public void PlayClickSound() {
        if (audioSource && clickSound) audioSource.PlayOneShot(clickSound);
    }

    public void PlayCorrectSound() {
        if (audioSource && correctSound) audioSource.PlayOneShot(correctSound);
    }


    void Update() {
        if (!puzzleStarted) return;

        foreach (var piece in pieces) {
            if (!piece.IsPlacedCorrectly())
                return;
        }
        winText.text = "You Win!";
    }
}
