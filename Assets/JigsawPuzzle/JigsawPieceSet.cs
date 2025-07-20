using UnityEngine;

[CreateAssetMenu(fileName = "JigsawPieceSet", menuName = "Jigsaw/PieceSet")]
public class JigsawPieceSet : ScriptableObject {
    public Sprite[] pieces;
    public int columns; // Set this to match your Sprite Editor's "Column Count"
    public int rows;

    public bool ValidateGrid() {
        if (columns * rows != pieces.Length) {
            Debug.LogWarning($"Piece count {pieces.Length} does not match grid {columns} x {rows}!");
            return false;
        }
        return true;
    }


}
