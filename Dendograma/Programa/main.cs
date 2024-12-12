using System;
using System.Threading;

class Dendograma2 {
    public static void Main(string[] args) {
        int n = 0;
        int N = 0;
        int t = 0;
        int gap = 20;
        int limit_species = 20;
        Species[] species = new Species[limit_species];
        species[0] = new Species(t,null);
        species[0].id = N;
        Species root = species[0];
        n++;
        while(n < limit_species) {
            Species[] speciesCopy = species.Clone() as Species[];
            for (int i = 0; i < limit_species ; i++) {
                if (speciesCopy[i] != null) {
                    //Console.WriteLine($"ESpecie{i}");
                    //Console.WriteLine($"Case1");
                    if (speciesCopy[i].created_sons == 0) {
                        if (t == speciesCopy[i].first_son_creation_time && n < limit_species) {    
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
                            if (t == speciesCopy[i].father.second_son_creation_time && n < limit_species) {
                                //Console.WriteLine($"Creando segundo hijo en {i}:{n}");
                                species[n] = speciesCopy[i].father.CreateSecond(t);                                
                                N++;
                                species[n].id = N;
                                n++;
                                speciesCopy[i].father.created_sons = speciesCopy[i].father.created_sons + 1;
                            }
                        }
                    }
                }
            }
            Console.WriteLine($"Tiempo actual: {t}");
            //Thread.Sleep(1000);
            t++;
            Console.WriteLine($"Número de especies: {n}");

            Console.WriteLine($"root: {root.creation_time}");
            
        }
        Console.WriteLine($"Número total de nodos en el árbol: {root.ContarNodos()}");
        for (int i = 0; i < limit_species; i++) {
            Console.WriteLine($"La especie {i}:{species[i].id} fue creada en {species[i].creation_time}");
        }
        int[,] M = new int[limit_species,limit_species];
        for (int i = 0; i < limit_species ; i++) {
            for (int j = 0; j < limit_species; j++) {
                if(i != j) {
                    Species fix = species[i];
                    Species pointer = species[j];
                    bool isTheSame = false;
                    while(isTheSame != true) {
                        if (fix.father == pointer.father) {
                            isTheSame = true;
                        } 
                        if(pointer.father == null) {
                            fix = fix.father;
                        } else {
                            pointer = pointer.father;
                        }
                    }
                    M[i,j] = (species[i].creation_time - pointer.creation_time) + (species[j].creation_time - pointer.creation_time) - gap;

                } else {
                    M[i,j] = 0;
                }
            }
        }

        // Imprimir la matriz M
        Console.WriteLine("Matriz M:");
        for (int i = 0; i < limit_species; i++) {
            for (int j = 0; j < limit_species; j++) {
                Console.Write(M[i, j] + " "); // Imprimir cada elemento seguido de una tabulación
            }
            Console.WriteLine(); // Nueva línea al final de cada fila
        }
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
        int temp1 = tiempo + GenerarPoisson(this.lambda_sons);
        int temp2 = tiempo + GenerarPoisson(this.lambda_sons);
        if (temp1 < temp2) {
            this.first_son_creation_time = temp1;
            this.second_son_creation_time = temp2;
        } else {
            this.first_son_creation_time = temp2;
            this.second_son_creation_time = temp1;
        }
        Console.WriteLine("Una nueva especie nacio en el tiempo:" + tiempo);
        Console.WriteLine($"Número generado para el primer hijo: {this.first_son_creation_time}");
        Console.WriteLine($"Número generado para el segundo hijo: {this.second_son_creation_time}");
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

