using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        string directoryPathOri = $"./Database/{date_registry}";
        Directory.CreateDirectory(directoryPathOri);

        double[,] M_ori;
        int n; // Declarar n al principio
        using (var reader = new StreamReader("./interaction_matrix.txt"))
        {
            var lines = new List<string>();
            while (!reader.EndOfStream)
            {
                lines.Add(reader.ReadLine());
            }

            n = lines.Count; // Asignar el número de líneas a n
            M_ori = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                var values = lines[i].Split(' ');
                for (int j = 0; j < n; j++)
                {
                    M_ori[i, j] = double.Parse(values[j]);
                }
            }
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        // Main program

        double running_times = 1000000; // TODO: Miles de ejecuciones
        int m = M_ori.GetLength(1);
        
        double[,] A_ori = M_ori;

        double[] vector = new double[n];
        Random random = new Random();

        
        
        using (StreamWriter env_writer = new StreamWriter($"./Database/{date_registry}/env.txt"))
        {
            env_writer.WriteLine($"total species:{n}");
            env_writer.WriteLine($"Runs:{running_times}");
        }
        

        // Crear un diccionario para almacenar los resultados
        Dictionary<string, int> resultsDictionary = new Dictionary<string, int>();

        for (int i = 0; i <= running_times; i++)
        {
            for (int r = 0; r < n; r++)
            {
                vector[r] = GetRandomNumber(5.0,6.0); // Genera un vector con números aleatorios entre 0.0 y 1.0
            }
            

            double[,] A = new double[n, m];


            Array.Copy(A_ori, A, A_ori.Length);
            
            // Llamar a Run_program y almacenar el resultado
            string result = Run_program(n, A, vector);

            // Verificar si la clave ya existe en el diccionario
            if (resultsDictionary.ContainsKey(result))
            {
                resultsDictionary[result] += 1; // Sumar 1 al valor existente
            }
            else
            {
                resultsDictionary[result] = 1; // Inicializar el valor en 1
            }
        }

        // Guardar los resultados en un archivo CSV
        using (StreamWriter writer_z = new StreamWriter($"./Database/{date_registry}/zeros.txt"))
        using (StreamWriter writer = new StreamWriter($"./Database/{date_registry}/resultados.csv"))
        {
            writer.WriteLine("Resultado,Conteo"); // Escribir encabezados
            foreach (var entry in resultsDictionary)
            {
                // Verificar si el resultado contiene solo ceros
                int decimalValue = Convert.ToInt32(entry.Key, 2);
                if (entry.Key.All(c => c == '0')) {
                    writer_z.WriteLine($"{decimalValue},{entry.Value}");
                } else {
                    int num = 0;
                    
                    writer.WriteLine($"{decimalValue},{entry.Value}"); // Escribir cada resultado y su conteo
                }
            }
        }

        stopwatch.Stop();
        using (StreamWriter env_writer = new StreamWriter($"./Database/{date_registry}/env.txt", true))
        {
            env_writer.WriteLine($"Execution time(sec): {stopwatch.Elapsed.TotalSeconds}");
        }
        Console.WriteLine($"Tiempo de ejecución: {stopwatch.Elapsed.TotalSeconds} segundos");
    }
    static double GetRandomNumber(double minimum, double maximum)
    { 
        Random random = new Random();
        return random.NextDouble() * (maximum - minimum) + minimum;
    }

    static string Run_program(int n_in, double[,] A_in, double[] xi)
    {
        double[,] A = A_in;
        double inct = 1000.0;
        double h = 25.0 / (inct - 1.0);
        
        int n = n_in; // Número de filas - plantas

        // Constantes para la ecuación diferencial
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;


        double[] y = (double[])xi.Clone();

        
        double[] dydx = new double[n];
        double[] yout = new double[n];
        double x = 0.0;
        
        int[] binaryVector = new int[n]; // Crear el vector binario

        // Crear el vector binario
        string binaryVectorString = ""; // Inicializar la cadena

        for (int i = 0; i < inct; i++)
        {
            //Console.WriteLine($"\nIteración {i}:");
            //Console.WriteLine($"Valores de y: {string.Join(", ", y.Select(val => $"{val:F4}"))}");
            rk4(y, dydx, n, x, h, yout, (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, A, D, E, H));
            
            for (int j = 0; j < n; j++)
                y[j] = yout[j];
            x += h;

        }
        // Generar el vector binario basado en los valores de y
        for (int j = 0; j < n; j++)
        {
            binaryVector[j] = y[j] > 1 ? 1 : 0; // Asignar 1 si y[j] > 1, de lo contrario 0
        }

        // Convertir el vector binario a una cadena sin comas ni espacios
        binaryVectorString = string.Join("", binaryVector.Select(b => b.ToString()));

        
        //Console.WriteLine($"Datos guardados");
        return binaryVectorString;
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

    static void ProjectFunction(double[] xi, double[] dxdt, double B, double K, double C, double[,] A, double D, double E, double H)
    {
        int N = xi.Length;
        double growthTerm = 0;
        double sum = 0.0;
        double denominator = 0;
        double interactionTerm = 0;
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
            dxdt[i] = growthTerm + sum;
        }
    }
}
