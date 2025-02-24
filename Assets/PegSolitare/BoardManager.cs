using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour {
    public GameObject pegPrefab;   // Assign Peg Prefab in Inspector
    public Transform gridParent;   // Assign GridLayoutGroup GameObject in Inspector

    private Dictionary<Vector2Int, Peg> pegs = new Dictionary<Vector2Int, Peg>();
    private Transform[,] holeTransforms = new Transform[7, 7]; // Stores each hole's world position

    private int[,] boardLayout = {
        { -1, -1,  1,  1,  1, -1, -1 },
        { -1, -1,  1,  1,  1, -1, -1 },
        {  1,  1,  1,  1,  1,  1,  1 },
        {  1,  1,  1,  0,  1,  1,  1 },
        {  1,  1,  1,  1,  1,  1,  1 },
        { -1, -1,  1,  1,  1, -1, -1 },
        { -1, -1,  1,  1,  1, -1, -1 }
    };

    private Peg selectedPeg = null;

    private void Start() {
        CacheHolePositions(); // Store hole positions in an array
        PlacePegs();
    }

    private void CacheHolePositions() {
        int childIndex = 0; // Track hole positions inside the grid

        for (int x = 0; x < 7; x++) {
            for (int y = 0; y < 7; y++) {
                if (boardLayout[x, y] != -1) // Ignore invalid areas
                {
                    holeTransforms[x, y] = gridParent.GetChild(childIndex); // Store hole transform
                    childIndex++;
                }
            }
        }
    }

    private void PlacePegs() {
        for (int x = 0; x < 7; x++) {
            for (int y = 0; y < 7; y++) {
                if (boardLayout[x, y] == 1) // If peg should be placed
                {
                    Transform hole = holeTransforms[x, y];
                    if (hole != null) {
                        GameObject pegObj = Instantiate(pegPrefab, hole.position, Quaternion.identity, hole);
                        Peg peg = pegObj.GetComponent<Peg>();
                        pegs[new Vector2Int(x, y)] = peg;
                    }
                }
            }
        }
    }


    public void SelectPeg(Peg peg) {
        if (selectedPeg == null) {
            selectedPeg = peg;
        } else {
            Vector2 targetPos = peg.transform.position;
            TryMove(selectedPeg, targetPos);
            selectedPeg = null;
        }
    }

    private void TryMove(Peg peg, Vector2 targetPos) {
        Vector2 pegPosition = (Vector2)peg.transform.position;
        Vector2 middlePos = (pegPosition + targetPos) / 2;
        Vector2Int middleCell = new Vector2Int((int)middlePos.x, (int)middlePos.y);
        Vector2Int targetCell = new Vector2Int((int)targetPos.x, (int)targetPos.y);
        Vector2Int pegCell = new Vector2Int((int)pegPosition.x, (int)pegPosition.y);

        if (pegs.ContainsKey(middleCell) && !pegs.ContainsKey(targetCell) && IsValidMove(pegCell, targetCell)) {
            Destroy(pegs[middleCell].gameObject);
            pegs.Remove(middleCell);
            pegs.Remove(pegCell);

            peg.MoveTo(targetPos);
            pegs[targetCell] = peg;

            CheckWinCondition();
        } else {
            peg.ResetPosition();
        }
    }

    private bool IsValidMove(Vector2Int from, Vector2Int to) {
        Vector2Int[] validMoves = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };

        foreach (Vector2Int move in validMoves) {
            if (from + move == to)
                return true;
        }
        return false;
    }

    private void CheckWinCondition() {
        if (pegs.Count == 1) {
            Debug.Log("You Win!");
        }
    }
}
