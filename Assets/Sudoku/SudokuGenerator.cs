using UnityEngine;
using System;

public class SudokuGenerator {
    private static int[,] grid = new int[9, 9];

    public static void Generate(out int[,] puzzle, out int[,] solution) {
        FillGrid();
        solution = (int[,])grid.Clone();
        RemoveNumbers(40); // Change number to set difficulty (Easy: 30, Hard: 50)
        puzzle = (int[,])grid.Clone();
    }

    private static void FillGrid() {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
                grid[i, j] = 0;

        Solve(0, 0);
    }

    private static bool Solve(int row, int col) {
        if (row == 9) return true;
        if (col == 9) return Solve(row + 1, 0);
        if (grid[row, col] != 0) return Solve(row, col + 1);

        int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);

        foreach (int num in numbers) {
            if (IsValidMove(row, col, num)) {
                grid[row, col] = num;
                if (Solve(row, col + 1)) return true;
                grid[row, col] = 0;
            }
        }
        return false;
    }

    private static bool IsValidMove(int row, int col, int num) {
        for (int i = 0; i < 9; i++)
            if (grid[row, i] == num || grid[i, col] == num) return false;

        int startRow = row / 3 * 3;
        int startCol = col / 3 * 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (grid[startRow + i, startCol + j] == num) return false;

        return true;
    }

    private static void RemoveNumbers(int amount) {
        int removed = 0;
        System.Random rand = new System.Random();

        while (removed < amount) {
            int row = rand.Next(9);
            int col = rand.Next(9);
            if (grid[row, col] != 0) {
                grid[row, col] = 0;
                removed++;
            }
        }
    }

    private static void Shuffle(int[] array) {
        System.Random rand = new System.Random();
        for (int i = array.Length - 1; i > 0; i--) {
            int j = rand.Next(i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
}
