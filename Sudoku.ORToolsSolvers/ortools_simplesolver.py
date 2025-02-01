import numpy as np
from ortools.sat.python import cp_model
from timeit import default_timer

def solve_sudoku(grid):
    model = cp_model.CpModel()

    # Créer des variables pour chaque cellule du Sudoku (9x9)
    cells = [[model.NewIntVar(1, 9, f'cell_{i}_{j}') for j in range(9)] for i in range(9)]

    # Ajouter les contraintes pour les lignes et les colonnes
    for i in range(9):
        model.AddAllDifferent(cells[i])  # Lignes uniques
        model.AddAllDifferent([cells[j][i] for j in range(9)])  # Colonnes uniques

    # Ajouter les contraintes pour les sous-grilles 3x3
    for box_row in range(3):
        for box_col in range(3):
            box_cells = []
            for i in range(3):
                for j in range(3):
                    box_cells.append(cells[box_row * 3 + i][box_col * 3 + j])
            model.AddAllDifferent(box_cells)

    # Ajouter les contraintes pour les cases déjà remplies dans le Sudoku
    for i in range(9):
        for j in range(9):
            if grid[i, j] != 0:  # Si la cellule est déjà remplie
                # Capturer les indices dans des variables locales pour éviter tout problème de closure
                row = i
                col = j
                model.Add(cells[row][col] == grid[row, col])

    # Créer un solveur
    solver = cp_model.CpSolver()
    status = solver.Solve(model)

    # Si une solution est trouvée, mettre à jour la grille
    if status == cp_model.OPTIMAL or status == cp_model.FEASIBLE:
        for i in range(9):
            for j in range(9):
                grid[i, j] = solver.Value(cells[i][j])
        return True  # Sudoku résolu
    else:
        return False  # Aucune solution trouvée

# Définir `instance` uniquement si non déjà défini par PythonNET
if 'instance' not in locals():
    instance = np.array([
        [0, 0, 0, 0, 9, 4, 0, 3, 0],
        [0, 0, 0, 5, 1, 0, 0, 0, 7],
        [0, 8, 9, 0, 0, 0, 0, 4, 0],
        [0, 0, 0, 0, 0, 0, 2, 0, 8],
        [0, 6, 0, 2, 0, 1, 0, 5, 0],
        [1, 0, 2, 0, 0, 0, 0, 0, 0],
        [0, 7, 0, 0, 0, 0, 5, 2, 0],
        [9, 0, 0, 0, 6, 5, 0, 0, 0],
        [0, 4, 0, 9, 7, 0, 0, 0, 0]
    ], dtype=int)

start = default_timer()
# Exécuter la résolution du Sudoku avec OR-Tools
if solve_sudoku(instance):
    result = instance  # `result` sera utilisé pour récupérer la grille résolue depuis C#
    # print(result)
else:
    print("Aucune solution trouvée.")
execution = default_timer() - start
print("Le temps de résolution est de : ", execution * 1000, " ms")
