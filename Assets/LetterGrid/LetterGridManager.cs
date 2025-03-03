using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour {
    public GameObject letterTilePrefab; // Prefab for letter tiles
    public Transform gridParent; // Parent panel for grid tiles
    public int gridSize = 4; // 4x4 grid

    private char[,] letterGrid;
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private void Start() {
        GenerateGrid();
    }

    void GenerateGrid() {
        letterGrid = new char[gridSize, gridSize];

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                // Generate a random letter
                char letter = alphabet[Random.Range(0, alphabet.Length)];
                letterGrid[i, j] = letter;

                // Instantiate letter tile
                GameObject tile = Instantiate(letterTilePrefab, gridParent);
                tile.GetComponentInChildren<Text>().text = letter.ToString();
                tile.GetComponent<LetterGridLetterTile>().SetLetter(letter);
            }
        }
    }
}
