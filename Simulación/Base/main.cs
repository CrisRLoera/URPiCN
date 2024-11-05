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
        // inct = 10;

        string filePath = "/home/crisdev/Escritorio/UACH/ProyectoUACH/Datasets/Plant-Pollinator-96-276";
        double[][] M = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split('\t')
                .Select(num => double.Parse(num))
                .ToArray())
            .ToArray();

        Console.WriteLine("Se creó la matriz M");
        int n2 = M.Length; // Número de filas
        int m = M[0].Length; // Número de columnas
        
        List<double[]> X0_cond = new List<double[]>();
        X0_cond.Add(Enumerable.Repeat(0.001, 26).ToArray());

        double[,] A = CalcularMatrizA(M, n2,m);
        
        Console.WriteLine("Se creó la matriz A");
        Console.WriteLine("\nMatriz A:");
        for (int i = 0; i < n2; i++)
        {
            for (int j = 0; j < n2; j++)
            {
                Console.Write($"{A[i,j]:F4}\t");
            }
            Console.WriteLine();
        }
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;

        using (StreamWriter writer = new StreamWriter("project.csv"))
        {

            foreach (var x0 in X0_cond)
            {
                Console.WriteLine("X0");
                Console.WriteLine($"X0: {string.Join(", ", x0.Select(val => $"{val:F4}"))}");

                double[] y = (double[])x0.Clone();
                
                double[] dydx = new double[n];
                double[] yout = new double[n];
                double x = 0.0;
                
                double[] lastYValues = new double[n];

                for (int i = 0; i < inct; i++)
                {
                    Console.WriteLine($"\nIteración {i}:");
                    Console.WriteLine($"Valores de y: {string.Join(", ", y.Select(val => $"{val:F4}"))}");
                    rk4(y, dydx, n, x, h, yout, (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, A, D, E, H));
                    
                    for (int j = 0; j < n; j++)
                        y[j] = yout[j];
                    x += h;
                    string yValues = string.Join(",", y.Select(val => $"{val:F4}"));
                    writer.WriteLine($"{yValues}");

                    lastYValues = (double[])y.Clone();
                }

                double average = lastYValues.Average();
                Console.WriteLine($"\nPromedio de los últimos valores de y: {average:F4}");
                //writer.WriteLine($"\nPromedio de los últimos valores de y: {average:F4}");

            }
        }

        Console.WriteLine("Save");
    }

    static double[,] CalcularMatrizA(double[][] M,int n2_length,int m_length)
    {
        int n2 = n2_length;
        int m = m_length;
        double[,] A = new double[n2, n2];

        for (int i = 0; i < n2; i++)
        {
            for (int j = 0; j < n2; j++)
            {
                double sum_k = 0.0;

                for (int k = 0; k < m; k++)
                {
                    double numerator = M[i][k] * M[j][k];
                    double denominator = 0.0;

                    for (int s = 0; s < n2; s++)
                    {
                        denominator += M[s][k];
                    }

                    sum_k += numerator / denominator;
                }

                A[i, j] = sum_k;
            }
        }
        Console.WriteLine("Se creó la matriz A");
        return A;
    }

    static void rk4(double[] y, double[] dydx, int n, double x, double h, double[] yout, Action<double, double[], double[]> derivs)
    {
        int i;
        double xh, hh, h6;
        double[] dym = new double[n];
        double[] dyt = new double[n];
        double[] yt = new double[n];

        hh = h * 0.5;
        h6 = h / 6.0;
        xh = x + hh;

        for (i = 0; i < n; i++) yt[i] = y[i] + hh * dydx[i];
        derivs(xh, yt, dyt);

        for (i = 0; i < n; i++) yt[i] = y[i] + hh * dyt[i];
        derivs(xh, yt, dym);

        for (i = 0; i < n; i++)
        {
            yt[i] = y[i] + h * dym[i];
            dym[i] += dyt[i];
        }

        derivs(x + h, yt, dyt);

        for (i = 0; i < n; i++)
            yout[i] = y[i] + h6 * (dydx[i] + dyt[i] + 2.0 * dym[i]);
    }

    static void ProjectFunction(double[] xi, double[] dxdt, double B, double K, double C, double[,] A, double D, double E, double H)
    {
        int N = xi.Length;

        for (int i = 0; i < N; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < N; j++)
            {
                double interactionTerm = A[i, j] * ((xi[i] * xi[j]) / (D + (E * xi[i]) + (H * xi[j])));
                sum += interactionTerm;
//                Console.WriteLine($"Interacción [{i},{j}]: A[{i},{j}] * ({xi[i]} * {xi[j]}) / ({D} + ({E} * {xi[i]}) + ({H} * {xi[j]})) = {interactionTerm}");
            }
            
            double growthTerm = xi[i] * (1 - (xi[i] / K)) * ((xi[i] / C )- 1);
            dxdt[i] = B + growthTerm + sum;
            
//            Console.WriteLine($"\nEcuación para especie {i}:");
//            Console.WriteLine($"dxdt[{i}] = {B} + [{xi[i]} * (1 - {xi[i]}/{K}) * ({xi[i]}/{C} - 1)] + {sum}");
//            Console.WriteLine($"dxdt[{i}] = {B} + {growthTerm} + {sum} = {dxdt[i]}\n");
        }
        // System.Threading.Thread.Sleep(30000); // 30000 milisegundos = 30 segundos
    }
}