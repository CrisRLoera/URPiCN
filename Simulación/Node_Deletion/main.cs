using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Windows.Media;


class Program
{
    static void Main()
    {
        // Inicialización de variables
        int n = 26; // Número de elementos o nodos
        double inct = 100000.0; // Número de incrementos para el tiempo
        double h = 25.0 / (inct - 1.0); // Paso de tiempo calculado

        // Ruta del archivo donde se encuentran los datos
        string filePath = "/home/crisdev/Escritorio/UACH/ProyectoUACH/Datasets/Anemona-fish-26-10";
        
        // Lectura del archivo y conversión de las líneas en una matriz de doubles
        Console.WriteLine("Leyendo los datos del archivo...");
        double[][] M = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line)) // Ignora líneas vacías
            .Select(line => line.Split('\t') // Divide cada línea por tabulaciones
                .Select(num => double.Parse(num)) // Convierte cada número en double
                .ToArray())
            .ToArray();

        Console.WriteLine($"Datos leídos: {M.Length} filas y {M[0].Length} columnas.");

        // Obtiene el tamaño de la matriz leída
        int n2 = M.Length; // Número de filas
        int m = M[0].Length; // Número de columnas
        List<double[]> X0_cond = new List<double[]>(); // Lista para almacenar condiciones iniciales
        X0_cond.Add(Enumerable.Repeat(0.001, n).ToArray()); // Agrega una condición inicial con todos los valores en 0.001
        
        // Calcula la matriz A basada en los datos leídos
        Console.WriteLine("Calculando la matriz A...");
        double[,] A = CalcularMatrizA(M, n2, m);

        // Parámetros para la función del modelo
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;

        // Listas para almacenar las fracciones eliminadas y las medias finales
        List<double> fractionsRemoved = new List<double>();
        List<double> yAverages = new List<double>();

        // Abre un StreamWriter para guardar los resultados en un archivo CSV
        using (StreamWriter writer = new StreamWriter("project.csv"))
        {
            // Itera sobre cada condición inicial
            foreach (var x0 in X0_cond)
            {
                Console.WriteLine("Procesando condición inicial...");
                // Crea copias de las condiciones iniciales
                double[] y = (double[])x0.Clone();
                double[] dydx = new double[n]; // Almacena las derivadas
                double[] yout = new double[n]; // Almacena los valores calculados en cada paso
                double x = 0.0; // Inicializa la variable de tiempo
                double[] lastYValues = new double[n]; // Almacena los últimos valores de y

                // Itera sobre el número de nodos a eliminar
                for (int removedNodes = 0; removedNodes <= n2; removedNodes++)
                {
                    Console.WriteLine($"Eliminando {removedNodes} nodos...");
                    double[,] modifiedA = RemoveNodesFromMatrix(A, removedNodes);
                    x = 0.0;
                    
                    // El problema está aquí - necesitamos ajustar los arrays al nuevo tamaño
                    int currentSize = n - removedNodes;
                    if (currentSize <= 0) break; // Evitar tamaños negativos o cero
                    
                    double[] currentY = new double[currentSize];
                    double[] currentDydx = new double[currentSize];
                    double[] currentYout = new double[currentSize];
                    
                    // Copiamos solo los elementos que necesitamos
                    Array.Copy(x0, currentY, currentSize);

                    // Usamos los nuevos arrays redimensionados
                    int iterationCount = 0;
                    bool allPositive = true;
                    double[] oldCurrentY = new double[currentSize];
                    while (allPositive && iterationCount < inct)
                    {
                        // Guardamos el estado anterior
                        Array.Copy(currentY, oldCurrentY, currentSize);
                        
                        rk4(currentY, currentDydx, currentSize, x, h, currentYout, 
                            (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, modifiedA, D, E, H));
                        Array.Copy(currentYout, currentY, currentSize);
                        x += h;

                        // Calculamos la norma del vector currentY
                        double normY = Math.Sqrt(currentY.Sum(v => v * v));
                        
                        // Calculamos la diferencia entre el estado anterior y el actual
                        double maxDiff = 0;
                        for (int i = 0; i < currentSize; i++)
                        {
                            maxDiff = Math.Max(maxDiff, Math.Abs(currentY[i] - oldCurrentY[i]));
                        }
                        
                        /*
                        if (normY < 1e-6 )
                        {
                            Console.WriteLine("Condición de paro norm de Y < 0");
                        }
                        else if(maxDiff < 1e-4)
                        {
                            Console.WriteLine("Condición de paro ||oldY - cY|| < 0");
                        }*/

                        // Nueva condición de paro
                        allPositive = !(normY < 1e-6 || maxDiff < 1e-4);
                        iterationCount++;
                    }

                    // Calculamos estadísticas con el array actual
                    double averageY = currentY.Average();
                    double fractionRemoved = (double)removedNodes / n;

                    //fractionsRemoved.Add(fractionRemoved);
                    //yAverages.Add(averageY);

                    double[] xeff = new double[currentSize];
                    
                    double[] vectorUnos = Enumerable.Repeat(1.0, currentSize).ToArray();
                    double[,] MatrizUnos = ConvertVectorToMatrizT(vectorUnos);
                    double[,] currentYtoM = ConvertVectorToMatriz(currentY);
                    /*
                    double[,] vectorTranspuesto = new double[n2, 1];
                    for (int i = 0; i < n2; i++)
                    {
                        vectorTranspuesto[i, 0] = vectorUnos[i];
                    }
                    */
                    double[,] oneA = MultiMatriz2(MatrizUnos,modifiedA);
                    double[,] numer = MultiMatriz2(oneA,currentYtoM);
                    Console.WriteLine("Elementos del vector numer:");
                    int filas = numer.GetLength(0);
                    int columnas = numer.GetLength(1);
                    Console.WriteLine($"Filas{filas} Columnas{columnas}");
                    for (int i = 0; i < filas; i++)
                    {
                        for (int j = 0; j < columnas; j++)
                        {
                            Console.WriteLine($"numer[{i},{j}] = {numer[i,j]}");
                        }
                    }



                    double Beff = new double();
                }
                
            }

            // Guarda los resultados en el archivo
            Console.WriteLine("Guardando resultados en el archivo CSV...");
            // Calcular el promedio de las soluciones de rk4
            /*for (int i = 0; i < fractionsRemoved.Count; i++)
            {
                writer.WriteLine($"{fractionsRemoved[i]},{yAverages[i]}"); // Escribe en el archivo CSV
            }*/
            //

            // 
        }

        // Indica que los datos han sido guardados
        Console.WriteLine("Datos guardados para graficar.");
    }

    static double[,] ConvertVectorToMatriz(double[] vector)
    {
        double[,] vectorToMatriz = new double[vector.Length,1];
        Console.WriteLine(vector.Length);
        for (int i = 0; i < vector.Length; i++)
        {
            vectorToMatriz[i, 0] = vector[i];
            Console.WriteLine(i);
        }
        return vectorToMatriz;
    }
    static double[,] ConvertVectorToMatrizT(double[] vector)
    {
        double[,] vectorToMatriz = new double[1,vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            vectorToMatriz[0, i] = vector[i];
        }
        return vectorToMatriz;
    }
    // Método para calcular la multilplicación de un a matriz por otra
    static double[,] MultiMatriz2(double[,] A, double[,] B)
    {
        Console.WriteLine($"Dimensiones de A: {A.GetLength(0)} x {A.GetLength(1)}");
        Console.WriteLine($"Dimensiones de B: {B.GetLength(0)} x {B.GetLength(1)}");
        
        int filasA = A.GetLength(0);
        int columnasA = A.GetLength(1);
        int columnasB = B.GetLength(1);
        

        if (columnasA != B.GetLength(0))
        {
            Console.WriteLine("Error: la matriz A no tiene la misma cantidad de columnas que B de filas");
            return null;
        }
        
        double[,] R = new double[filasA, columnasB];
        
        for (int i = 0; i < filasA; i++)
        {
            for (int j = 0; j < columnasB; j++)
            {
                for (int k = 0; k < columnasA; k++)
                {
                    R[i,j] += A[i,k] * B[k,j];
                }
            }
        }
        
        return R;
    }

    // Método para calcular la matriz A a partir de los datos M
    static double[,] CalcularMatrizA(double[][] M, int n2, int m)
    {
        double[,] A = new double[n2, n2]; // Inicializa la matriz A
        Console.WriteLine("Calculando los elementos de la matriz A...");
        for (int i = 0; i < n2; i++)
        {
            for (int j = 0; j < n2; j++)
            {
                double sum_k = 0.0; // Acumulador para la suma
                for (int k = 0; k < m; k++)
                {
                    double numerator = M[i][k] * M[j][k]; // Calcula el numerador
                    double denominator = M.Sum(row => row[k]); // Suma para el denominador
                    sum_k += numerator / denominator; // Actualiza la suma
                }
                A[i, j] = sum_k; // Asigna el valor a la matriz A
            }
        }
        Console.WriteLine("Matriz A calculada.");
        return A; // Retorna la matriz A
    }

    // Método para eliminar nodos de la matriz A
    static double[,] RemoveNodesFromMatrix(double[,] A, int removedNodes)
{
    int originalSize = A.GetLength(0);
    int newSize = originalSize - removedNodes;
    
    if (newSize <= 0) return new double[0,0];
    
    // Crear lista de índices disponibles
    List<int> availableIndices = Enumerable.Range(0, originalSize).ToList();
    Random random = new Random();
    
    // Seleccionar índices a mantener
    List<int> indicesToKeep = new List<int>();
    for (int i = 0; i < newSize; i++)
    {
        int randomIndex = random.Next(availableIndices.Count);
        indicesToKeep.Add(availableIndices[randomIndex]);
        availableIndices.RemoveAt(randomIndex);
    }
    
    // Ordenar los índices para mantener la estructura de la matriz
    indicesToKeep.Sort();
    
    // Crear nueva matriz con los nodos seleccionados
    double[,] newA = new double[newSize, newSize];
    for (int i = 0; i < newSize; i++)
    {
        for (int j = 0; j < newSize; j++)
        {
            newA[i,j] = A[indicesToKeep[i], indicesToKeep[j]];
        }
    }
    
    Console.WriteLine($"Creando nueva matriz A de tamaño {newSize} x {newSize} tras eliminar {removedNodes} nodos aleatorios...");
    return newA;
    }

    // Método de Runge-Kutta de cuarto orden
    static void rk4(double[] y, double[] dydx, int n, double x, double h, double[] yout, Action<double, double[], double[]> derivs)
    {
        double hh = h * 0.5; // Mitad del paso
        double h6 = h / 6.0; // Un sexto del paso
        double xh = x + hh; // Incrementa x en mitad del paso
        double[] dym = new double[n]; // Almacena derivadas medias
        double[] dyt = new double[n]; // Almacena derivadas temporales
        double[] yt = new double[n]; // Almacena los valores temporales de y

        // Calcula las derivadas para el primer medio paso
        for (int i = 0; i < n; i++) yt[i] = y[i] + hh * dydx[i];
        derivs(xh, yt, dyt);

        // Calcula las derivadas para el segundo medio paso
        for (int i = 0; i < n; i++) yt[i] = y[i] + hh * dyt[i];
        derivs(xh, yt, dym);

        // Calcula los valores de y para el siguiente paso
        for (int i = 0; i < n; i++)
        {
            yt[i] = y[i] + h * dym[i]; // Actualiza los valores temporales
            dym[i] += dyt[i]; // Suma las derivadas
        }

        // Calcula las derivadas finales
        derivs(x + h, yt, dyt);

        // Calcula el resultado final usando la fórmula de RK4
        for (int i = 0; i < n; i++)
            yout[i] = y[i] + h6 * (dydx[i] + 2.0 * dym[i] + 2.0 * dyt[i]); // Almacena el resultado
    }

    // Método que calcula la proyección
    static void ProjectFunction(double[] y, double[] dydx, double B, double K, double C, double[,] A, double D, double E, double H)
    {
        int n = y.Length; // Tamaño del vector
        for (int i = 0; i < n; i++)
        {
            double sum = 0.0; // Suma acumulada
            for (int j = 0; j < n; j++)
            {
                sum += A[i, j] * y[j]; // Suma los elementos de la matriz
            }

            // Calcula la derivada para el nodo i
            dydx[i] = B * sum - C * y[i] + H * E * (D - y[i]); // Ecuación de la proyección
        }
    }
}


// Eliminamos un nodo, ejecutamos varias iteraciones y calculamos la media de las x estables que pasan a formar mi <x>