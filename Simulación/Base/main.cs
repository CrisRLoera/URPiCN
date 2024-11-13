using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        double inct = 1000.0;
        double h = 25.0 / (inct - 1.0);
        
        // Importamos el dataset y lo almacenados en M de la forma exacta a la matriz del archivo
        string filePath = "/home/crisdev/Escritorio/UACH/ProyectoUACH/Datasets/Anemona-fish-26-10";
        double[][] M = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split('\t')
                .Select(num => double.Parse(num))
                .ToArray())
            .ToArray();

        /*  Se invierten las filas y las columnas para que la matriz quede de a forma n x m, donde
            n son las plantas y m los polinisadore 
        */
        M = M[0].Select((_, i) => M.Select(row => row[i]).ToArray()).ToArray();     

        // Definir los N y los M
        int n = M.Length; // Número de filas - plantas
        int m = M[0].Length; // Número de columnas - polinisadores

        // Imprimir la matriz M
        /*
        for(int i = 0; i < n ; i++ )
        {
            for(int j = 0; j < m ; j++ )
            {
                Console.Write($"{M[i][j]:F4}\t");
            }
            Console.Write("\n");

        }*/
        
        //Console.WriteLine("Se creó la matriz M");

        //Console.WriteLine($"Plantas: {n}");
        
        // Vector de condiciones iniciales
        List<double[]> X0_cond = new List<double[]>();
        // X_H = 6.0 Y X_L = 0.001

        X0_cond.Add(Enumerable.Repeat(0.001, n).ToArray());

        double[,] A = CalcularMatrizA(M, n,m);

        //Imprimir matriz A
        /*
        Console.WriteLine("\nMatriz A:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Console.Write($"{A[i,j]:F4}\t");
            }
            Console.Write("\n");
        }*/

        // Constantes para la ecuación diferencial
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;
        using (StreamWriter writer = new StreamWriter("base_all.csv"))
        {

            foreach (var x0 in X0_cond)
            {

                //Console.WriteLine($"X0: {string.Join(", ", x0.Select(val => $"{val:F4}"))}");

                double[] y = (double[])x0.Clone();
                
                double[] dydx = new double[n];
                double[] yout = new double[n];
                double x = 0.0;
                
                double[] lastYValues = new double[n];

                for (int i = 0; i < inct; i++)
                {
                    //Console.WriteLine($"\nIteración {i}:");
                    //Console.WriteLine($"Valores de y: {string.Join(", ", y.Select(val => $"{val:F4}"))}");
                    rk4(y, dydx, n, x, h, yout, (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, A, D, E, H));
                    
                    for (int j = 0; j < n; j++)
                        y[j] = yout[j];
                    x += h;

                    // Grabar en el archivo csv las n trayectorias
                    
                    string yValues = string.Join(",", y.Select(val => $"{val:F4}"));
                    writer.WriteLine($"{yValues}");

                    // Grabar en el archivo csv el promedio de las n trayectorias
                    /*
                    lastYValues = (double[])y.Clone();
                    double average = lastYValues.Average();
                    writer.WriteLine($"{average}");
                    */
                }
            }
        }

        Console.WriteLine("Datos guardados");
    }

    
    static double[,] CalcularMatrizA(double[][] M,int n_length,int m_length)
    {
        int n = n_length;
        int m = m_length;
        double[,] A = new double[n, n];
        double[] M_sum = new double[m];
        // Calcular M_sum - El cual da un vecotr de los denominadores usados para calcular A_ij
        for (int k = 0; k < m; k++)
        {
            double denominator = 0.0;

            for (int s = 0; s < n; s++)
            {
                denominator += M[s][k];
            }
            M_sum[k]=denominator;
        }
        // Imprimir vector M_sum
        /*
        for (int i = 0; i < m; i++)
        {    
            Console.Write($"M_sum {i}:{M_sum[i]:F4}\t");
        }*/
        /*
        De la proyección de una red bipartita n x m donde i es una planta y k un polinisador
        */  
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double sum_k = 0.0;

                for (int k = 0; k < m; k++)
                {
                    double numerator = M[i][k] * M[j][k];

                    sum_k += numerator / M_sum[k];
                }

                A[i, j] = sum_k;
            }
        }
        //Console.WriteLine("Se creó la matriz A");
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