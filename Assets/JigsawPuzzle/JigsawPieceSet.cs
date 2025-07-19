using UnityEngine;

[CreateAssetMenu(fileName = "JigsawPieceSet", menuName = "Jigsaw/PieceSet")]
public class JigsawPieceSet : ScriptableObject {
    public Sprite[] pieces;
    public int columns; // Set this to match your Sprite Editor's "Column Count"
}
