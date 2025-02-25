using UnityEngine;
using UnityEngine.EventSystems;

public class Hole : MonoBehaviour, IPointerClickHandler {
    public Vector2Int boardCoordinate; // This will be set by the BoardManager

    public void OnPointerClick(PointerEventData eventData) {
        BoardManager boardManager = FindFirstObjectByType<BoardManager>();
        boardManager.SelectHole(boardCoordinate);
    }
}
