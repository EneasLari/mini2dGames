using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tile : MonoBehaviour {
    public bool isMine;
    public int adjacentMines;
    private bool isRevealed = false;

    public TextMeshProUGUI tileText;

    void Start() {
        Button button = GetComponentInChildren<Button>();
        button.onClick.AddListener(RevealTile);
    }

    public void SetMine() {
        isMine = true;
    }

    public void SetAdjacentMines(int count) {
        adjacentMines = count;
        tileText.text = count > 0 ? count.ToString() : "";
    }

    public void RevealTile() {
        if (isRevealed) return;

        isRevealed = true;
        GetComponentInChildren<Button>().interactable = false;

        if (isMine) {
            tileText.text = "M";
            FindFirstObjectByType<MinesweeperManager>().GameOver();
        } else if (adjacentMines == 0) {
            FindFirstObjectByType<MinesweeperManager>().RevealAdjacentTiles(this);
        }
    }

    // ✅ New Method: Adjusts Font Size Based on Tile Size
    public void AdjustFontSize(float tileSize) {
        if (tileText != null) {
            tileText.fontSize = tileSize * 0.6f; // Adjust this multiplier for best appearance
        }
    }

    // Public method to check if the tile is revealed
    public bool IsRevealed() {
        return isRevealed;
    }

}
