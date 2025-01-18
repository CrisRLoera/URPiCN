using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        int nm = 0;
        int N = 0;
        int t = 0;
        //int gap = 50;
        int gap = 10000;
        int limit_species = 10;
        Species[] species = new Species[limit_species];
        species[0] = new Species(t,null);
        species[0].id = N;
        Species root = species[0];
        nm++;
        while(nm < limit_species) {
            Species[] speciesCopy = species.Clone() as Species[];
            for (int i = 0; i < limit_species ; i++) {
                if (speciesCopy[i] != null) {
                    //Console.WriteLine($"ESpecie{i}");
                    //Console.WriteLine($"Case1");
                    if (speciesCopy[i].created_sons == 0) {
                        if (t == speciesCopy[i].first_son_creation_time && nm < limit_species) {    
                            //Console.WriteLine($"Creando primer hijo en {i}");
                            species[i] = speciesCopy[i].CreateFirst(t);
                            speciesCopy[i].created_sons = speciesCopy[i].created_sons + 1;
                            N++;
                            species[i].id = N;
                        }
                    }
                    //Console.WriteLine($"Case2");
                    if (speciesCopy[i].father != null) {    
                        if (speciesCopy[i].father.created_sons == 1) {
                            if (t == speciesCopy[i].father.second_son_creation_time && nm < limit_species) {
                                //Console.WriteLine($"Creando segundo hijo en {i}:{n}");
                                species[nm] = speciesCopy[i].father.CreateSecond(t);                                
                                N++;
                                species[nm].id = N;
                                nm++;
                                speciesCopy[i].father.created_sons = speciesCopy[i].father.created_sons + 1;
                            }
                        }
                    }
                }
            }
            //Console.WriteLine($"Tiempo actual: {t}");
            //Thread.Sleep(1000);
            t++;
            //Console.WriteLine($"Número de especies: {nm}");            
        }
        Console.WriteLine($"Número total de nodos en el árbol: {root.ContarNodos()}");
        /*
        for (int i = 0; i < limit_species; i++) {
            if (species[i].father != null) {
                Console.WriteLine($"La especie {i}:{species[i].id} fue creada en {species[i].creation_time}");
                Console.WriteLine($"El padre es {species[i].father.creation_time}:{species[i].father.id}");
            } else {
                Console.WriteLine($"La especie {i}:{species[i].id} fue creada en {species[i].creation_time}");
                Console.WriteLine($"El padre es null");
            }
        }*/
        double[,] MO = new double[limit_species,limit_species];
        for (int i = 0; i < limit_species ; i++) {
            //Console.WriteLine($"> Especie {i}");
            for (int j = 0; j < limit_species; j++) {
                if(i != j) {
                    Species fix = species[i];
                    Species pointer = species[j];
                    bool isTheSame = false;
                    while(isTheSame != true) {
                        //Console.WriteLine($"fix:{fix.id} pointer:{pointer.id}");
                        if (fix == pointer && pointer !=null) {
                            isTheSame = true;
                        }
                        if (isTheSame!= true) {
                            if(pointer.father == null) {
                                if(fix.father != null) {
                                    fix = fix.father;
                                    pointer = species[j];
                                    //Console.WriteLine("Not fund");
                                }
                                else {
                                    Console.WriteLine("Fatal error");
                                }
                            } else {
                                pointer = pointer.father;
                            }
                        }
                    }
                    //Console.WriteLine($"{i}:{species[i].creation_time} + {j}:{species[j].creation_time} - {gap}");
                    //Console.WriteLine($"father:{fix.creation_time}");
                    MO[i,j] = (species[i].creation_time - pointer.creation_time) + (species[j].creation_time - pointer.creation_time) - gap;

                } else {
                    MO[i,j] = 0;
                }
            }
        }
        Console.WriteLine("Matriz M:");
        for (int i = 0; i < limit_species; i++) {
            for (int j = 0; j < limit_species; j++) {
                Console.Write($"{MO[i, j]} "); // Imprimir cada elemento con 3 decimales
                
            }
            Console.WriteLine(); // Nueva línea al final de cada fila
        }
        // Normalice
            // Buscar el más grande
        Console.WriteLine("Matriz M:");
        double theBiggest = 0;
        for (int i = 0; i < limit_species; i++) {
            for (int j = 0; j < limit_species; j++) {
                if (theBiggest < Math.Abs(MO[i,j])) {
                    theBiggest = MO[i,j];
                }
                
            }
        }
        Console.WriteLine($"El valor mas grande es {theBiggest}");
        for (int i = 0; i < limit_species; i++) {
            for (int j = 0; j < limit_species; j++) {
            
                MO[i,j] = MO[i,j]/theBiggest;
                
            }
        }
        // Imprimir la matriz M
        
        Console.WriteLine("Matriz M Normalizada:");
        for (int i = 0; i < limit_species; i++) {
            for (int j = 0; j < limit_species; j++) {
                Console.Write($"{MO[i, j]:F3} "); // Imprimir cada elemento con 3 decimales
                
            }
            Console.WriteLine(); // Nueva línea al final de cada fila
        }
        //Thread.Sleep(15000);

        /*
        bool exitCondition = false;
        Species positionCurrent = root;

        while(exitCondition != true) {
            Console.WriteLine("Menu:");
            Console.WriteLine(" ");
            Console.WriteLine("1: Izquierda");
            Console.WriteLine("2: Derecha");
            Console.WriteLine("3: Arriba");
            Console.WriteLine(" ");
            Console.WriteLine("Datos del nodo:");
            
            Console.WriteLine($"id:{positionCurrent.id}");
            Console.WriteLine($"creation:{positionCurrent.creation_time}");
            
            Console.WriteLine("Padre");
            if (positionCurrent.father != null) {
                Console.WriteLine($"    creacion:{positionCurrent.father.creation_time}");
                Console.WriteLine($"    id:{positionCurrent.father.id}");
            } else {
                Console.WriteLine("    No existe padre");
            }
            Console.WriteLine("Hijo izquierdo");
            if (positionCurrent.first_son != null) {
                Console.WriteLine($"    creacion:{positionCurrent.first_son.creation_time}");
                Console.WriteLine($"    id:{positionCurrent.first_son.id}");
            } else {
                Console.WriteLine("    No existe hijo");
            }
            Console.WriteLine("Hijo derecho");
            if (positionCurrent.second_son != null) {
                Console.WriteLine($"    creacion:{positionCurrent.second_son.creation_time}");
                Console.WriteLine($"    id:{positionCurrent.second_son.id}");
            } else {
                Console.WriteLine("    No existe hijo");
            }
            
            string option = Console.ReadLine();

            if(option == "1" ) {
                if (positionCurrent.first_son != null) {
                    positionCurrent = positionCurrent.first_son;
                } else {
                    Console.WriteLine("No existe hijo a la izquierda");
                }
            } else if (option == "2") {
                if (positionCurrent.second_son != null) {
                    positionCurrent = positionCurrent.second_son;
                } else {
                    Console.WriteLine("No existe hijo a la derecha");
                }               
            } else if (option == "3") {
                if (positionCurrent.father == null){
                    Console.WriteLine("No existe un padre");
                } else {
                    positionCurrent = positionCurrent.father;
                }
            }
            else if (option == "e") {
                exitCondition = true;
            }
        }
        Console.WriteLine("Saliendo del programa");
        */

        // Main program

        double running_times = 20;

        // Importamos el dataset y lo almacenados en M de la forma exacta a la matriz del archivo
        /*
        string filePath = "/home/crisdev/Escritorio/UACH/ProyectoUACH/Datasets/Anemona-fish-26-10";
        double[][] tempM = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split('\t')
                .Select(num => double.Parse(num))
                .ToArray())
            .ToArray();

        // Convertir de double[][] a double[,]
        */
        double[,] M_ori = MO;
        int n = M_ori.GetLength(0);
        int m = M_ori.GetLength(1);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                M_ori[j, i] = MO[i,j];
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
        double[,] A_ori = MO;

        double[] list_xi = new double[] { 0.001, 6.0 };
        string directoryPathL = $"./xL";
        string directoryPathH = $"./xH";
        Directory.CreateDirectory(directoryPathL);
        Directory.CreateDirectory(directoryPathH);

        for(int i = 0; i <= running_times; i++)
        {
            double[,] A = new double[n, m];
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
                    //A = CalcularMatrizA(M, n,m);
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


        // Constantes para la ecuación diferencial
        double B = 0.1, C = 1.0, K = 5.0, D = 5.0, E = 0.9, H = 0.1;

        double average = 0.0;

        using (StreamWriter writer_i = new StreamWriter(Path.Combine(path_save,$"node_i_{f_n}.csv")))
        using (StreamWriter writer = new StreamWriter(Path.Combine(path_save,$"node_f_{f_n}.csv")))
        {

            foreach (var x0 in X0_cond)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < index; j++)
                    {
                        if (i == ordDelList[j])
                        {
                            x0[i] = 0;
                        }
                    }
                }

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
                    rk4(y, dydx, n, x, h, yout, (xh, yt, dyt) => ProjectFunction(yt, dyt, B, K, C, A, D, E, H,ordDelList, index),index);
                    
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
        //Console.WriteLine("Se creó la matriz A");

        // Imprimir matriz A
        /*
        Console.WriteLine("Matriz A:");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                Console.Write($"{A[i,j]:F4}\t");
            }
            Console.WriteLine();
        }*/
        
        //bool isConnected = conectivity(A);
        //Console.WriteLine(isConnected ? "La red A es conexa" : "La red A no es conexa");
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
        }*/
        
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
        /*
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
        */

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
    
    static void rk4(double[] y, double[] dydx, int n, double x, double h, double[] yout, Action<double, double[], double[]> derivs,int index)
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
            /*
            if (double.IsNaN(yout[i]))
            {
                throw new InvalidOperationException($"Error: El valor yout {i} es NaN.");
            }*/
            yout[i] = y[i] + h6 * (dydx[i] + dyt[i] + 2.0 * dym[i]);
            //if (index == n){Console.WriteLine($"{yout[i]}");} // Imprime los yout de la ultima iteración
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
                interactionTerm = A[i, j] * ((xi[i] * xi[j]) / denominator);
                sum += interactionTerm;
            }
            
            growthTerm = xi[i] * (1 - (xi[i] / K)) * ((xi[i] / C )- 1);
            dxdt[i] = growthTerm + sum;
            
            //Console.WriteLine($"\nEcuación para especie {i}:");
            //Console.WriteLine($"dxdt[{i}] = [{xi[i]} * (1 - {xi[i]}/{K}) * ({xi[i]}/{C} - 1)] + {sum}");
            //Console.WriteLine($"dxdt[{i}] = {growthTerm} + {sum} = {dxdt[i]}\n");
        }
        // System.Threading.Thread.Sleep(30000); // 30000 milisegundos = 30 segundos
    }
}
class Species {
    public int id;
    public int creation_time;
    public Species father;
    public Species first_son = null;
    public int first_son_creation_time;
    public Species second_son = null;
    public int second_son_creation_time;
    public int lambda_sons = 5;
    public int created_sons = 0;

    public Species(int tiempo, Species father) {
        this.creation_time = tiempo;
        this.father = father;
        int temp1 = tiempo + GenerarPLaw();
        int temp2 = tiempo + GenerarPLaw();
        //int temp1 = tiempo + GenerarPoisson(this.lambda_sons) + 1;
        //int temp2 = tiempo + GenerarPoisson(this.lambda_sons) + 1;

        if (temp1 < temp2) {
            this.first_son_creation_time = temp1;
            this.second_son_creation_time = temp2;
        } else {
            this.first_son_creation_time = temp2;
            this.second_son_creation_time = temp1;
        }
        Console.WriteLine("Una nueva especie nacio en el tiempo:" + tiempo);
        //Console.WriteLine($"Número generado para el primer hijo: {this.first_son_creation_time}");
        //Console.WriteLine($"Número generado para el segundo hijo: {this.second_son_creation_time}");
    }
    static int GenerarPLaw()
    {
        Random random = new Random();
        double x = random.NextDouble();
        double alpha = 2.5;
        double min = 0.0;
        double max = 10000.0;
        double Py = min + (max - min) * (1 - Math.Pow(x, 1 / alpha));
        return (int)Math.Floor(Py);
    }
    static int GenerarPoisson(double lambda)
    {
        Random random = new Random();

        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;

        do
        {
            k++;
            p *= random.NextDouble();
        } while (p > L);

        return k - 1;
    }

    public Species CreateFirst(int t){
        this.first_son = new Species(this.first_son_creation_time,this);
        return this.first_son;
    }

    public Species CreateSecond(int t){
        this.second_son = new Species(this.second_son_creation_time,this);
        return this.second_son;
    }

    // Método para contar nodos
    public int ContarNodos()
    {
        int count = 1; // Contar el nodo actual
        if (first_son != null)
        {
            count += first_son.ContarNodos(); // Contar el hijo izquierdo
        }
        if (second_son != null)
        {
            count += second_son.ContarNodos(); // Contar el hijo derecho
        }
        return count;
    }
}


