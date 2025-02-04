using System;
using Microsoft.Z3;
using Sudoku.Shared;

namespace Sudoku.Z3Solvers
{
    public class Z3SimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            using (Context ctx = new Context())
            {
                Solver solver = ctx.MkSolver();
                IntExpr[,] cells = new IntExpr[9, 9];

                // Définition des variables pour chaque case de la grille
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        cells[i, j] = ctx.MkIntConst($"x_{i}_{j}");
                        solver.Add(ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), cells[i, j]), ctx.MkLe(cells[i, j], ctx.MkInt(9))));
                    }
                }

                // Ajout des contraintes de la grille initiale
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (s.Cells[i,j] != 0) // Si une valeur est donnée, on la fixe
                        {
                            solver.Add(ctx.MkEq(cells[i, j], ctx.MkInt(s.Cells[i,j])));
                        }
                    }
                }

                // Contraintes de ligne : chaque ligne doit contenir des valeurs uniques de 1 à 9
                for (int i = 0; i < 9; i++)
                {
                    solver.Add(ctx.MkDistinct(cells[i, 0], cells[i, 1], cells[i, 2], cells[i, 3], cells[i, 4], cells[i, 5], cells[i, 6], cells[i, 7], cells[i, 8]));
                }

                // Contraintes de colonne : chaque colonne doit contenir des valeurs uniques de 1 à 9
                for (int j = 0; j < 9; j++)
                {
                    solver.Add(ctx.MkDistinct(cells[0, j], cells[1, j], cells[2, j], cells[3, j], cells[4, j], cells[5, j], cells[6, j], cells[7, j], cells[8, j]));
                }

                // Contraintes de sous-grilles 3x3
                for (int boxRow = 0; boxRow < 3; boxRow++)
                {
                    for (int boxCol = 0; boxCol < 3; boxCol++)
                    {
                        solver.Add(ctx.MkDistinct(
                            cells[3 * boxRow, 3 * boxCol], cells[3 * boxRow, 3 * boxCol + 1], cells[3 * boxRow, 3 * boxCol + 2],
                            cells[3 * boxRow + 1, 3 * boxCol], cells[3 * boxRow + 1, 3 * boxCol + 1], cells[3 * boxRow + 1, 3 * boxCol + 2],
                            cells[3 * boxRow + 2, 3 * boxCol], cells[3 * boxRow + 2, 3 * boxCol + 1], cells[3 * boxRow + 2, 3 * boxCol + 2]
                        ));
                    }
                }

                // Résolution du Sudoku
                if (solver.Check() == Status.SATISFIABLE)
                {
                    Model model = solver.Model;
                    SudokuGrid solvedGrid = new SudokuGrid();
                    
                    for (int i = 0; i < 9; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            solvedGrid.Cells[i,j] = ((IntNum)model.Evaluate(cells[i, j])).Int;
                        }
                    }
                    return solvedGrid;
                }
                else
                {
                    throw new Exception("Le Sudoku n'a pas de solution.");
                }
            }
        }
    }
}
