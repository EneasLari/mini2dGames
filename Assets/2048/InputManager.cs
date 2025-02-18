using UnityEngine;

public class InputManager2048 : MonoBehaviour {
    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private GameManager2048 gameManager;

    void Start() {
        gameManager = FindFirstObjectByType<GameManager2048>();
    }

    void Update() {
        HandleTouchInput();
        HandleKeyboardInput();
    }

    void HandleTouchInput() {
        if (Input.GetMouseButtonDown(0)) {
            startTouchPosition = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) {
            endTouchPosition = Input.mousePosition;
            DetectSwipe();
        }
    }

    void HandleKeyboardInput() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.up));
        } else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.down));
        } else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.left));
        } else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.right));
        }
    }

    void DetectSwipe() {
        Vector2 swipeVector = endTouchPosition - startTouchPosition;
        print($"Swipe Vector: {swipeVector}");

        if (swipeVector.magnitude < 50) return; // Ignore small swipes

        if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y)) {
            // ✅ X-axis swipe: Left or Right
            if (swipeVector.x > 0) {
                print("Swiped Right → Moving Right");
                gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.right));
            } else {
                print("Swiped Left → Moving Left");
                gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.left));
            }
        } else {
            // ✅ Y-axis swipe: Fix inverted up/down logic
            if (swipeVector.y > 0) {
                print("Swiped Up → Moving Up");
                gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.up));
            } else {
                print("Swiped Down → Moving Down");
                gameManager.StartCoroutine(gameManager.MoveTiles(Vector2Int.down));
            }
        }
    }

}
