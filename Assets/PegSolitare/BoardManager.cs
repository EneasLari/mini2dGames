using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;

public class BoardManager : MonoBehaviour {
    public GameObject pegPrefab;   // Assign Peg Prefab in Inspector
    public Transform gridParent;   // The parent containing hole UI elements

    private Dictionary<Vector2Int, Peg> pegs = new Dictionary<Vector2Int, Peg>();
    private Transform[,] holeTransforms = new Transform[7, 7]; // Stores each hole's transform

    // Board layout: -1 = invalid area, 0 = empty, 1 = peg
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
        CacheHolePositions();
        PlacePegs();
    }

    // Cache each valid hole's transform from gridParent and set its board coordinate.
    private void CacheHolePositions() {
        int childIndex = 0; // Index of valid holes inside gridParent

        for (int x = 0; x < 7; x++) {
            for (int y = 0; y < 7; y++) {
                if (boardLayout[x, y] != -1) {
                    if (childIndex < gridParent.childCount) {
                        Transform holeTransform = gridParent.GetChild(childIndex);
                        holeTransforms[x, y] = holeTransform;

                        // If a Hole component exists on the UI element, set its boardCoordinate.
                        Hole holeComp = holeTransform.AddComponent<Hole>();
                        if (holeComp != null) {
                            holeComp.boardCoordinate = new Vector2Int(x, y);
                        }
                    } else {
                        Debug.LogError("Not enough hole elements in gridParent to match boardLayout.");
                    }
                } else {
                    Image holeImage = gridParent.GetChild(childIndex).GetComponent<Image>();
                    holeImage.enabled = false;
                }
                childIndex++;
            }
        }
    }

    // Place pegs on holes according to the boardLayout.
    private void PlacePegs() {
        for (int x = 0; x < 7; x++) {
            for (int y = 0; y < 7; y++) {
                if (boardLayout[x, y] == 1) { // A peg should be placed here.
                    Transform hole = holeTransforms[x, y];
                    if (hole != null) {
                        GameObject pegObj = Instantiate(pegPrefab, hole.position, Quaternion.identity, hole);
                        Peg peg = pegObj.GetComponent<Peg>();
                        peg.boardPosition = new Vector2Int(x, y);
                        pegs[new Vector2Int(x, y)] = peg;
                    }
                }
            }
        }
    }

    // Called when a peg is clicked.
    public void SelectPeg(Peg peg) {
        selectedPeg = peg;
    }

    // Called when a hole is clicked.
    public void SelectHole(Vector2Int holeCoordinate) {
        if (selectedPeg != null) {
            Vector2 targetPos = holeTransforms[holeCoordinate.x, holeCoordinate.y].position;
            TryMove(selectedPeg, holeCoordinate, targetPos);
            selectedPeg = null;
        }
    }

    // Attempts to move the selected peg to the target hole.
    private void TryMove(Peg peg, Vector2Int targetCell, Vector2 targetPos) {
        Vector2Int pegCell = peg.boardPosition;
        // Calculate the middle cell (the peg to be jumped over).
        Vector2Int middleCell = new Vector2Int((pegCell.x + targetCell.x) / 2, (pegCell.y + targetCell.y) / 2);

        // Check if the move is valid: there must be a peg in the middle, the target must be empty,
        // and the move must be exactly two cells in one direction.
        if (pegs.ContainsKey(middleCell) && !pegs.ContainsKey(targetCell) && IsValidMove(pegCell, targetCell)) {
            // Remove the jumped peg.
            Destroy(pegs[middleCell].gameObject);
            pegs.Remove(middleCell);
            // Remove the moving peg's old entry.
            pegs.Remove(pegCell);

            // Update the peg's board position and move it.
            peg.boardPosition = targetCell;
            peg.MoveTo(targetPos);
            // Change the peg's parent to the new hole.
            peg.transform.SetParent(holeTransforms[targetCell.x, targetCell.y]);
            pegs[targetCell] = peg;

            CheckWinCondition();
        } else {
            // Invalid move – reset the peg to its original position.
            peg.ResetPosition();
        }
    }

    // Valid move if the peg moves two cells horizontally or vertically.
    private bool IsValidMove(Vector2Int from, Vector2Int to) {
        Vector2Int diff = to - from;
        if (Mathf.Abs(diff.x) == 2 && diff.y == 0) return true;
        if (Mathf.Abs(diff.y) == 2 && diff.x == 0) return true;
        return false;
    }

    private void CheckWinCondition() {
        if (pegs.Count == 1) {
            Debug.Log("You Win!");
        }
    }
}
