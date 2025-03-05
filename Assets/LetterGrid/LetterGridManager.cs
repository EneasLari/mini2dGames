using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LetterGridManager : MonoBehaviour {
    public static LetterGridManager instance;
    public GameObject letterTilePrefab; // Prefab for letter tiles
    public Transform gridParent; // Parent panel for grid tiles
    public int gridSize = 4; // 4x4 grid
    
    private char[,] letterGrid;
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private List<LetterGridLetterTile> tilesList=new();

    private void Awake() {
        instance = this;
    }
    private void Start() {
        gridParent.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedRowCount;
        gridParent.GetComponent<GridLayoutGroup>().constraintCount = gridSize;
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
                tile.GetComponentInChildren<TMP_Text>().text = letter.ToString();
                LetterGridLetterTile letterTile = tile.GetComponent<LetterGridLetterTile>();
                letterTile.SetLetter(letter);
                tilesList.Add(letterTile);
            }
        }
    }

    public void ResetTiles() {
        foreach (var item in tilesList) {
            item.Deselect();
        }
    }
}
