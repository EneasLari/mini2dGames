using UnityEngine;
using TMPro;

public class JigsawManager : MonoBehaviour {
    public JigsawPieceSet pieceSet; // Reference to your ScriptableObject
    public GameObject piecePrefab;
    public Transform parentCanvas;
    public TextMeshProUGUI winText;

    private JigsawPiece[] pieces;

    void Start() {
        winText.text = "";

        pieces = new JigsawPiece[pieceSet.pieces.Length];

        for (int i = 0; i < pieceSet.pieces.Length; i++) {
            GameObject piece = Instantiate(piecePrefab, parentCanvas);
            piece.GetComponent<UnityEngine.UI.Image>().sprite = pieceSet.pieces[i];
            pieces[i] = piece.GetComponent<JigsawPiece>();
        }
    }

    void Update() {
        foreach (var piece in pieces) {
            if (!piece.IsPlacedCorrectly())
                return;
        }

        winText.text = "You Win!";
    }
}
