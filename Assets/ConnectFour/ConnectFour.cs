using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectFour : MonoBehaviour {
    private const int ROWS = 6;
    private const int COLUMNS = 7;
    private int[,] grid = new int[ROWS, COLUMNS]; // 0 = Empty, 1 = Player 1, 2 = Player 2

    public GameObject redDiscPrefab;
    public GameObject yellowDiscPrefab;
    public Transform boardTransform;  // Grid Layout Group (Holds Empty Slots)
    public Transform columnButtons;   // UI Buttons to select columns
    private Transform discContainer;  // Dynamically created container for discs
    public TMP_Text winText;
    public float fallSpeed = 5.0f; // Speed of falling discs

    private int currentPlayer = 1; // Player 1 starts

    private Transform[,] boardSlots = new Transform[ROWS, COLUMNS];

    private bool isDropping = false; // Prevents multiple clicks while a disc is falling

    void Start() {

        // Set Grid Layout Group properties dynamically
        GridLayoutGroup gridLayout = boardTransform.GetComponent<GridLayoutGroup>();
        if (gridLayout != null) {
            gridLayout.startCorner = GridLayoutGroup.Corner.LowerLeft; // Forces bottom-left as (0,0)
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;    // Ensures left-to-right row filling
            gridLayout.childAlignment = TextAnchor.MiddleCenter;        // Ensures centered alignment
        }

        // Find the Canvas and create the disc container inside it
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) {
            Debug.LogError("Canvas not found! Make sure your scene has a UI Canvas.");
            return;
        }

        // // Create Disc Container, Dynamically create an empty GameObject as a sibling of boardTransform inside the Canvas
        GameObject container = new GameObject("DiscContainer");
        container.transform.SetParent(canvas.transform, false);
        discContainer = container.transform;



        // Store board slots into a 2D array for correct referencing
        int childIndex = 0;
        for (int row = 0; row < ROWS; row++) {
            for (int col = 0; col < COLUMNS; col++) {
                boardSlots[row, col] = boardTransform.GetChild(childIndex);
                childIndex++;
            }
        }

        // Assign button listeners to columns
        for (int i = 0; i < COLUMNS; i++) {
            int columnIndex = i; // Local copy for lambda closure
            columnButtons.GetChild(i).GetComponent<Button>().onClick.AddListener(() => StartCoroutine(DropDisc(columnIndex)));
        }
    }

    IEnumerator DropDisc(int column) {
        if (isDropping) yield break; // Prevents clicking during animation
        isDropping = true; // Lock input

        int row = GetLowestRow(column);
        if (row == -1) {
            isDropping = false; // Unlock input if column is full
            yield break;
        }

        grid[row, column] = currentPlayer;

        // Get the correct target slot using pre-stored reference
        Transform targetSlot = boardSlots[row, column];

        // Instantiate disc at the top row position (inside discContainer)
        Transform topSlot = columnButtons.GetChild(column);
        GameObject disc = Instantiate(
            currentPlayer == 1 ? redDiscPrefab : yellowDiscPrefab,
            topSlot.position,  // Start from the top row
            Quaternion.identity,
            discContainer // Now stored inside the dynamically created discContainer
        );

        // Falling animation using MoveTowards (More realistic)
        Vector3 startPos = topSlot.position;
        Vector3 endPos = targetSlot.position;
        float speed = fallSpeed * 2f; // Adjust speed for a natural fall

        while (Vector3.Distance(disc.transform.position, endPos) > 0.01f) {
            disc.transform.position = Vector3.MoveTowards(disc.transform.position, endPos, speed * Time.deltaTime);
            yield return null; // Wait for next frame
        }

        disc.transform.position = endPos; // Ensure it snaps to the final position

        // Check for a win
        if (CheckWin(row, column)) {
            winText.text = "Player " + currentPlayer + " Wins!";
            isDropping = false; // Allow input after game ends
            yield break;
        }

        // Switch player
        SwitchPlayer();

        isDropping = false; // Re-enable input after animation finishes
    }




    int GetLowestRow(int column) {
        for (int row = 0; row < ROWS; row++) {
            if (grid[row, column] == 0) {
                return row;
            }
        }
        return -1; // Column full
    }

    void SwitchPlayer() {
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
    }

    bool CheckWin(int row, int column) {
        return (CheckDirection(row, column, 1, 0) ||  // Horizontal
                CheckDirection(row, column, 0, 1) ||  // Vertical
                CheckDirection(row, column, 1, 1) ||  // Diagonal /
                CheckDirection(row, column, 1, -1));  // Diagonal \
    }

    bool CheckDirection(int row, int column, int rowStep, int colStep) {
        int count = 1;
        count += CountMatches(row, column, rowStep, colStep);
        count += CountMatches(row, column, -rowStep, -colStep);
        return count >= 4;
    }

    int CountMatches(int row, int column, int rowStep, int colStep) {
        int count = 0;
        int player = grid[row, column];

        for (int i = 1; i < 4; i++) {
            int newRow = row + i * rowStep;
            int newCol = column + i * colStep;

            if (newRow < 0 || newRow >= ROWS || newCol < 0 || newCol >= COLUMNS || grid[newRow, newCol] != player) {
                break;
            }
            count++;
        }

        return count;
    }

    public void RestartGame() {
        for (int row = 0; row < ROWS; row++) {
            for (int col = 0; col < COLUMNS; col++) {
                grid[row, col] = 0;
            }
        }

        foreach (Transform child in discContainer) // Now clearing only the spawned discs
        {
            Destroy(child.gameObject);
        }

        currentPlayer = 1;
        winText.text = "";
    }
}
