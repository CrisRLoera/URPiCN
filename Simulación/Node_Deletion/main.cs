using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        double running_times = 20;

        // Importamos el dataset y lo almacenados en M de la forma exacta a la matriz del archivo
        string filePath = "/home/crisdev/Escritorio/UACH/ProyectoUACH/Datasets/Anemona-fish-26-10";
        double[][] tempM = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split('\t')
                .Select(num => double.Parse(num))
                .ToArray())
            .ToArray();

        // Convertir de double[][] a double[,]
        int n = tempM.Length;
        int m = tempM[0].Length;
        double[,] M_ori = new double[n, m];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                M_ori[i, j] = tempM[i][j];
            }
        }

        /*  Se invierten las filas y las columnas para que la matriz quede de a forma n x m, donde
            n son las plantas y m los polinisadores 
        */
        

        // Definir los N y los M
        n = M_ori.GetLength(0); // Número de filas - plantas
        m = M_ori.GetLength(1); // Número de columnas - polinisadores
        
        // Imprimir la matriz M
        /*
        for(int i = 0; i < n ; i++ )
        {
            for(int j = 0; j < m ; j++ )
            {
                Console.Write($"{M_ori[i,j]:F4}\t");
            }
            Console.Write("\n");

        }*/
        
        double delta_inct = n; // Delta_inct va a ser el número de nodos ya que son las veces que eliminamos nodos
        
        //Console.WriteLine("Se creó la matriz M");

        //Console.WriteLine($"Plantas: {n}");

        //double[,] A_ori = CalcularMatrizA(M_ori, n,m);
        double[,] A_ori = new double[n, n];

        double[] list_xi = new double[] { 0.001, 6.0 };

        for(int i = 0; i <= running_times; i++)
        {
            double[,] A = new double[n, n];
            double[,] M = new double[n, m];
            Array.Copy(A_ori, A, A_ori.Length);
            
            Array.Copy(M_ori, M, M_ori.Length);

            List<int> Ord = Enumerable.Range(0, n).ToList(); // Lista de orden de eliminación
            Ord = GenerarOrdenEliminacion(Ord);
            //Console.WriteLine($"Orden de eliminación: {string.Join(", ", Ord)}"); // Imprimir la lista en consola
            double delta_sum = 0.0;
            using (StreamWriter writer_L = new StreamWriter($"total_{i}_L.csv"))
            using (StreamWriter writer_H = new StreamWriter($"total_{i}_H.csv"))
            {
                double avrg_L = 0.0;
                double avrg_H = 0.0;
                for(int j = 0; j <= delta_inct;j++)
                {
                    A = CalcularMatrizA(M, n,m);
                    avrg_L = Run_program(delta_sum,n,m,A,list_xi[0]);
                    avrg_H = Run_program(delta_sum,n,m,A,list_xi[1]);
                    Console.WriteLine($"Promedio: {avrg_L}, Suma Delta: {delta_sum}"); // Imprimir los valores
                    Console.WriteLine($"Promedio: {avrg_H}, Suma Delta: {delta_sum}"); // Imprimir los valores
                    writer_L.WriteLine($"{avrg_L},{delta_sum}");
                    writer_H.WriteLine($"{avrg_H},{delta_sum}");
                    delta_sum += (1/delta_inct);
                    if (j < Ord.Count) // Verificar si hay más nodos para eliminar
                    {
                        A = EliminarNodosA(A, Ord, j);
                        //M = EliminarNodosM(M,Ord,j);
                        
                    }
                }

            }
            Console.WriteLine($"Se termino la ejecución: {i}");
            

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
        }
        stopwatch.Stop();
        Console.WriteLine($"Tiempo de ejecución: {stopwatch.Elapsed.TotalSeconds} segundos");
    }

    static double Run_program(double delta, int n_in, int m_in, double[,] A_in, double xi)
    {
        double f_n = delta;
        double[,] A = A_in;
        //Console.WriteLine($"Delta:{f_n}");
        double inct = 1000.0;
        double h = 25.0 / (inct - 1.0);
        
        int n = n_in; // Número de filas - plantas
        int m = m_in; // Número de columnas - polinisadores
        
        // Vector de condiciones iniciales
        List<double[]> X0_cond = new List<double[]>();
        // X_H = 6.0 Y X_L = 0.001

        X0_cond.Add(Enumerable.Repeat(xi, n).ToArray());

        // Constantes para la ecuación diferencial
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;

        double average = 0.0;

        //using (StreamWriter writer_i = new StreamWriter($"node_i_{f_n}.csv"))
        //using (StreamWriter writer = new StreamWriter($"node_f_{f_n}.csv"))
        //{

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
                    //writer_i.WriteLine($"{yValues}");

                    // Grabar en el archivo csv el promedio de las n trayectorias
                    lastYValues = (double[])y.Clone();
                    average = lastYValues.Average();
                    //writer.WriteLine($"{average}");
                }
            }
        //}
        //Console.WriteLine($"Datos guardados");
        return average;
    }

    
    static double[,] CalcularMatrizA(double[,] M,int n_length,int m_length)
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
                denominator += M[s,k];
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
                    double numerator = M[i,k] * M[j,k];
                    //Console.WriteLine($"{numerator}");
                    if (numerator / M_sum[k] != Double.NaN){
                        if(numerator != 0 && M_sum[k]!= 0){
                            sum_k += numerator / M_sum[k];
                            //Console.WriteLine($"{numerator}/{M_sum[k]}={numerator / M_sum[k]} S:{sum_k}");
                        }
                    }
                }

                A[i, j] = sum_k;
            }
        }
        Console.WriteLine("Se creó la matriz A");

        // Imprimir matriz A
        
        Console.WriteLine("Matriz A:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Console.Write($"{A[i,j]:F4}\t");
            }
            Console.WriteLine();
        }
        /*
        bool isConnected = conectivity(A);
        Console.WriteLine(isConnected ? "La red A es conexa" : "La red A no es conexa");*/
        return A;
    }

    static List<int> GenerarOrdenEliminacion(List<int> indx_list)
    {
        List<int> schuffled = new List<int>();
        Random random = new Random((int)DateTime.Now.Ticks);

        // Crear una copia de la lista original para no modificarla
        List<int> tempList = new List<int>(indx_list);

        while (tempList.Count > 0)
        {
            // Elegir un índice aleatorio
            int index = random.Next(tempList.Count);
            // Quitar el número aleatorio y agregarlo a schuffled
            schuffled.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return schuffled;
    }

    static double[,] EliminarNodosA(double[,] A, List<int> ord, int index)
    {
        double[,] mod_A = new double[A.GetLength(0), A.GetLength(1)];
        Array.Copy(A, mod_A, A.Length);
        int n = A.GetLength(0);
        for(int j = 0; j < n;j++)
        {
            mod_A[ord[index],j]=0;
            mod_A[j,ord[index]]=0;
        }
        //Imprimir matriz mod_A
        /*
        Console.WriteLine("\nMatriz mod_A:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Console.Write($"{mod_A[i,j]:F4}\t");
            }
            Console.Write("\n");
        }
        */
        //Console.WriteLine($"Total de nodos eliminados: {deleted_nodes}");
        //bool isConnected = conectivity(mod_A);
        //Console.WriteLine(isConnected ? "La red A es conexa" : "La red A no es conexa");
        return mod_A;
    }

    static double[,] EliminarNodosM(double[,] M, List<int> ord, int index)
    {
        double[,] mod_M = new double[M.GetLength(0), M.GetLength(1)];
        for (int i = 0; i < M.GetLength(0); i++)
        {
            for (int j = 0; j < M.GetLength(1); j++)
            {
                mod_M[i, j] = M[i, j];
            }
        }
        int n = M.GetLength(0);
        int m = M.GetLength(1);
        for(int j = 0; j < m;j++)
        {
            mod_M[ord[index],j]=0;
        }
        //Imprimir matriz mod_M
        Console.WriteLine("\nMatriz mod_M:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                Console.Write($"{mod_M[i,j]:F0} ");
            }
            Console.Write("\n");
        }
        bool isConnected = conectivity(mod_M);
        Console.WriteLine(isConnected ? "La red es conexa" : "La red no es conexa");
        
        //Console.WriteLine($"Total de nodos eliminados: {deleted_nodes}");
        return mod_M;
    }

    static bool conectivity(double[,] M)
    {
        int n = M.GetLength(0);
        bool[] visited = new bool[n];

        // Inicializar el arreglo de visitados
        for (int i = 0; i < n; i++)
        {
            visited[i] = false;
        }

        // Iniciar DFS desde el primer nodo
        DFS(0, M, visited);

        // Verificar si todos los nodos fueron visitados
        foreach (bool isVisited in visited)
        {
            if (!isVisited)
            {
                return false; // La red no es conexa
            }
        }

        return true; // La red es conexa
    }

    // Método auxiliar DFS
    static void DFS(int node, double[,] M, bool[] visited)
    {
        visited[node] = true;
        int n = M.GetLength(0);

        for (int neighbor = 0; neighbor < n; neighbor++)
        {
            // Si hay una conexión y el nodo no ha sido visitado
            if (M[node, neighbor] != 0 && !visited[neighbor])
            {
                DFS(neighbor, M, visited);
            }
        }
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