using System;
using Sudoku.Shared;
using DlxLib;

namespace Sudoku.DlxLib
{
    public class DlxLibSolver : ISudokuSolver
    {
        private DLXNode header;
        private DLXNode[] columns;
        private System.Collections.Generic.List<DLXNode> solution;

        public DlxLibSolver()
        {
            // Initialisation des membres
            header = new DLXNode();
            columns = new DLXNode[324];
            solution = new System.Collections.Generic.List<DLXNode>();
            for (int i = 0; i < 324; i++)
            {
                columns[i] = new DLXNode();
            }
        }

        public SudokuGrid Solve(SudokuGrid s)
        {
            // Réinitialiser les liens entre les nœuds
            ResetLinks();

            // Créer la matrice de contraintes
            CreateMatrix(s);

            // Résoudre le Sudoku avec l'algorithme DLX
            if (Search(0))
            {
                // Si une solution est trouvée, reconstruire le tableau de Sudoku
                SudokuGrid solvedGrid = new SudokuGrid();
                foreach (var node in solution)
                {
                    int rowId = node.RowId;
                    int cell = rowId / 9;
                    int num = rowId % 9 + 1;
                    int i = cell / 9;
                    int j = cell % 9;
                    solvedGrid.Cells[i, j] = num;
                }
                return solvedGrid;
            }
            return s; // Retourner le grid inchangé si aucune solution n'a été trouvée
        }

        private void ResetLinks()
        {
            header = new DLXNode();
            for (int i = 0; i < 324; i++)
            {
                columns[i] = new DLXNode();
            }
            solution.Clear();
        }

        private void CreateMatrix(SudokuGrid s)
        {
            // Création des colonnes
            DLXNode current = header;
            foreach (var col in columns)
            {
                col.Right = current.Right;
                col.Left = current;
                current.Right.Left = col;
                current.Right = col;
                current = col;
            }

            // Ajout des lignes pour chaque contrainte
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int num = s.Cells[i, j];
                    foreach (var n in (num == 0 ? new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 } : new int[] { num }))
                    {
                        int rowId = (i * 9 + j) * 9 + (n - 1);
                        int[] constraints = new int[]
                        {
                            i * 9 + j,                 // Contrainte cellule
                            81 + i * 9 + (n - 1),      // Contrainte ligne
                            162 + j * 9 + (n - 1),     // Contrainte colonne
                            243 + (i / 3 * 3 + j / 3) * 9 + (n - 1)  // Contrainte région
                        };
                        AddRow(rowId, constraints);
                    }
                }
            }
        }

        private void AddRow(int rowId, int[] colIndices)
        {
            DLXNode? firstNode = null;
            foreach (var colIdx in colIndices)
            {
                var colNode = columns[colIdx];
                var newNode = new DLXNode
                {
                    RowId = rowId,
                    Column = colNode
                };

                // Lien vertical
                newNode.Down = colNode;
                newNode.Up = colNode.Up;
                colNode.Up.Down = newNode;
                colNode.Up = newNode;

                // Lien horizontal
                if (firstNode != null)
                {
                    newNode.Right = firstNode;
                    newNode.Left = firstNode.Left;
                    firstNode.Left.Right = newNode;
                    firstNode.Left = newNode;
                }
                else
                {
                    firstNode = newNode;
                }

                colNode.Size++;
            }
        }

        private bool Search(int k)
        {
            if (header.Right == header)
            {
                return true;
            }

            var col = SelectColumn();
            Cover(col);

            var row = col.Down;
            while (row != col)
            {
                solution.Add(row);
                var rightNode = row.Right;
                while (rightNode != row)
                {
                    Cover(rightNode.Column);
                    rightNode = rightNode.Right;
                }

                if (Search(k + 1))
                {
                    return true;
                }

                solution.RemoveAt(solution.Count - 1);
                var leftNode = row.Left;
                while (leftNode != row)
                {
                    Uncover(leftNode.Column);
                    leftNode = leftNode.Left;
                }

                row = row.Down;
            }

            Uncover(col);
            return false;
        }

        private DLXNode SelectColumn()
        {
            var col = header.Right;
            int minSize = int.MaxValue;
            DLXNode selected = col;
            while (col != header)
            {
                if (col.Size < minSize)
                {
                    minSize = col.Size;
                    selected = col;
                }
                col = col.Right;
            }
            return selected; 
        }

        private void Cover(DLXNode col)
        { 
            col.Right.Left = col.Left;
            col.Left.Right = col.Right;

            var row = col.Down;
            while (row != col)
            {
                var rightNode = row.Right;
                while (rightNode != row)
                {
                    rightNode.Up.Down = rightNode.Down;
                    rightNode.Down.Up = rightNode.Up;
                    rightNode.Column.Size--;
                    rightNode = rightNode.Right;
                }
                row = row.Down;
            }
        }

        private void Uncover(DLXNode col)
        {
            var row = col.Up;
            while (row != col)
            {
                var leftNode = row.Left;
                while (leftNode != row)
                {
                    leftNode.Up.Down = leftNode;
                    leftNode.Down.Up = leftNode;
                    leftNode.Column.Size++;
                    leftNode = leftNode.Left;
                }
                row = row.Up;
            }

            col.Right.Left = col;
            col.Left.Right = col;
        }
    }

    public class DLXNode
    {
        public DLXNode Left { get; set; }
        public DLXNode Right { get; set; }
        public DLXNode Up { get; set; }
        public DLXNode Down { get; set; }
        public DLXNode Column { get; set; }
        public int Size { get; set; }
        public int RowId { get; set; }

        public DLXNode()
        {
            Left = Right = Up = Down = this;
            Column = this; // Initialize Column property
        }
    }
}