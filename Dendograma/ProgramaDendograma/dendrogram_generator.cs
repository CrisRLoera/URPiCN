using System;
using System.Linq;
using System.IO;
using SpeciesClass;

namespace DendrogramGeneratorClass
{
    class DendrogramGenerator
    {
        private System.Diagnostics.Stopwatch stopwatch;
        private int current_species = 0;
        private int N = 0;
        private int t = 0;
        public int gap = 10000;
        public int limit_species = 10;
        public Species[] species;
        public double[,] M_d;

        public Species root;

        public DendrogramGenerator()
        {
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
            this.species = new Species[limit_species];
            this.M_d = new double[limit_species, limit_species];
            GenerateSpecies();
            GenerateMatrix();
            SaveMatrixTXTFile();
            SaveNewickFile("tree.nwk");
            Console.WriteLine($"Tiempo de ejecución: {this.stopwatch.Elapsed.TotalSeconds} segundos");
        }

        public DendrogramGenerator(int custom_gap, int custom_limit_species, string path)
        {
            this.gap = custom_gap;
            this.limit_species = custom_limit_species;
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
            this.species = new Species[limit_species];
            this.M_d = new double[limit_species, limit_species];
            GenerateSpecies();
            GenerateMatrix();
            SaveMatrixTXTFile(path);
            Directory.CreateDirectory(path);
            SaveNewickFile($"{path}/tree.nwk");
            Console.WriteLine($"Tiempo de ejecución: {this.stopwatch.Elapsed.TotalSeconds} segundos");
        }

        public double[,] GetMatrix()
        {
            return this.M_d;
        }

        void GenerateSpecies()
        {
            this.species[0] = new Species(t, null);
            this.species[0].id = N;
            this.root = species[0];
            this.current_species++;
            Species[] speciesCopy;
            while (this.current_species < this.limit_species)
            {
                speciesCopy = this.species.Clone() as Species[];
                for (int i = 0; i < this.limit_species; i++)
                {
                    if (speciesCopy[i] != null)
                    {
                        if (speciesCopy[i].created_sons == 0)
                        {
                            if (this.t == speciesCopy[i].first_son_creation_time && this.current_species < this.limit_species)
                            {
                                this.species[i] = speciesCopy[i].CreateFirst(this.t);
                                speciesCopy[i].created_sons = speciesCopy[i].created_sons + 1;
                                this.N++;
                                this.species[i].id = N;
                            }
                        }
                        if (speciesCopy[i].father != null)
                        {
                            if (speciesCopy[i].father.created_sons == 1)
                            {
                                if (this.t == speciesCopy[i].father.second_son_creation_time && this.current_species < this.limit_species)
                                {
                                    this.species[current_species] = speciesCopy[i].father.CreateSecond(this.t);
                                    this.N++;
                                    this.species[current_species].id = N;
                                    this.current_species++;
                                    speciesCopy[i].father.created_sons = speciesCopy[i].father.created_sons + 1;
                                }
                            }
                        }
                    }
                }
                this.t++;
            }
        }

        double[,] GenerateMatrix()
        {
            Species fix;
            Species pointer;
            bool isTheSame;
            for (int i = 0; i < this.limit_species; i++)
            {
                for (int j = 0; j < this.limit_species; j++)
                {
                    if (i != j)
                    {
                        fix = this.species[i];
                        pointer = this.species[j];
                        isTheSame = false;
                        while (isTheSame != true)
                        {
                            if (fix == pointer && pointer != null)
                            {
                                isTheSame = true;
                            }
                            if (isTheSame != true)
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
                                    }
                                }
                                else
                                {
                                    pointer = pointer.father;
                                }
                            }
                        }

                        this.M_d[i, j] = (species[i].creation_time - pointer.creation_time) + (species[j].creation_time - pointer.creation_time) - gap;

                    }
                    else
                    {
                        this.M_d[i, j] = 0;
                    }
                }
            }

            double theBiggest = 0;
            for (int i = 0; i < this.limit_species; i++)
            {
                for (int j = 0; j < this.limit_species; j++)
                {
                    if (theBiggest < Math.Abs(this.M_d[i, j]))
                    {
                        theBiggest = this.M_d[i, j];
                    }
                }
            }

            for (int i = 0; i < this.limit_species; i++)
            {
                for (int j = 0; j < this.limit_species; j++)
                {
                    this.M_d[i, j] = this.M_d[i, j] / theBiggest;
                }
            }
            return this.M_d;
        }

        void SaveMatrixTXTFile()
        {
            using (StreamWriter writer = new StreamWriter($"interaction_matrix.txt"))
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
                return $"{root.id}:{root.creation_time}";
            }
            string left = GenerateNewick(root.first_son);  // Supongamos que la relación es binaria
            string right = GenerateNewick(root.second_son); // Si tienes una estructura más compleja, puedes ajustar esto
            return $"({left},{right}){root.id}:{root.creation_time}";
        }

    }
}
