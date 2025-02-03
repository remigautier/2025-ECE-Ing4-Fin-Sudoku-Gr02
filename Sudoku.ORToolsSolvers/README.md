# Sudoku OR-Tools Solver

Ce projet contient des implémentations de solveurs de Sudoku utilisant **Google OR-Tools**. Cette bibliothèque puissante permet de résoudre des problèmes complexes en combinant la programmation par contraintes (CP), la programmation linéaire (LP) et la programmation par nombres entiers mixtes (MIP). Les implémentations proposées démontrent la flexibilité et l'efficacité d'OR-Tools pour résoudre des grilles de Sudoku.

## Solvers OR-Tools

### 1. Solveur en C# : `ORToolsSimpleSolver`

Le solveur `ORToolsSimpleSolver` utilise la bibliothèque OR-Tools pour C# (via le package NuGet `Google.OrTools`) pour modéliser les contraintes et résoudre la grille de Sudoku.

- **Points clés** :
  - Déclaration des variables pour chaque cellule de la grille.
  - Ajout de contraintes pour les lignes, colonnes et blocs 3x3 afin d'assurer l'unicité des valeurs.
  - Intégration des cellules pré-remplies comme contraintes supplémentaires.
  - Utilisation du solveur CP pour résoudre la grille et mettre à jour les cellules avec les valeurs trouvées.

- **Code source** : [`ORToolsSimpleSolver.cs`](./ORToolsSimpleSolver.cs)

### 2. Solveur avec Python.NET : `OrToolsSimplePythonSolver`

Le solveur `OrToolsSimplePythonSolver` exécute un script Python utilisant OR-Tools via **Python.NET**, permettant une flexibilité accrue pour manipuler les contraintes directement en Python.

- **Points clés** :
  - Modélisation des contraintes via le script Python.
  - Appel du solveur OR-Tools pour résoudre la grille.
  - Récupération de la solution dans l'environnement C# pour mettre à jour les cellules.

- **Code source** : [`OrToolsSimplePythonSolver.cs`](./OrToolsSimplePythonSolver.cs)

- **Script Python** : [`ortools_simplesolver.py`](./ortools_simplesolver.py)

## Comparaison avec Backtracking

- **Programmation par contraintes vs Backtracking** :
  - Le backtracking explore toutes les solutions possibles récursivement, tandis qu'OR-Tools utilise une approche basée sur les contraintes, bien plus efficace pour éliminer rapidement des solutions impossibles.
  - Les solveurs OR-Tools permettent d'exploiter des techniques avancées comme l'élimination propagative, les heuristiques de recherche et l'optimisation multi-objectifs.

- **Simplicité de gestion des contraintes** :
  - Dans OR-Tools, les contraintes sont définies de manière déclarative et restent lisibles même pour des modèles complexes.
  - OR-Tools prend en charge des contraintes supplémentaires sans alourdir l'implémentation, contrairement au backtracking où chaque contrainte doit être explicitement ajoutée dans la logique de parcours.

- **Performance** :
  - OR-Tools excelle sur des grilles difficiles ou partiellement remplies, où les contraintes complexes sont essentielles pour réduire l'espace de recherche.

## Configuration et environnement

### Prérequis

- .NET 9.0 ou supérieur
- Python 3.x (pour le solveur Python.NET)
- Bibliothèques Python : `ortools`

### Installation des dépendances Python

Pour exécuter le solveur Python, installez les dépendances nécessaires :

```bash
pip install ortools
```

### Bloc `.csproj`

Pour intégrer le script Python dans le répertoire de sortie, ajoutez le bloc suivant au fichier `.csproj` :

```xml
<ItemGroup>
    <None Update="ortools_simplesolver.py">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

## Extensions possibles pour les solveurs OR-Tools

Pour pousser plus loin l'exploration, voici des idées d'extensions basées sur d'autres moteurs ou fonctionnalités avancées d'OR-Tools :

- **Moteurs alternatifs** :
  - **Solveur par contraintes historiques (Legacy CP)** : Bien que migré vers le moteur SAT, ce solveur reste disponible pour explorer des approches alternatives.
  - **Programmation linéaire (LP)** : Modélisez le Sudoku comme un problème d'optimisation linéaire avec des contraintes relaxées pour expérimenter des résolutions approximatives.
  - **Programmation par entiers mixtes (MIP)** : Combinez les avantages des solveurs linéaires et non linéaires pour traiter des contraintes plus complexes.

- **Paramètres avancés** :
  - Explorez les paramètres comme :
    - Nombre maximum d'itérations.
    - Heuristiques de choix des variables (`CpSolverParameters`).
    - Limites de temps pour optimiser les performances.
  - Par exemple, l'utilisation de plusieurs workers (`num_workers`) pour paralléliser la résolution peut réduire significativement le temps de calcul.

- **Optimisation multi-objectifs** :
  - Ajoutez des objectifs secondaires comme la minimisation du nombre de valeurs pré-remplies nécessaires pour résoudre la grille, ou maximisez la symétrie de la solution.


Ces pistes permettent d'explorer la puissance d'OR-Tools tout en élargissant les compétences en programmation par contraintes et en optimisation.
L'idéal pour leur mise en oeuvre est l'utilisation d'une classe de base mutualisant le code réutilisé avec des classes héritées personnalisant certains aspects de l'exécution.
