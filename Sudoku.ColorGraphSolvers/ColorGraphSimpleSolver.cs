using Sudoku.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku.ColorGraphSolvers;
public class ColorGraphSimpleSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid grid)
    {
        var graph = BuildGraph(grid);
        bool solved = ApplyGraphColoring(graph, grid);
        return solved ? grid : null;
    }

    private Dictionary<(int, int), HashSet<int>> BuildGraph(SudokuGrid grid)
    {
        var graph = new Dictionary<(int, int), HashSet<int>>();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (grid.Cells[i, j] == 0)
                {
                    var possibleNumbers = new HashSet<int>(Enumerable.Range(1, 9));
                    foreach (var neighbor in SudokuGrid.CellNeighbours[i][j])
                    {
                        possibleNumbers.Remove(grid.Cells[neighbor.row, neighbor.column]);
                    }
                    graph[(i, j)] = possibleNumbers;
                }
            }
        }
        return graph;
    }

    private bool ApplyGraphColoring(Dictionary<(int, int), HashSet<int>> graph, SudokuGrid grid)
    {
        var sortedCells = graph.Keys.OrderBy(cell => graph[cell].Count).ToList();
        return BacktrackColoring(sortedCells, graph, grid, 0);
    }

    private bool BacktrackColoring(List<(int, int)> sortedCells, Dictionary<(int, int), HashSet<int>> graph, SudokuGrid grid, int index)
    {
        if (index >= sortedCells.Count) return true;

        var (row, col) = sortedCells[index];
        foreach (var number in graph[(row, col)])
        {
            if (IsValidPlacement(grid, row, col, number))
            {
                grid.Cells[row, col] = number;
                if (BacktrackColoring(sortedCells, graph, grid, index + 1)) return true;
                grid.Cells[row, col] = 0;
            }
        }
        return false;
    }

    private bool IsValidPlacement(SudokuGrid grid, int row, int col, int number)
    {
        foreach (var neighbor in SudokuGrid.CellNeighbours[row][col])
        {
            if (grid.Cells[neighbor.row, neighbor.column] == number) return false;
        }
        return true;
    }
}
