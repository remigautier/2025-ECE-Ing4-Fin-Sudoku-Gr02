using Sudoku.Shared;
using Microsoft.Z3;
using System;
using System.Collections.Generic;

namespace sudoku.Z3Solver
{
    public class Z3BitVectorSimpleSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            using (Context ctx = new Context())
            {
                var solverParams = ctx.MkParams();
                solverParams.Add("auto_config", true);
                solverParams.Add("smt.arith.solver", 2);
                solverParams.Add("smt.mbqi", false); // Désactiver MBQI pour améliorer l'efficacité
                solverParams.Add("timeout", 5000); // Timeout de 5 secondes

                Solver solver = ctx.MkSolver();
                solver.Parameters = solverParams;

                BitVecExpr[,] cells = DeclareVariables(ctx, solver);
                AddSudokuConstraints(ctx, solver, cells);
                FixInitialValues(ctx, solver, s, cells);

                return SolveSudoku(solver, cells, s);
            }
        }

        private BitVecExpr[,] DeclareVariables(Context ctx, Solver solver)
        {
            BitVecExpr[,] cells = new BitVecExpr[9, 9];
            List<BoolExpr> domainConstraints = new List<BoolExpr>();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    cells[row, col] = ctx.MkBVConst($"cell_{row}_{col}", 4);
                    domainConstraints.Add(ctx.MkAnd(
                        ctx.MkBVULE(ctx.MkBV(1, 4), cells[row, col]),
                        ctx.MkBVULE(cells[row, col], ctx.MkBV(9, 4))
                    ));
                }
            }
            solver.Add(ctx.MkAnd(domainConstraints)); // Ajout en une seule opération
            return cells;
        }

        private void AddSudokuConstraints(Context ctx, Solver solver, BitVecExpr[,] cells)
        {
            List<BoolExpr> constraints = new List<BoolExpr>();

            for (int i = 0; i < 9; i++)
            {
                constraints.Add(ctx.MkDistinct(GetRow(cells, i)));
                constraints.Add(ctx.MkDistinct(GetColumn(cells, i)));
            }

            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    constraints.Add(ctx.MkDistinct(GetBox(cells, boxRow, boxCol)));
                }
            }

            solver.Add(ctx.MkAnd(constraints));
        }

        private void FixInitialValues(Context ctx, Solver solver, SudokuGrid s, BitVecExpr[,] cells)
        {
            List<BoolExpr> fixedValues = new List<BoolExpr>();

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (s.Cells[row, col] != 0)
                    {
                        fixedValues.Add(ctx.MkEq(cells[row, col], ctx.MkBV(s.Cells[row, col], 4)));
                    }
                }
            }
            solver.Add(ctx.MkAnd(fixedValues)); // Ajout groupé pour optimiser
        }

        private SudokuGrid SolveSudoku(Solver solver, BitVecExpr[,] cells, SudokuGrid s)
        {
            if (solver.Check() == Status.SATISFIABLE)
            {
                SudokuGrid solvedGrid = s.CloneSudoku();
                Model model = solver.Model;

                for (int row = 0; row < 9; row++)
                {
                    for (int col = 0; col < 9; col++)
                    {
                        solvedGrid.Cells[row, col] = (int)((BitVecNum)model.Eval(cells[row, col], true)).UInt64;
                    }
                }
                return solvedGrid;
            }
            else
            {
                throw new Exception("Aucune solution trouvée pour cette grille.");
            }
        }

        private Expr[] GetRow(BitVecExpr[,] grid, int row)
        {
            Expr[] rowValues = new Expr[9];
            for (int col = 0; col < 9; col++)
            {
                rowValues[col] = grid[row, col];
            }
            return rowValues;
        }

        private Expr[] GetColumn(BitVecExpr[,] grid, int col)
        {
            Expr[] colValues = new Expr[9];
            for (int row = 0; row < 9; row++)
            {
                colValues[row] = grid[row, col];
            }
            return colValues;
        }

        private Expr[] GetBox(BitVecExpr[,] grid, int boxRow, int boxCol)
        {
            List<Expr> boxValues = new List<Expr>(9);
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
