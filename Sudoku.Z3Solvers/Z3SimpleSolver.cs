using Sudoku.Shared;
using Microsoft.Z3;

namespace Sudoku.Z3Solvers
{
    public class Z3SimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Créer un contexte Z3
            using (var context = new Context())
            {
                // Créer un tableau de variables Z3 pour chaque case du Sudoku (9x9)
                IntExpr[,] grid = new IntExpr[9, 9];
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        grid[row, col] = context.MkIntConst($"cell_{row}_{col}");
                    }
                }

                // Contraintes : chaque case doit avoir une valeur entre 1 et 9
                BoolExpr constraints = context.MkTrue();
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        constraints = context.MkAnd(constraints, context.MkAnd(
                            context.MkGe(grid[row, col], context.MkInt(1)),
                            context.MkLe(grid[row, col], context.MkInt(9))
                        ));
                    }
                }

                // Contraintes de ligne : toutes les valeurs de chaque ligne doivent être uniques
                for (int row = 0; row < 9; row++)
                {
                    for (int col1 = 0; col1 < 9; col1++)
                    {
                        for (int col2 = col1 + 1; col2 < 9; col2++)
                        {
                            constraints = context.MkAnd(constraints,
                                context.MkNot(context.MkEq(grid[row, col1], grid[row, col2]))
                            );
                        }
                    }
                }

                // Contraintes de colonne : toutes les valeurs de chaque colonne doivent être uniques
                for (int col = 0; col < 9; col++)
                {
                    for (int row1 = 0; row1 < 9; row1++)
                    {
                        for (int row2 = row1 + 1; row2 < 9; row2++)
                        {
                            constraints = context.MkAnd(constraints,
                                context.MkNot(context.MkEq(grid[row1, col], grid[row2, col]))
                            );
                        }
                    }
                }

                // Contraintes de boîte 3x3 : toutes les valeurs dans chaque boîte doivent être uniques
                for (int boxRow = 0; boxRow < 3; boxRow++)
                {
                    for (int boxCol = 0; boxCol < 3; boxCol++)
                    {
                        for (int row1 = 0; row1 < 3; row1++)
                        {
                            for (int col1 = 0; col1 < 3; col1++)
                            {
                                for (int row2 = row1; row2 < 3; row2++)
                                {
                                    for (int col2 = col1 + 1; col2 < 3; col2++)
                                    {
                                        constraints = context.MkAnd(constraints,
                                            context.MkNot(context.MkEq(grid[boxRow * 3 + row1, boxCol * 3 + col1], 
                                                                grid[boxRow * 3 + row2, boxCol * 3 + col2]))
                                        );
                                    }
                                }
                            }
                        }
                    }
                }

                // Remplir les valeurs existantes dans le Sudoku
                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        if (s.Cells[row, col] != 0) // Si la case n'est pas vide, ajouter une contrainte
                        {
                            constraints = context.MkAnd(constraints, context.MkEq(grid[row, col], context.MkInt(s.Cells[row, col])));
                        }
                    }
                }

                // Résoudre le Sudoku
                Solver solver = context.MkSolver();
                solver.Add(constraints);
                Status status = solver.Check();

                if (status == Status.SATISFIABLE)
                {
                    // Si une solution existe, extraire les valeurs et remplir la grille
                    Model model = solver.Model;
                    SudokuGrid solvedSudoku = new SudokuGrid();
                    for (int row = 0; row < 9; row++)
                    {
                        for (int col = 0; col < 9; col++)
                        {
                            solvedSudoku.Cells[row, col] = int.Parse(model.Evaluate(grid[row, col]).ToString());
                        }
                    }
                    return solvedSudoku;
                }
                else
                {
                    // Aucune solution trouvée
                    return null;
                }
            }
        }
    }
}
