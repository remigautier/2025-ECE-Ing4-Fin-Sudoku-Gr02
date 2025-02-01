using Sudoku.Shared;
using Microsoft.Z3;
using System;
using System.Collections.Generic;

namespace sudoku.Z3Solver
{
    public class Z3Solver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            // Initialisation du contexte Z3
            using (Context ctx = new Context())
            {
                Solver solver = ctx.MkSolver();

                // Étape 1 : Déclarer les variables
                IntExpr[,] cells = DeclareVariables(ctx, solver);

                // Étape 2 : Ajouter les contraintes du Sudoku
                AddSudokuConstraints(ctx, solver, cells);

                // Étape 3 : Ajouter les contraintes des valeurs connues
                FixInitialValues(ctx, solver, s, cells);

                // Étape 4 : Résolution du Sudoku
                return SolveSudoku(solver, cells, s);
            }
        }

        // Déclarer les variables du Sudoku sous forme d'entiers (1-9)
        private IntExpr[,] DeclareVariables(Context ctx, Solver solver)
        {
            IntExpr[,] cells = new IntExpr[9, 9];

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    cells[row, col] = (IntExpr)ctx.MkIntConst($"cell_{row}_{col}");
                    // Chaque cellule doit être un nombre entre 1 et 9
                    solver.Add(ctx.MkAnd(ctx.MkLe(ctx.MkInt(1), cells[row, col]), ctx.MkLe(cells[row, col], ctx.MkInt(9))));
                }
            }

            return cells;
        }

        // Ajouter les contraintes du Sudoku (lignes, colonnes, blocs 3x3)
        private void AddSudokuConstraints(Context ctx, Solver solver, IntExpr[,] cells)
        {
            for (int i = 0; i < 9; i++)
            {
                solver.Add(ctx.MkDistinct(GetRow(cells, i)));
                solver.Add(ctx.MkDistinct(GetColumn(cells, i)));
            }

            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    solver.Add(ctx.MkDistinct(GetBox(cells, boxRow, boxCol)));
                }
            }
        }

        // Fixer les valeurs données dans la grille initiale
        private void FixInitialValues(Context ctx, Solver solver, SudokuGrid s, IntExpr[,] cells)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (s.Cells[row, col] != 0)
                    {
                        solver.Add(ctx.MkEq(cells[row, col], ctx.MkInt(s.Cells[row, col])));
                    }
                }
            }
        }

        // Résoudre le Sudoku et retourner la grille mise à jour
        private SudokuGrid SolveSudoku(Solver solver, IntExpr[,] cells, SudokuGrid s)
        {
            if (solver.Check() == Status.SATISFIABLE)
            {
                SudokuGrid solvedGrid = s.CloneSudoku();
                Model model = solver.Model;

                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        solvedGrid.Cells[row, col] = int.Parse(model.Eval(cells[row, col]).ToString());
                    }
                }

                return solvedGrid;
            }
            else
            {
                throw new Exception("Impossible de résoudre le Sudoku.");
            }
        }

        // Méthodes auxiliaires pour récupérer les lignes, colonnes et blocs
        private Expr[] GetRow(IntExpr[,] grid, int row)
        {
            Expr[] rowValues = new Expr[9];
            for (int col = 0; col < 9; col++)
            {
                rowValues[col] = grid[row, col];
            }
            return rowValues;
        }

        private Expr[] GetColumn(IntExpr[,] grid, int col)
        {
            Expr[] colValues = new Expr[9];
            for (int row = 0; row < 9; row++)
            {
                colValues[row] = grid[row, col];
            }
            return colValues;
        }

        private Expr[] GetBox(IntExpr[,] grid, int boxRow, int boxCol)
        {
            List<Expr> boxValues = new List<Expr>();
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    boxValues.Add(grid[3 * boxRow + row, 3 * boxCol + col]);
                }
            }
            return boxValues.ToArray();
        }
    }
}
