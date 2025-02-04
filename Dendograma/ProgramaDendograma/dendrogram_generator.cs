using System;
using SpeciesClass;
using System.Linq;
using System.IO;
namespace DendrogramGeneratorClass {
    class DendrogramGenerator 
    {
        private System.Diagnostics.Stopwatch stopwatch;
        /*********************************************
            Algoritmo de generación de dendograma
        **********************************************/
        // Variables de control para el algoritmo de generación del dendograma
        private int current_species = 0;    // Especies actuales en las hojas del árbol
        private int N = 0;                  // N es un contador que permite generar un identificador para cada nodo
        private int t = 0;                  // Variable de tiempo del programa
        /*
            gap es el umbral que define las unidades de tiempo necesarias para que
            una especie tenga una interacción que se considera negativa al llevar
            un tiempo de separación mas corto se asume que compiten por los mismos
            recursos reduciendo las poblaciones de las especies cercanas.
        */
        public int gap = 10000;
        public int limit_species = 10;     // Especies que se desean generar
        public Species[] species;
        public double[,] M_d;
        public DendrogramGenerator()
        {
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
            this.species = new Species[limit_species];
            this.M_d = new double[limit_species,limit_species];
            GenerateSpecies();
            GenerateMatrix();
            SaveMatrixTXTFile();
            Console.WriteLine($"Tiempo de ejecución: {this.stopwatch.Elapsed.TotalSeconds} segundos");
        }
        public DendrogramGenerator(int custom_gap, int custom_limit_species,string path)
        {
            this.gap = custom_gap;
            this.limit_species = custom_limit_species;
            this.stopwatch = System.Diagnostics.Stopwatch.StartNew();
            this.species = new Species[limit_species];
            this.M_d = new double[limit_species,limit_species];
            GenerateSpecies();
            GenerateMatrix();
            SaveMatrixTXTFile(path);
            Console.WriteLine($"Tiempo de ejecución: {this.stopwatch.Elapsed.TotalSeconds} segundos");
        }

        public double[,] GetMatrix()
        {
            return this.M_d;
        }
        void GenerateSpecies()
        {
            /**********************************************
            
                Se ejecuta una simulación en la que las
                especies generan dos tiempos de creación
                para dos nodos hijo, los cuales se 
                crearan cuando su tiempo de aparición
                sea igual a t.
                El objetivo es que se genere un número
                n de especies dado por la variable 
                limit_species

            ***********************************************/
            
            this.species[0] = new Species(t,null);
            this.species[0].id = N;
            //Species root = species[0];
            this.current_species++;
            Species[] speciesCopy;
            while(this.current_species < this.limit_species) {
                speciesCopy = this.species.Clone() as Species[];
                for (int i = 0; i < this.limit_species ; i++) {
                    if (speciesCopy[i] != null) {
                        if (speciesCopy[i].created_sons == 0) {
                            if (this.t == speciesCopy[i].first_son_creation_time && this.current_species < this.limit_species) {    
                                this.species[i] = speciesCopy[i].CreateFirst(this.t);
                                speciesCopy[i].created_sons = speciesCopy[i].created_sons + 1;
                                this.N++;
                                this.species[i].id = N;
                            }
                        }
                        if (speciesCopy[i].father != null) {    
                            if (speciesCopy[i].father.created_sons == 1) {
                                if (this.t == speciesCopy[i].father.second_son_creation_time && this.current_species < this.limit_species) {
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
            /**********************************************
                Generamos una matriz de interacción
                de tamaño n x n, donde las interacciones
                entre las misma especie es 0
                y para especies diferentes se calcula
                la distancia filogenetica de una con
                respecto a otra, menos el umbral
            **********************************************/
            Species fix;
            Species pointer;
            bool isTheSame;
            for (int i = 0; i < this.limit_species ; i++) {
                //Console.WriteLine($"> Especie {i}");
                for (int j = 0; j < this.limit_species; j++) {
                    if(i != j) {
                        fix = this.species[i];
                        pointer = this.species[j];
                        isTheSame = false;
                        while(isTheSame != true) {
                            //Console.WriteLine($"fix:{fix.id} pointer:{pointer.id}");
                            if (fix == pointer && pointer !=null) {
                                isTheSame = true;
                            }
                            if (isTheSame!= true) {
                                if(pointer.father == null) {
                                    if(fix.father != null) {
                                        fix = fix.father;
                                        pointer = this.species[j];
                                    }
                                    else {
                                        Console.WriteLine("Fatal error");
                                    }
                                } else {
                                    pointer = pointer.father;
                                }
                            }
                        }
                        
                        this.M_d[i,j] = (species[i].creation_time - pointer.creation_time) + (species[j].creation_time - pointer.creation_time) - gap;

                    } else {
                        this.M_d[i,j] = 0;
                    }
                }
            }
            
            // Normalice
                // Buscar el más grande
            double theBiggest = 0;
            for (int i = 0; i < this.limit_species; i++) {
                for (int j = 0; j < this.limit_species; j++) {
                    if (theBiggest < Math.Abs(this.M_d[i,j])) {
                        theBiggest = this.M_d[i,j];
                    }
                    
                }
            }
            
            for (int i = 0; i < this.limit_species; i++) {
                for (int j = 0; j < this.limit_species; j++) {
                
                    this.M_d[i,j] = this.M_d[i,j]/theBiggest;
                    
                }
            }
            return this.M_d;
        }
        void SaveMatrixTXTFile()
        {
            using (StreamWriter writer = new StreamWriter($"interaction_matrix.txt"))
            {
                for (int i = 0; i < this.limit_species; i++) {
                    // Guardar cada fila de la matriz con espacios como separadores
                    string line = string.Join(" ", Enumerable.Range(0, this.limit_species).Select(j => this.M_d[i, j].ToString("F4")));
                    writer.WriteLine(line);
                }
            }
        }
        void SaveMatrixTXTFile(string path_file)
        {
            using (StreamWriter writer = new StreamWriter($"./Database/{path_file}/interaction_matrix.txt"))
            {
                for (int i = 0; i < this.limit_species; i++) {
                    // Guardar cada fila de la matriz con espacios como separadores
                    string line = string.Join(" ", Enumerable.Range(0, this.limit_species).Select(j => this.M_d[i, j].ToString("F4")));
                    writer.WriteLine(line);
                }
            }
        }
        /*
        static void Main(string[] args) 
        {
            // Crear una instancia de DendrogramGenerator
            DendrogramGenerator generator = new DendrogramGenerator(30000,20);
            // O puedes usar el constructor con parámetros si es necesario
            // DendrogramGenerator generator = new DendrogramGenerator(5000, 5);
        }*/
    }
}