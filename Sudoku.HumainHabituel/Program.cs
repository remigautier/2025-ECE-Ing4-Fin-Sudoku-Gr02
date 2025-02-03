using System.Linq; 
using Python.Runtime;
using Sudoku.Shared;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace Sudoku.HumainHabituel
{
    public class HumainHabituelPythonSolver : PythonSolverBase
    {
        public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
        {
            //System.Diagnostics.Debugger.Break();

            //For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
            //using (Py.GIL())
            //{
            // create a Python scope
            using (PyModule scope = Py.CreateScope())
            {

                // Injectez le script de conversion
                AddNumpyConverterScript(scope);

                // Convertissez le tableau .NET en tableau NumPy
                var pyCells = AsNumpyArray(s.Cells, scope);

                // create a Python variable "instance"
                scope.Set("instance", pyCells);

                // run the Python script
                string code = Resources.humain_py;
                scope.Exec(code);

                PyObject result = scope.Get("result");

                // Convertissez le résultat NumPy en tableau .NET
                var managedResult = AsManagedArray(scope, result);

                return new SudokuGrid() { Cells = managedResult };
            }
        }
    }
}