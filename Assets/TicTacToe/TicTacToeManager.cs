using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class TicTacToeManager : MonoBehaviour {
    public Button[] buttons;
    public TMP_Text statusText;
    public GameObject winningLine;
    public bool isSinglePlayer = true;
    public bool useMinimaxAI = false; // Minimax AI (Hard Mode) or Random AI (Easy Mode)
    public Button ReplayButton;

    private string currentPlayer = "X";
    private string[] board = new string[9];

    void Start() {
        ResetBoard();
        SubscribeButtons();
        ReplayButton.onClick.AddListener(() => ResetBoard());
    }

    void SubscribeButtons() {
        for (int i = 0; i < buttons.Length; i++) {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    public void OnButtonClick(int index) {
        if (board[index] == "") {
            MakeMove(index, currentPlayer);

            int[] winCombo = GetWinningCombination();
            if (MoveCount() >= 5 && winCombo != null) {
                OnGameEnd(currentPlayer, winCombo);
                return;
            } else if (CheckDraw()) {
                statusText.text = "It's a Draw!";
                return;
            }

            currentPlayer = (currentPlayer == "X") ? "O" : "X";
            statusText.text = "Player " + currentPlayer + "'s turn";

            if (isSinglePlayer && currentPlayer == "O") {
                Invoke("AIMove", 0.5f);
            }
        }
    }

    void AIMove() {
        int moveIndex = useMinimaxAI ? GetBestMove() : GetRandomMove();

        if (moveIndex != -1) {
            MakeMove(moveIndex, "O");

            int[] winCombo = GetWinningCombination();
            if (MoveCount() >= 5 && winCombo != null) {
                OnGameEnd("O", winCombo);
                return;
            } else if (CheckDraw()) {
                statusText.text = "It's a Draw!";
                return;
            }

            currentPlayer = "X";
            statusText.text = "Player X's turn";
        }
    }

    void MakeMove(int index, string player) {
        board[index] = player;
        TMP_Text buttonText = buttons[index].GetComponentInChildren<TMP_Text>();
        if (buttonText != null) {
            buttonText.text = player;
        }
        buttons[index].interactable = false;
    }

    int GetRandomMove() {
        List<int> availableMoves = new List<int>();
        for (int i = 0; i < board.Length; i++) {
            if (board[i] == "") {
                availableMoves.Add(i);
            }
        }
        return availableMoves.Count > 0 ? availableMoves[Random.Range(0, availableMoves.Count)] : -1;
    }

    int GetBestMove() {
        int bestScore = int.MinValue;
        int bestMove = -1;

        for (int i = 0; i < board.Length; i++) {
            if (board[i] == "") {
                board[i] = "O";
                int score = Minimax(board, 0, false);
                board[i] = "";

                if (score > bestScore) {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }
        return bestMove;
    }

    int Minimax(string[] newBoard, int depth, bool isMaximizing) {
        int[] winCombo = GetWinningCombination();
        if (winCombo != null) return (currentPlayer == "O") ? 10 - depth : depth - 10;
        if (CheckDraw()) return 0;

        if (isMaximizing) {
            int bestScore = int.MinValue;
            for (int i = 0; i < newBoard.Length; i++) {
                if (newBoard[i] == "") {
                    newBoard[i] = "O";
                    int score = Minimax(newBoard, depth + 1, false);
                    newBoard[i] = "";
                    bestScore = Mathf.Max(score, bestScore);
                }
            }
            return bestScore;
        } else {
            int bestScore = int.MaxValue;
            for (int i = 0; i < newBoard.Length; i++) {
                if (newBoard[i] == "") {
                    newBoard[i] = "X";
                    int score = Minimax(newBoard, depth + 1, true);
                    newBoard[i] = "";
                    bestScore = Mathf.Min(score, bestScore);
                }
            }
            return bestScore;
        }
    }

    int[] GetWinningCombination() {
        int[,] winPatterns = new int[,] {
            {0,1,2}, {3,4,5}, {6,7,8}, // Rows
            {0,3,6}, {1,4,7}, {2,5,8}, // Columns
            {0,4,8}, {2,4,6}           // Diagonals
        };

        for (int i = 0; i < winPatterns.GetLength(0); i++) {
            int a = winPatterns[i, 0], b = winPatterns[i, 1], c = winPatterns[i, 2];
            if (board[a] == currentPlayer && board[b] == currentPlayer && board[c] == currentPlayer) {
                return new int[] { a, b, c };
            }
        }
        return null;
    }

    void OnGameEnd(string winner, int[] winCombo) {
        statusText.text = winner + " Wins!";
        DisableButtons();
        DrawWinningLine(winCombo[0], winCombo[1], winCombo[2]);
        ReplayButton.gameObject.SetActive(true);
    }

    void DrawWinningLine(int a, int b, int c) {
        if (winningLine == null) return;

        winningLine.SetActive(true);

        Vector3 posA = buttons[a].transform.position;
        Vector3 posC = buttons[c].transform.position;

        Vector3 centerPos = (posA + posC) / 2f;
        winningLine.transform.position = centerPos;

        Vector3 direction = posC - posA;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        winningLine.transform.rotation = Quaternion.Euler(0, 0, angle);

        float distance = Vector3.Distance(posA, posC);
        winningLine.GetComponent<RectTransform>().sizeDelta = new Vector2(distance + 50, 10);
    }

    bool CheckDraw() {
        foreach (string spot in board) {
            if (spot == "") return false;
        }
        return GetWinningCombination() == null;
    }

    void DisableButtons() {
        foreach (Button button in buttons) {
            button.interactable = false;
        }
    }

    public void ResetBoard() {
        for (int i = 0; i < board.Length; i++) {
            board[i] = "";

            // Get the button and its TMP_Text
            Button button = buttons[i];
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();

            // Set random colors
            Color randomButtonColor = GetRandomColor();
            Color contrastingTextColor = GetContrastingColor(randomButtonColor);

            // Apply colors
            button.GetComponent<Image>().color = randomButtonColor;
            if (buttonText != null) {
                buttonText.text = "";
                buttonText.color = contrastingTextColor;
            }

            button.interactable = true;
        }

        currentPlayer = "X";
        statusText.text = "Player X's turn";

        if (winningLine != null)
            winningLine.SetActive(false);
        if (ReplayButton != null)
            ReplayButton.gameObject.SetActive(false);
    }

    // Generates a random button color
    Color GetRandomColor() {
        return new Color(Random.value, Random.value, Random.value);
    }

    // Ensures high contrast between button background and text
    Color GetContrastingColor(Color backgroundColor) {
        // Convert color to grayscale value
        float brightness = (backgroundColor.r * 0.299f + backgroundColor.g * 0.587f + backgroundColor.b * 0.114f);

        // Return white text for dark backgrounds, black text for bright backgrounds
        return (brightness > 0.5f) ? Color.black : Color.white;
    }


    int MoveCount() {
        return System.Array.FindAll(board, cell => cell != "").Length;
    }
}
