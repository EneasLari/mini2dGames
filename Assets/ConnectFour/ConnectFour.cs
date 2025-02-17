using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConnectFour : MonoBehaviour {
    private const int ROWS = 6;
    private const int COLS = 7;

    public GameObject redDiscPrefab;
    public GameObject yellowDiscPrefab;
    public Transform[] columns; // 7 columns in the UI

    private int[,] grid = new int[ROWS, COLS]; // 0: Empty, 1: Red, 2: Yellow
    private bool isRedTurn = true; // Red goes first

    void Start() {
        ResetBoard();
    }

    public void DropDisc(int col) {
        for (int row = ROWS - 1; row >= 0; row--) // Start from the bottom
        {
            if (grid[row, col] == 0) // Empty slot found
            {
                grid[row, col] = isRedTurn ? 1 : 2;
                SpawnDisc(row, col, isRedTurn);

                if (CheckWin(row, col)) {
                    Debug.Log(isRedTurn ? "Red Wins!" : "Yellow Wins!");
                    Invoke("ResetBoard", 2f);
                } else {
                    isRedTurn = !isRedTurn; // Switch turn
                }
                return;
            }
        }
    }

    void SpawnDisc(int row, int col, bool isRed) {
        GameObject disc = Instantiate(isRed ? redDiscPrefab : yellowDiscPrefab, columns[col]);
        disc.transform.position = new Vector3(columns[col].position.x, 3f, 0); // Drop from the top
        StartCoroutine(DropAnimation(disc, row));
    }

    IEnumerator DropAnimation(GameObject disc, int row) {
        Vector3 targetPos = new Vector3(disc.transform.position.x, row * -1f, 0);
        while (Vector3.Distance(disc.transform.position, targetPos) > 0.01f) {
            disc.transform.position = Vector3.Lerp(disc.transform.position, targetPos, Time.deltaTime * 10);
            yield return null;
        }
    }

    bool CheckWin(int row, int col) {
        int player = grid[row, col];
        return CheckDirection(row, col, 1, 0, player) ||  // Horizontal
               CheckDirection(row, col, 0, 1, player) ||  // Vertical
               CheckDirection(row, col, 1, 1, player) ||  // Diagonal \
               CheckDirection(row, col, 1, -1, player);   // Diagonal /
    }

    bool CheckDirection(int row, int col, int dRow, int dCol, int player) {
        int count = 1;
        count += CountMatches(row, col, dRow, dCol, player);   // Forward check
        count += CountMatches(row, col, -dRow, -dCol, player); // Backward check
        return count >= 4;
    }

    int CountMatches(int row, int col, int dRow, int dCol, int player) {
        int count = 0;
        for (int i = 1; i < 4; i++) {
            int r = row + i * dRow, c = col + i * dCol;
            if (r < 0 || r >= ROWS || c < 0 || c >= COLS || grid[r, c] != player) break;
            count++;
        }
        return count;
    }

    void ResetBoard() {
        grid = new int[ROWS, COLS];
        isRedTurn = true;

        foreach (Transform col in columns) {
            foreach (Transform child in col) {
                Destroy(child.gameObject);
            }
        }
    }
}
