using UnityEngine;
using System.Collections.Generic;

public class LetterTilePool : MonoBehaviour {
    public static LetterTilePool Instance;

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int prewarmCount = 16;

    public Queue<GameObject> pool = new Queue<GameObject>();

    void Awake() {
        if (Instance == null || (Instance != null && Instance != this)) Instance = this;
        else Destroy(gameObject);

        // Pre-instantiate pool
        for (int i = 0; i < prewarmCount; i++) {
            var tile = Instantiate(tilePrefab, transform);
            tile.name = $"Pre_LetterTile_{ i}";
            tile.SetActive(false);
            pool.Enqueue(tile);
        }
    }

    public GameObject GetTile(Transform parent) {
        GameObject tile;
        if (pool.Count > 0) {
            tile = pool.Dequeue();
        }
        else {
            tile = Instantiate(tilePrefab, transform);
            tile.name = $"LetterTile_{parent.childCount}";
        }
        tile.transform.SetParent(parent, false);
        tile.SetActive(true);
        return tile;
    }

    public void ReturnTile(GameObject tile) {
        if (!pool.Contains(tile)) {
            tile.SetActive(false);
            pool.Enqueue(tile);
        }
        else {
            Debug.LogWarning("Attempted to pool the same tile twice!");
        }
    }

}
