using System;
using System.Threading;

class Dendograma2 {
    public static void Main(string[] args) {
        int n = 0;
        int N = 0;
        int t = 0;
        int gap = 0;
        int limit_species = 5;
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
            if (species[i].father != null) {
                Console.WriteLine($"La especie {i}:{species[i].id} fue creada en {species[i].creation_time}");
                Console.WriteLine($"El padre es {species[i].father.creation_time}:{species[i].father.id}");
            } else {
                Console.WriteLine($"La especie {i}:{species[i].id} fue creada en {species[i].creation_time}");
                Console.WriteLine($"El padre es null");
            }
        }
        int[,] M = new int[limit_species,limit_species];
        for (int i = 0; i < limit_species ; i++) {
            Console.WriteLine($"> Especie {i}");
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
                    Console.WriteLine($"{i}:{species[i].creation_time} + {j}:{species[j].creation_time} - {gap}");
                    Console.WriteLine($"father:{fix.creation_time}");
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
        int temp1 = tiempo + GenerarPoisson(this.lambda_sons) + 1;
        int temp2 = tiempo + GenerarPoisson(this.lambda_sons) + 1;
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

