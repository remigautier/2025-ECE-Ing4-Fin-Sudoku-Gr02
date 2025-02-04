using Sudoku.Shared;
using Tensorflow;
using static Tensorflow.Binding;
using System;
using System.Linq;

namespace Sudoku.NeuralNetworkSolver
{
    public class NeuralNetworkSolver : ISudokuSolver, IDisposable
    {
        private readonly Graph _graph;
        private readonly Session _session;
        private readonly Tensor inputs;
        private readonly Tensor output;

        public NeuralNetworkSolver()
        {
            // Initialisation du modèle de réseau de neurones
            _graph = new Graph();
            _session = new Session(_graph);

            // Construire le modèle ici
            BuildModel();
        }

        private void BuildModel()
        {
            // Défini les dimensions de l'entrée et de la sortie
            var inputShape = new int[] { 1, 9, 9, 1 };  // Grille Sudoku 9x9
            var outputShape = new int[] { 1, 9, 9, 9 }; // Probabilité de chaque chiffre dans chaque cellule

            // Placeholders pour les entrées et les sorties
            inputs = tf.placeholder(tf.float32, shape: inputShape);
            var labels = tf.placeholder(tf.float32, shape: outputShape);

            // Réseau de neurones convolutif (Exemple simple)
            var conv1 = tf.layers.conv2d(inputs, 32, 3, activation: tf.nn.relu);
            var pool1 = tf.layers.max_pooling2d(conv1, 2, 2);
            var flatten = tf.layers.flatten(pool1);
            var dense1 = tf.layers.dense(flatten, 128, activation: tf.nn.relu);
            output = tf.layers.dense(dense1, 9, activation: tf.nn.softmax);

            // Fonction de perte et optimiseur
            var loss = tf.reduce_mean(tf.nn.softmax_cross_entropy_with_logits(labels: labels, logits: output));
            var optimizer = tf.train.AdamOptimizer(learning_rate: 0.001).minimize(loss);

            // Sauvegarde le modèle dans une variable
            _graph.OperationByName("dense_1/Relu").Op.Name = "relu_1"; // Forcer le nom pour l'accès ultérieur
        }

        public SudokuGrid Solve(SudokuGrid s)
        {
            // Préparer les données d'entrée sous la forme attendue par le modèle
            var input = PrepareInput(s);
            var feedDict = new FeedItem[]
            {
                new FeedItem(inputs, input)
            };

            // Exécuter la prédiction
            var predictions = _session.run(output, feed_dict: feedDict);

            // Convertir les résultats en grille Sudoku
            SudokuGrid result = ConvertPredictionsToSudoku(predictions);

            return result;
        }

        private float[,] PrepareInput(SudokuGrid grid)
        {
            // Transformer la grille en format compatible avec TensorFlow (1, 9, 9, 1)
            float[,] input = new float[9, 9];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    input[i, j] = grid.Cells[i, j] == 0 ? -1 : grid.Cells[i, j];  // -1 pour les cellules vides
                }
            }

            return input;
        }

        private SudokuGrid ConvertPredictionsToSudoku(Tensor predictions)
        {
            // Récupérer les prédictions du modèle
            var predictionArray = predictions.Data<float>();
            var resultGrid = new SudokuGrid();

            // Remplir la grille avec les résultats du modèle
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    // Trouver l'indice du chiffre avec la probabilité la plus élevée
                    var cellProbabilities = predictionArray[i * 9 + j];  // Probabilités pour chaque chiffre de 1 à 9
                    int predictedValue = Array.IndexOf(cellProbabilities, cellProbabilities.Max()) + 1; // +1 pour que l'index commence à 1
                    resultGrid.Cells[i, j] = predictedValue;
                }
            }

            return resultGrid;
        }

        // Méthode Dispose pour libérer les ressources de TensorFlow
        public void Dispose()
        {
            _session?.Dispose();
            _graph?.Dispose();
        }
    }
}
