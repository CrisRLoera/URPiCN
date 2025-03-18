using System;
using System.Linq;
using System.IO;
using SpeciesClass;
using System.Collections.Generic;
using System.Text.Json;

namespace DendrogramGeneratorClass
{
    class DendrogramGenerator
    {
        private System.Diagnostics.Stopwatch stopwatch;
        private int current_species = 0;
        private int N = 0;
        private int t = 0;
        public int gap = 0;
        public int limit_species = 10;
        public Species[] species;
        public double[,] M_d;
        public Species root = null;
        public int[,] interaction_ids;


        public DendrogramGenerator(int custom_gap, int custom_limit_species, string path)
        {
            this.gap = custom_gap;
            this.limit_species = custom_limit_species;
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
            this.species = new Species[limit_species];
            this.M_d = new double[limit_species, limit_species];
            this.interaction_ids = new int[limit_species, limit_species];
            double k_nodes = 0;
            if (this.limit_species % 2 == 0)
            {
                k_nodes = limit_species - 1;
            } else {
                k_nodes = (limit_species - 1)+ 0.5;
            }
            this.root = GenerateCompleteBinaryTree((int)((2*(k_nodes))+1));
            GenerateMatrix(path);
            SaveMatrixTXTFile(path);
            Directory.CreateDirectory(path);
            SaveNewickFile($"{path}/tree.nwk");
            SaveMatrixIDFile(path);
            Species.SaveToJson(this.root, path);
            Console.WriteLine($"Tiempo de ejecución: {this.stopwatch.Elapsed.TotalSeconds} segundos");
        }

        public double[,] GetMatrix()
        {
            return this.M_d;
        }


        public Species GenerateCompleteBinaryTree(int n)
        {
            Console.WriteLine($"Num Nodes in tree:{n}");
            if (n <= 0) return null;

            // Crear la raíz con tiempo inicial 0
            Species root = new Species(0,0, null);
            int id_count = 0;
            root.id = id_count;
            Queue<Species> queue = new Queue<Species>();
            queue.Enqueue(root);
            int count = 1;
            List<Species> terminalNodes = new List<Species>(); // Lista temporal para hojas

            // Generar nodos hasta alcanzar n
            while (count < n)
            {
                Species current = queue.Dequeue();
                bool isLeaf = true;

                // Crear primer hijo si aún no llegamos a n
                if (count < n)
                {
                    Species first = current.CreateFirst(current.creation_time);
                    id_count++;
                    first.id = id_count;
                    queue.Enqueue(first);
                    count++;
                    isLeaf = false;
                }

                // Crear segundo hijo si aún no llegamos a n
                if (count < n)
                {
                    Species second = current.CreateSecond(current.creation_time);
                    queue.Enqueue(second);
                    id_count++;
                    second.id = id_count;
                    count++;
                    isLeaf = false;
                }

                // Si el nodo no creó hijos, es una hoja
                if (isLeaf)
                {
                    Console.WriteLine($"Hoja");
                    terminalNodes.Add(current);
                }
            }
            Console.WriteLine($"Elementos en la cola antes de Dequeue: {queue.Count}");
            // Guardar los nodos de la cola en una lista
            List<Species> allNodes = new List<Species>(queue);

            // Guardar los nodos terminales
            this.species = allNodes.ToArray(); // En lugar de terminalNodes.ToArray()
            

            // Guardar los nodos terminales en this.species
            //this.species = terminalNodes.ToArray();

            return root;
        }


        public double[,] GenerateMatrix(string path_file)
        {
            Species fix;
            Species pointer;
            int current_time = 0;
            int speciesCount = this.species.Length; // Para evitar accesos fuera de rango
            for(int i = 0; i < speciesCount; i++)
            {
                if(this.species[i].creation_time > current_time)
                {
                    current_time = this.species[i].creation_time;
                }
            }
            Console.WriteLine(current_time);
            bool isTheSame;
            //Console.WriteLine($"Case 3 {speciesCount}");
            for (int i = 0; i < speciesCount; i++)
            {
                
                for (int j = 0; j < speciesCount; j++)
                {
                    
                    if (i != j)
                    {

                        fix = this.species[i];
                        pointer = this.species[j];

                        if (fix == null || pointer == null)
                        {
                            Console.WriteLine($"Error: species[{i}] o species[{j}] es null.");
                            continue; // Evitar accesos inválidos
                        }

                        isTheSame = false;
                        while (!isTheSame)
                        {
                            if (fix == pointer && pointer != null)
                            {
                                isTheSame = true;
                            }
                            if (!isTheSame)
                            {
                                if (pointer.father == null)
                                {
                                    if (fix.father != null)
                                    {
                                        fix = fix.father;
                                        pointer = this.species[j];
                                    }
                                    else
                                    {
                                        Console.WriteLine("Fatal error");
                                        break;
                                    }
                                }
                                else
                                {
                                    pointer = pointer.father;
                                }
                            }
                        }
                        this.M_d[i, j] = ((((species[i].creation_time - pointer.creation_time)+(species[j].creation_time - pointer.creation_time))/2) - gap);
                        this.interaction_ids[i, j] = species[j].id;
                        //Console.WriteLine($"{current_time} - {pointer.creation_time} - {gap} = {this.M_d[i, j]} for {i},{j}");
                    }
                    else
                    {
                        this.M_d[i, j] = 0;
                    }
                }
            }
            /*
            for(int i = 0; i < speciesCount ; i++ )
            {
                for(int j = 0; j < speciesCount ; j++ )
                {
                    Console.Write($"{M_d[i,j]:F4}\t");
                }
                Console.Write("\n");

            }*/
            SaveMatrixTXTNNFile(path_file);
            this.M_d = NormalizarMatriz(this.M_d,-1,1);
            
            return this.M_d;
        }
        static double[,] NormalizarMatriz(double[,] matriz, double nuevoMin, double nuevoMax)
    {
        int filas = matriz.GetLength(0);
        int columnas = matriz.GetLength(1);

        double minValor = double.MaxValue;
        double maxValor = double.MinValue;

        // Encontrar el mínimo y el máximo en la matriz
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                if (matriz[i, j] < minValor) minValor = matriz[i, j];
                if (matriz[i, j] > maxValor) maxValor = matriz[i, j];
            }
        }

        // Aplicar la normalización en el rango [-1,1]
        double[,] resultado = new double[filas, columnas];
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                if (i != j)
                {
                    resultado[i, j] = nuevoMin + (nuevoMax - nuevoMin) * (matriz[i, j] - minValor) / (maxValor - minValor);
                }
                else
                {
                    resultado[i, j] = 0;
                }
            }
        }

        return resultado;
        }

        void SaveMatrixTXTNNFile(string path_file)
        {
            using (StreamWriter writer = new StreamWriter($"./{path_file}/interaction_matrix_NN.txt"))
            {
                for (int i = 0; i < this.limit_species; i++)
                {
                    string line = string.Join(" ", Enumerable.Range(0, this.limit_species).Select(j => this.M_d[i, j].ToString("F4")));
                    writer.WriteLine(line);
                }
            }
        }

        void SaveMatrixTXTFile(string path_file)
        {
            using (StreamWriter writer = new StreamWriter($"./{path_file}/interaction_matrix.txt"))
            {
                for (int i = 0; i < this.limit_species; i++)
                {
                    string line = string.Join(" ", Enumerable.Range(0, this.limit_species).Select(j => this.M_d[i, j].ToString("F4")));
                    writer.WriteLine(line);
                }
            }
        }

        void SaveMatrixIDFile(string path_file)
        {
            string filePath = Path.Combine(path_file, "interaction_matrix.csv");
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                int n = species.Length;
                
                // Escribir encabezado
                writer.Write(","); // Celda vacía en la esquina superior izquierda
                writer.WriteLine(string.Join(",", species.Select(s => s.id)));
                
                for (int i = 0; i < n; i++)
                {
                    writer.Write(species[i].id + ","); // Escribir ID en la primera columna
                    
                    for (int j = 0; j < n; j++)
                    {
                        int distance = species[i].creation_time  + species[j].creation_time;
                        writer.Write(distance);
                        
                        if (j < n - 1)
                            writer.Write(","); // Separar valores con coma
                    }
                    
                    writer.WriteLine(); // Nueva línea al final de cada fila
                }
            }
        }

        // Función para guardar el árbol filogenético en formato Newick
        void SaveNewickFile(string path)
        {
            string newick = GenerateNewick(this.root);
            File.WriteAllText(path, newick);
        }

        // Generar el árbol en formato Newick a partir de las especies
        public string GenerateNewick(Species root)
        {
            if (root == null) return "";
            if (root.first_son == null && root.second_son == null)
            {
                return $"{root.id}:{root.creation_time_pure}";
            }
            string left = GenerateNewick(root.first_son);  // Supongamos que la relación es binaria
            string right = GenerateNewick(root.second_son); // Si tienes una estructura más compleja, puedes ajustar esto
            return $"({left},{right}){root.id}:{root.creation_time_pure}";
        }

    }
}
