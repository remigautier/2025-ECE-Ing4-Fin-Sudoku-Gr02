using System;
using System.Collections.Generic;
using System.Linq;
using Sudoku.Shared;

namespace Sudoku.RecuitSimule.Solvers
{
    public class RecuitSimuleSolverSimple : ISudokuSolver
    {
        private static Random random = new Random();

        public SudokuGrid Solve(SudokuGrid s)
        {
            // Cloner la grille d'origine et préparer le masque des cellules fixes
            int[,] grid = (int[,])s.Cells.Clone();
            bool[,] fixedCells = new bool[9, 9];
            
            // Remplir le masque des cellules fixes (cellules non vides initiales)
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    fixedCells[i, j] = grid[i, j] != 0;
                }
            }
            
            SolveWithSimulatedAnnealing(grid, fixedCells);
            return new SudokuGrid { Cells = grid };
        }

        private void SolveWithSimulatedAnnealing(int[,] grid, bool[,] fixedCells)
        {
            // Paramètres du recuit simulé
            double temperature = 1.0;
            double coolingRate = 0.995;
            int iterations = 100000;

            // Initialisation de la solution en respectant les cellules fixes
            int[,] currentSolution = InitializeSolution(grid, fixedCells);
            int[,] bestSolution = (int[,])currentSolution.Clone();
            int currentEnergy = ComputeEnergy(currentSolution);
            int bestEnergy = currentEnergy;

            for (int i = 0; i < iterations; i++)
            {
                // Créer une nouvelle solution modifiée (en respectant les cellules fixes)
                int[,] newSolution = MakeSmallChange(currentSolution, fixedCells);
                int newEnergy = ComputeEnergy(newSolution);

                // Accepter la nouvelle solution si elle est meilleure ou selon une probabilité
                if (newEnergy < bestEnergy || AcceptWorseSolution(currentEnergy, newEnergy, temperature))
                {
                    currentSolution = newSolution;
                    currentEnergy = newEnergy;
                    
                    // Mettre à jour la meilleure solution trouvée
                    if (currentEnergy < bestEnergy)
                    {
                        bestSolution = (int[,])currentSolution.Clone();
                        bestEnergy = currentEnergy;
                    }
                }

                // Refroidir le système
                temperature *= coolingRate;
                if (bestEnergy == 0) break; // Solution optimale trouvée
            }

            // Copier la meilleure solution dans la grille de résultat
            Array.Copy(bestSolution, grid, grid.Length);
        }

        // Initialise une solution valide en remplissant les cellules vides sans conflits dans les lignes
        private int[,] InitializeSolution(int[,] grid, bool[,] fixedCells)
        {
            int[,] solution = (int[,])grid.Clone();
            for (int i = 0; i < 9; i++)
            {
                // Trouver les nombres manquants dans la ligne
                HashSet<int> missingNumbers = new HashSet<int>(Enumerable.Range(1, 9));
                for (int j = 0; j < 9; j++)
                {
                    if (solution[i, j] != 0)
                        missingNumbers.Remove(solution[i, j]);
                }
                
                // Remplir les cellules non fixes avec les nombres manquants
                foreach (int j in Enumerable.Range(0, 9).Where(j => !fixedCells[i, j]))
                {
                    if (missingNumbers.Count > 0)
                    {
                        int number = missingNumbers.First();
                        solution[i, j] = number;
                        missingNumbers.Remove(number);
                    }
                }
            }
            return solution;
        }

        // Calcule l'énergie (nombre de conflits) de la grille
        private int ComputeEnergy(int[,] grid)
        {
            int conflicts = 0;
            
            // Conflits dans les lignes et colonnes
            for (int i = 0; i < 9; i++)
            {
                conflicts += CountConflicts(grid, i, true); // Lignes
                conflicts += CountConflicts(grid, i, false); // Colonnes
            }
            
            // Conflits dans les sous-grilles 3x3
            for (int i = 0; i < 9; i += 3)
            {
                for (int j = 0; j < 9; j += 3)
                {
                    conflicts += CountSubgridConflicts(grid, i, j);
                }
            }
            
            return conflicts;
        }

        // Compte les conflits dans une ligne ou une colonne
        private int CountConflicts(int[,] grid, int index, bool isRow)
        {
            HashSet<int> seen = new HashSet<int>();
            int conflicts = 0;
            for (int j = 0; j < 9; j++)
            {
                int num = isRow ? grid[index, j] : grid[j, index];
                if (seen.Contains(num)) conflicts++;
                seen.Add(num);
            }
            return conflicts;
        }

        // Compte les conflits dans une sous-grille 3x3
        private int CountSubgridConflicts(int[,] grid, int startRow, int startCol)
        {
            HashSet<int> seen = new HashSet<int>();
            int conflicts = 0;
            for (int i = startRow; i < startRow + 3; i++)
            {
                for (int j = startCol; j < startCol + 3; j++)
                {
                    int num = grid[i, j];
                    if (seen.Contains(num)) conflicts++;
                    seen.Add(num);
                }
            }
            return conflicts;
        }

        // Échange deux cellules non fixes dans la même ligne
        private int[,] MakeSmallChange(int[,] grid, bool[,] fixedCells)
        {
            int[,] newGrid = (int[,])grid.Clone();
            int row = random.Next(0, 9);
            
            // Collecter les indices des cellules modifiables dans cette ligne
            List<int> swappableColumns = new List<int>();
            for (int j = 0; j < 9; j++)
            {
                if (!fixedCells[row, j]) // Ne considère que les cellules non fixes
                    swappableColumns.Add(j);
            }
            
            // S'il y a moins de 2 cellules à échanger, retourner la grille originale
            if (swappableColumns.Count < 2)
                return newGrid;
            
            // Choisir deux cellules aléatoirement
            int colIndex1 = random.Next(swappableColumns.Count);
            int colIndex2;
            do
            {
                colIndex2 = random.Next(swappableColumns.Count);
            } while (colIndex1 == colIndex2);
            
            // Échanger les valeurs
            int col1 = swappableColumns[colIndex1];
            int col2 = swappableColumns[colIndex2];
            (newGrid[row, col1], newGrid[row, col2]) = (newGrid[row, col2], newGrid[row, col1]);
            
            return newGrid;
        }

        // Détermine si on accepte une solution moins bonne
        private bool AcceptWorseSolution(int currentEnergy, int newEnergy, double temperature)
        {
            if (newEnergy < currentEnergy) return true;
            double probability = Math.Exp((currentEnergy - newEnergy) / temperature);
            return random.NextDouble() < probability;
        }
    }
}