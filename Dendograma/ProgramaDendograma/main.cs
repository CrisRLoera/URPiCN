using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpeciesClass;
using DendrogramGeneratorClass;
/******************************************************************************

    Este programa simula la generación de un dendograma con una de dos
    distribuciones para generar los tiempos de aparición de las especies en
    el dendograma. Para posteriormente generar una matriz de interación de
    acuerdo a las distancias filogeneticas de las especies, esta matriz después
    se normaliza para evitar saltos muy grandes en las simulación numerica.

    La simulación numerica emplea una ecuacion diferencial resuelta mediante
    runge kutta de 4to orden, que simulan las interaciones del ecosistema.
    En cada iteración del programa principal se elimina una fración de las
    especies con el proposito de observar su reciliencia.

*******************************************************************************/

class Program
{
    static void Main(string[] args)
    {
        string date_registry = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string directoryPathLOri = $"./Database/{date_registry}";
        string directoryPathHOri = $"./Database/{date_registry}";
        string directoryPathLM;
        string directoryPathHM;
        Directory.CreateDirectory(directoryPathLOri);
        Directory.CreateDirectory(directoryPathHOri);
        DendrogramGenerator root = new DendrogramGenerator(30,60,directoryPathLOri);
        double[,] M_ori = root.GetMatrix();

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        // Main program

        double running_times = 1;
        int n = M_ori.GetLength(0);
        int m = M_ori.GetLength(1);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                M_ori[j, i] = M_ori[i,j];
            }
        }

        /*  Se invierten las filas y las columnas para que la matriz quede de a forma n x m, donde
            n son las plantas y m los polinisadores 
        */
        //M = M[0].Select((_, i) => M.Select(row => row[i]).ToArray()).ToArray();

        // Definir los N y los M
        n = M_ori.GetLength(0); // Número de filas - plantas
        m = M_ori.GetLength(1); // Número de columnas - polinisadores
        double delta_inct = n; // Delta_inct va a ser el número de nodos ya que son las veces que eliminamos nodos
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

        //double[,] A_ori = CalcularMatrizA(M_ori, n,m);
        for(int i = 0; i < n ; i++ )
        {
            for(int j = 0; j < m ; j++ )
            {
                Console.Write($"{M_ori[i,j]:F4}\t");
            }
            Console.Write("\n");

        }
        double[,] A_ori = M_ori;

        double[] list_xi = new double[] { 0.001, 6.0 };
        
        using (StreamWriter env_writer = new StreamWriter($"./Database/{date_registry}/env.txt"))
        {
            env_writer.WriteLine($"total species:{n}");
            env_writer.WriteLine($"Runs:{running_times}");
            env_writer.WriteLine($"Initial conditions: {string.Join(", ", list_xi)}");
        }
        

        for(int i = 0; i <= running_times; i++)
        {
            directoryPathLM = $"{directoryPathLOri}/run_{i}/";
            directoryPathHM = $"{directoryPathHOri}/run_{i}/";
            Directory.CreateDirectory(directoryPathLM);
            Directory.CreateDirectory(directoryPathHM);
            double[,] A = new double[n, m];
            double[,] M = new double[n, m];
            Array.Copy(A_ori, A, A_ori.Length);
            
            Array.Copy(M_ori, M, M_ori.Length);

            List<int> Ord = Enumerable.Range(0, n).ToList(); // Lista de orden de eliminación
            Ord = GenerarOrdenEliminacion(Ord);
            using (StreamWriter env_writer = new StreamWriter($"./Database/{date_registry}/env.txt", true))
            {
                env_writer.WriteLine($"Elimination Order of Run {i}: {string.Join(", ", Ord)}");
            }
            //Console.WriteLine($"Orden de eliminación: {string.Join(", ", Ord)}"); // Imprimir la lista en consola
            double delta_sum = 0.0;
            string directoryPathH, directoryPathL;
            using (StreamWriter writer_L = new StreamWriter($"./Database/{date_registry}/total_{i}_L.csv"))
            using (StreamWriter writer_H = new StreamWriter($"./Database/{date_registry}/total_{i}_H.csv"))
            {
                double avrg_L = 0.0;
                double avrg_H = 0.0;
                for(int j = 0; j <= delta_inct;j++)
                {
                    //A = CalcularMatrizA(M, n,m);
                    directoryPathL = $"{directoryPathLM}fn{j}-{n}/xL/";
                    directoryPathH = $"{directoryPathHM}fn{j}-{n}/xH/";
                    Directory.CreateDirectory(directoryPathL);
                    Directory.CreateDirectory(directoryPathH);
                    
                    avrg_L = Run_program(delta_sum,n,m,A,list_xi[0],directoryPathL,Ord,j);
                    avrg_H = Run_program(delta_sum,n,m,A,list_xi[1],directoryPathH,Ord,j);
                    //Console.WriteLine($"Promedio: {avrg_L}, Suma Delta: {delta_sum}"); // Imprimir los valores
                    //Console.WriteLine($"Promedio: {avrg_H}, Suma Delta: {delta_sum}"); // Imprimir los valores
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
            Console.WriteLine("\current_speciesatriz A:");
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
        using (StreamWriter gnu_writer_total = new StreamWriter($"./Database/{date_registry}/plot_x_fn.gnu"))
        {
            gnu_writer_total.WriteLine($"set terminal pngcairo size 1600,900 enhanced font \"Arial,20\" ");
            gnu_writer_total.WriteLine($"set output \'img_plot_avgx_fn.png\'");
            gnu_writer_total.WriteLine($"set grid");
            gnu_writer_total.WriteLine($"set xlabel \"f_n\" font \"Arial,20\" ");
            gnu_writer_total.WriteLine($"set ylabel \"avg_x\" font \"Arial,20\"");
            gnu_writer_total.WriteLine($"set datafile separator \",\"");
            gnu_writer_total.WriteLine($"plot for [i=0:{n}] sprintf(\"total_%d_L.csv\", i) using 2:1 with linespoints notitle, for [j=0:{n}] sprintf(\"total_%d_H.csv\", j) using 2:1 with linespoints notitle");

        }
        using (StreamWriter env_writer = new StreamWriter($"./Database/{date_registry}/env.txt", true))
        {
            env_writer.WriteLine($"Execution time(sec): {stopwatch.Elapsed.TotalSeconds}");
        }
        Console.WriteLine($"Tiempo de ejecución: {stopwatch.Elapsed.TotalSeconds} segundos");
    }

    static double Run_program(double delta, int n_in, int m_in, double[,] A_in, double xi, string path_save,List<int> ordDelList, int index)
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
        //Console.WriteLine($"Contenido de X0_cond: {string.Join(", ", X0_cond.Select(arr => $"[{string.Join(", ", arr)}]"))}");


        // Constantes para la ecuación diferencial
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;

        double average = 0.0;

        using (StreamWriter writer_i = new StreamWriter(Path.Combine(path_save,$"node_all.csv")))
        using (StreamWriter writer = new StreamWriter(Path.Combine(path_save,$"node_avg.csv")))
        {
            //writer_i.WriteLine($"{string.Join(", ", X0_cond[0].Select(val => $"{val:F4}"))}");
            //writer.WriteLine($"{xi}");
            foreach (var x0 in X0_cond)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < index; j++)
                    {
                        if (i == ordDelList[j])
                        {
                            x0[i] = 0;
                            //X0_cond[i] = new double[0];
                        }
                    }
                }

                //Console.WriteLine($"X0: {string.Join(", ", X0_cond[0].Select(val => $"{val:F4}"))}");

                double[] y = (double[])x0.Clone();
                //double[] y = (double[])X0_cond[0].Clone();

                
                double[] dydx = new double[n];
                double[] yout = new double[n];
                double x = 0.0;
                
                double[] lastYValues = new double[n];

                for (int i = 0; i < inct; i++)
                {
                    //Console.WriteLine($"\nIteración {i}:");
                    //Console.WriteLine($"Valores de y: {string.Join(", ", y.Select(val => $"{val:F4}"))}");
                    rk4(y, dydx, n, x, h, yout, (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, A, D, E, H,ordDelList, index));
                    
                    for (int j = 0; j < n; j++)
                        y[j] = yout[j];
                    x += h;
                    //Console.WriteLine($"Valores de y: {string.Join(", ", y.Select(val => $"{val:F4}"))}");
                    // Grabar en el archivo csv las n trayectorias
                    
                    string yValues = string.Join(",", y.Select(val => $"{val:F4}"));
                    writer_i.WriteLine($"{yValues}");

                    // Grabar en el archivo csv el promedio de las n trayectorias
                    lastYValues = (double[])y.Clone();
                    //Console.WriteLine($"Valores de lastYValues: {string.Join(", ", lastYValues)}");
                    average = lastYValues.Average();
                    writer.WriteLine($"{average}");
                }
            }
        }
        //Console.WriteLine($"Datos guardados");
        using (StreamWriter gnu_writer_all = new StreamWriter(Path.Combine(path_save,$"plot_all.gnu")))
        using (StreamWriter gnu_writer_avg = new StreamWriter(Path.Combine(path_save,$"plot_avg.gnu")))
        {
            gnu_writer_all.WriteLine($"set terminal pngcairo size 1600,900 enhanced font \"Arial,20\"");
            gnu_writer_avg.WriteLine($"set terminal pngcairo size 1600,900 enhanced font \"Arial,20\"");
            gnu_writer_all.WriteLine($"set output \'img_plot_all_node_deletion_{index}.png\'");
            gnu_writer_avg.WriteLine($"set output \'img_plot_avg_node_deletion_{index}.png\'");
            gnu_writer_all.WriteLine($"set grid");
            gnu_writer_avg.WriteLine($"set grid");
            gnu_writer_all.WriteLine($"set xlabel \"Iteraciones\" font \"Arial,20\"");
            gnu_writer_avg.WriteLine($"set xlabel \"Iteraciones\" font \"Arial,20\"");
            gnu_writer_all.WriteLine($"set ylabel \"x_i\" font \"Arial,20\"");
            gnu_writer_avg.WriteLine($"set ylabel \"x_i\" font \"Arial,20\"");
            gnu_writer_all.WriteLine($"set datafile separator \",\"");
            gnu_writer_avg.WriteLine($"set datafile separator \",\"");
            gnu_writer_all.WriteLine($"plot for [i=1:{n_in}] \'node_all.csv\' using ($0):i notitle");
            gnu_writer_avg.WriteLine($"plot \'node_avg.csv\' notitle");

        }
        if (double.IsNaN(average))
        {
            throw new InvalidOperationException("Error: El valor promedio es NaN.");
        }
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
        double[,] Mod_A = new double[A.GetLength(0), A.GetLength(1)];
        Array.Copy(A, Mod_A, A.Length);
        int n = A.GetLength(0);
        for(int j = 0; j < n;j++)
        {
            Mod_A[ord[index],j]=0;
            Mod_A[j,ord[index]]=0;
        }
        return Mod_A;
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
        for (i = 0; i < n; i++){
            yout[i] = y[i] + h6 * (dydx[i] + dyt[i] + 2.0 * dym[i]);
        }
    }

    static void ProjectFunction(double[] xi, double[] dxdt, double B, double K, double C, double[,] A, double D, double E, double H, List<int> ord, int index)
    {
        int N = xi.Length;
        double growthTerm = 0;
        double sum = 0.0;
        double denominator = 0;
        double interactionTerm = 0;
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < index; j++)
            {
                if (i == ord[j])
                {
                    xi[i] = 0;
                }
            }
        }
        for (int i = 0; i < N; i++)
        {
            sum = 0.0;
            for (int j = 0; j < N; j++)
            {
                denominator = D + (E * xi[i]) + (H * xi[j]);
                if (denominator == 0)
                {
                    Console.WriteLine("¡Alerta! El denominador es 0.");
                }
                interactionTerm = A[i, j] * ((xi[i] * xi[j]) / denominator);
                sum += interactionTerm;
            }
            
            growthTerm = xi[i] * (1 - (xi[i] / K)) * ((xi[i] / C )- 1);
            dxdt[i] = B + growthTerm + sum;
            for (int a = 0; a < N; a++)
            {
                for (int b = 0; b < index; b++)
                {
                    if (a == ord[b])
                    {
                        dxdt[a] = 0;
                    }
                }
            }
        }
    }
}
