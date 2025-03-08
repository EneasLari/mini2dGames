using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum Operation {
    Addition,
    Subtraction,
    Multiplication,
    Division,
    None // For cages with a single cell
}

[System.Serializable]
public class KenKenCage {
    public int targetValue;
    public Operation op;
    // List of cell positions in the grid that form this cage (row, column)
    public List<Vector2Int> cellPositions;

    // Check if the cage's values meet the target condition
    public bool CheckCage(KenKenCell[,] cells) {
        List<int> values = new List<int>();
        foreach (var pos in cellPositions) {
            values.Add(cells[pos.x, pos.y].Value);
        }

        switch (op) {
            case Operation.Addition:
                int sum = 0;
                foreach (int v in values) sum += v;
                return sum == targetValue;
            case Operation.Multiplication:
                int product = 1;
                foreach (int v in values) product *= v;
                return product == targetValue;
            case Operation.Subtraction:
                // Assuming subtraction cages only have 2 cells
                if (values.Count == 2)
                    return Mathf.Abs(values[0] - values[1]) == targetValue;
                break;
            case Operation.Division:
                // Assuming division cages only have 2 cells
                if (values.Count == 2) {
                    int a = values[0], b = values[1];
                    if (b != 0 && a / b == targetValue && a % b == 0) return true;
                    if (a != 0 && b / a == targetValue && b % a == 0) return true;
                }
                break;
            case Operation.None:
                // For a single cell cage, just check the value
                if (values.Count == 1)
                    return values[0] == targetValue;
                break;
        }
        return false;
    }

    public string OperationString() {
        switch (op) {
            case Operation.Addition:
                return "+";

            case Operation.Multiplication:
                return "*";
            case Operation.Subtraction:
                return "-";
            case Operation.Division:
                return "/";
            case Operation.None:
                return "";
            default:
                return "?";
        }
        
    }
}
