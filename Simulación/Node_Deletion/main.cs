using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        int n = 26;
        double inct = 1000.0;
        double h = 25.0 / (inct - 1.0);

        string filePath = "/home/crisdev/Escritorio/UACH/ProyectoUACH/Datasets/Plant-Pollinator-96-276";
        double[][] M = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split('\t')
                .Select(num => double.Parse(num))
                .ToArray())
            .ToArray();

        int n2 = M.Length;
        int m = M[0].Length;
        List<double[]> X0_cond = new List<double[]>();
        X0_cond.Add(Enumerable.Repeat(0.001, n).ToArray());
        double[,] A = CalcularMatrizA(M, n2, m);

        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;

        // Listas para almacenar las fracciones eliminadas y medias finales
        List<double> fractionsRemoved = new List<double>();
        List<double> yAverages = new List<double>();

        using (StreamWriter writer = new StreamWriter("project.csv"))
        {
            foreach (var x0 in X0_cond)
            {
                double[] y = (double[])x0.Clone();
                double[] dydx = new double[n];
                double[] yout = new double[n];
                double x = 0.0;
                double[] lastYValues = new double[n];

                for (int removedNodes = 0; removedNodes <= n; removedNodes++)
                {
                    // Crear matriz A con nodos eliminados
                    double[,] modifiedA = RemoveNodesFromMatrix(A, removedNodes);
                    x = 0.0;
                    Array.Copy(x0, y, n);  // Resetear valores iniciales para cada iteración

                    for (int i = 0; i < inct; i++)
                    {
                        rk4(y, dydx, n, x, h, yout, (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, modifiedA, D, E, H));
                        Array.Copy(yout, y, n);
                        x += h;
                    }

                    // Calcular la media final de y[i] y la fracción eliminada
                    lastYValues = (double[])y.Clone();
                    double averageY = lastYValues.Average();
                    double fractionRemoved = (double)removedNodes / n;

                    // Guardar datos para graficar
                    fractionsRemoved.Add(fractionRemoved);
                    yAverages.Add(averageY);
                }
            }

            // Guardar los resultados en un archivo
            for (int i = 0; i < fractionsRemoved.Count; i++)
            {
                writer.WriteLine($"{fractionsRemoved[i]},{yAverages[i]}");
            }
        }

        Console.WriteLine("Datos guardados para graficar.");
    }

    static double[,] CalcularMatrizA(double[][] M, int n2, int m)
    {
        double[,] A = new double[n2, n2];
        for (int i = 0; i < n2; i++)
        {
            for (int j = 0; j < n2; j++)
            {
                double sum_k = 0.0;
                for (int k = 0; k < m; k++)
                {
                    double numerator = M[i][k] * M[j][k];
                    double denominator = M.Sum(row => row[k]);
                    sum_k += numerator / denominator;
                }
                A[i, j] = sum_k;
            }
        }
        return A;
    }

    static double[,] RemoveNodesFromMatrix(double[,] A, int removedNodes)
    {
        int newSize = A.GetLength(0) - removedNodes;
        double[,] newA = new double[newSize, newSize];
        for (int i = 0; i < newSize; i++)
            for (int j = 0; j < newSize; j++)
                newA[i, j] = A[i, j];
        return newA;
    }

    static void rk4(double[] y, double[] dydx, int n, double x, double h, double[] yout, Action<double, double[], double[]> derivs)
    {
        double hh = h * 0.5;
        double h6 = h / 6.0;
        double xh = x + hh;
        double[] dym = new double[n];
        double[] dyt = new double[n];
        double[] yt = new double[n];

        for (int i = 0; i < n; i++) yt[i] = y[i] + hh * dydx[i];
        derivs(xh, yt, dyt);

        for (int i = 0; i < n; i++) yt[i] = y[i] + hh * dyt[i];
        derivs(xh, yt, dym);

        for (int i = 0; i < n; i++)
        {
            yt[i] = y[i] + h * dym[i];
            dym[i] += dyt[i];
        }

        derivs(x + h, yt, dyt);

        for (int i = 0; i < n; i++)
            yout[i] = y[i] + h6 * (dydx[i] + dyt[i] + 2.0 * dym[i]);
    }

    static void ProjectFunction(double[] xi, double[] dxdt, double B, double K, double C, double[,] A, double D, double E, double H)
    {
        int N = xi.Length;
        for (int i = 0; i < N; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < N; j++)
                sum += A[i, j] * ((xi[i] * xi[j]) / (D + (E * xi[i]) + (H * xi[j])));
            dxdt[i] = B + xi[i] * (1 - xi[i] / K) * (xi[i] / C - 1) + sum;
        }
    }
}
