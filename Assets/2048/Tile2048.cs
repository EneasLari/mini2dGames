using UnityEngine;
using TMPro;
using System.Collections;

public class Tile2048 : MonoBehaviour {
    public int value;
    public TMP_Text valueText;
    public Vector2Int position; // ✅ Track tile’s position in the grid

    public void Initialize(int x, int y, int newValue) {
        position = new Vector2Int(x, y);
        SetValue(newValue);
    }

    public void SetValue(int newValue) {
        value = newValue;
        valueText.text = value.ToString();
    }

    // ✅ Set new position in the logic grid
    public void SetGridPosition(int x, int y) {
        position = new Vector2Int(x, y);
    }

    // ✅ Smooth movement animation
    public IEnumerator MoveAnimation(Vector3 target) {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        float duration = 0.1f; // Adjust speed here

        while (elapsedTime < duration) {
            transform.position = Vector3.Lerp(startPosition, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
    }
}
