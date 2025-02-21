using UnityEngine;

public class Peg : MonoBehaviour {
    private Vector2 originalPosition;
    private BoardManager boardManager;

    private void Start() {
        boardManager = FindObjectOfType<BoardManager>();
        originalPosition = transform.position;
    }

    private void OnMouseDown() {
        boardManager.SelectPeg(this);
    }

    public void MoveTo(Vector2 newPosition) {
        transform.position = newPosition;
    }

    public void ResetPosition() {
        transform.position = originalPosition;
    }
}
