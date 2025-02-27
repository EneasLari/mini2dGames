using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CrosswordGrid : MonoBehaviour {
    public GameObject cellPrefab; // Assign the InputField prefab in Inspector
    private GameObject[,] cells;

    public void GenerateGrid(int rows,int cols) {
        cells = new GameObject[rows, cols];
        transform.GetComponent<GridLayoutGroup>().constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        transform.GetComponent<GridLayoutGroup>().constraintCount = cols;
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                GameObject cell = Instantiate(cellPrefab, transform);
                cell.name = $"Cell_{i}_{j}";

                // Set placeholder text
                TMP_InputField input = cell.GetComponent<TMP_InputField>();
                input.characterLimit = 1; // Allow only one letter
                input.onValueChanged.AddListener(delegate { ValidateInput(input); });

                cells[i, j] = cell;
            }
        }
    }

    void ValidateInput(TMP_InputField field) {
        if (field.text.Length >= 1)
            field.text = field.text[0].ToString().ToUpper(); // Convert to uppercase
    }
}
