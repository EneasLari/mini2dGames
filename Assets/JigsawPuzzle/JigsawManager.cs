using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class JigsawManager : MonoBehaviour {
    public JigsawPieceSet pieceSet;        // ScriptableObject with Sprite[] pieces and int columns
    public GameObject piecePrefab;         // Prefab with JigsawPiece component, UI Image, RectTransform
    public Transform puzzleBoard;          // Parent object with GridLayoutGroup for grid arrangement
    public TextMeshProUGUI winText;

    private JigsawPiece[] pieces;
    private List<Vector2> gridPositions = new List<Vector2>();
    private bool puzzleStarted = false;
    private GridLayoutGroup gridLayout;

    void Start() {
        winText.text = "";

        // Get and set up the GridLayoutGroup
        gridLayout = puzzleBoard.GetComponent<GridLayoutGroup>();
        RectTransform pieceRect = piecePrefab.GetComponent<RectTransform>();

        // 1. Set grid cell size to match the prefab
        if (gridLayout != null && pieceRect != null) {
            gridLayout.cellSize = new Vector2(pieceRect.rect.width, pieceRect.rect.height);

            // 2. Set grid constraint to use the column count from the ScriptableObject
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = pieceSet.columns;
        } else {
            Debug.LogError("Missing GridLayoutGroup or piecePrefab RectTransform!");
            return;
        }

        // 3. Calculate rows just for logic (not used by GridLayoutGroup)
        int totalPieces = pieceSet.pieces.Length;
        int columns = pieceSet.columns;
        int rows = Mathf.CeilToInt((float)totalPieces / columns);

        // 4. Instantiate all pieces in the grid (they will appear solved)
        pieces = new JigsawPiece[totalPieces];
        for (int i = 0; i < totalPieces; i++) {
            GameObject piece = Instantiate(piecePrefab, puzzleBoard);
            piece.GetComponent<Image>().sprite = pieceSet.pieces[i];
            pieces[i] = piece.GetComponent<JigsawPiece>();
        }

        // 5. Wait a frame for layout to position everything
        StartCoroutine(SetupAndShuffle());
    }

    IEnumerator SetupAndShuffle() {
        yield return null; // Wait for Unity's layout system to arrange the grid

        // 6. Record all correct grid positions
        gridPositions.Clear();
        foreach (var piece in pieces) {
            RectTransform rect = piece.GetComponent<RectTransform>();
            piece.correctPosition = rect.anchoredPosition;
            gridPositions.Add(rect.anchoredPosition);
        }

        // 7. Show completed puzzle for 3 seconds
        yield return new WaitForSeconds(3f);

        // 8. Disable GridLayoutGroup so you can move pieces freely
        if (gridLayout != null)
            gridLayout.enabled = false;

        // 9. Shuffle the positions (randomize where each piece is)
        List<Vector2> shuffledPositions = new List<Vector2>(gridPositions);
        for (int i = 0; i < shuffledPositions.Count; i++) {
            Vector2 temp = shuffledPositions[i];
            int randomIndex = Random.Range(i, shuffledPositions.Count);
            shuffledPositions[i] = shuffledPositions[randomIndex];
            shuffledPositions[randomIndex] = temp;
        }
        for (int i = 0; i < pieces.Length; i++) {
            pieces[i].GetComponent<RectTransform>().anchoredPosition = shuffledPositions[i];
        }

        puzzleStarted = true;
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
