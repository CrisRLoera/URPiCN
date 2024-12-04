using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class Dendrogram
{
    static void Main(string[] args)
    {
        int t = 0;
        int existing_species = 0;
        int limit_species = 20;

        Species[] species = new Species[20];

        // Crear una instancia de la clase Random
        Random random = new Random();

        // Generar un número aleatorio entre 0 y 1
        double p = random.NextDouble();
        
        species[existing_species] = new Species(t);
        do
        {
            // Iterar sobre las especies existentes
            existing_species = CountSpecies(species);
            t++;
            // Crear una lista clon de species
            List<Species> speciesClon = new List<Species>(species.Where(s => s != null)); // Clonar solo las especies no nulas
            for (int i = 0; i < speciesClon.Count; i++)
            {
                var especie = speciesClon[i];
                p = random.NextDouble();
                Console.WriteLine($"Índice de la especie en speciesClon: {i}, Probabilidad de la especie: {p:F7}"); // Mostrar el índice y la probabilidad
                if (p < 0.2 && existing_species < limit_species)
                {
                    species[existing_species] = new Species(t);
                    existing_species++; // Asegúrate de incrementar existing_species
                }
            }
        } while (existing_species < limit_species);
        
        // Crear un diccionario para agrupar especies por tiempo
        Dictionary<int, List<Species>> especiesPorTiempo = new Dictionary<int, List<Species>>();

        // Iterar sobre cada especie en el arreglo
        foreach (var especie in species)
        {
            if (especie != null) // Verificar que la especie no sea nula
            {
                int tiempo = especie.GetTiempo(); // Obtener el tiempo de creación
                if (!especiesPorTiempo.ContainsKey(tiempo))
                {
                    especiesPorTiempo[tiempo] = new List<Species>(); // Inicializar la lista si no existe
                }
                especiesPorTiempo[tiempo].Add(especie); // Agregar la especie a la lista correspondiente
            }
        }

        // Imprimir las especies agrupadas por tiempo
        foreach (var kvp in especiesPorTiempo)
        {
            Console.WriteLine($"Tiempo {kvp.Key}: {kvp.Value.Count} especies");
            foreach (var especie in kvp.Value)
            {
                Console.WriteLine($" - Especie creada en el tiempo: {especie.GetTiempo()}");
            }
        }

        // Llamar a la función para generar números de Poisson y guardar en CSV
        GenerarDatosPoisson("test.csv", 500, 5); // 5 es el valor de lambda
    }

    // Método para contar instancias de Species
    static int CountSpecies(Species[] species)
    {
        int count = 0;
        foreach (var s in species)
        {
            if (s != null) count++;
        }
        return count;
    }

    // Nueva función para generar números de Poisson y guardar en un archivo CSV
    static void GenerarDatosPoisson(string filePath, int cantidad, double lambda)
    {
        // Crear un diccionario para contar las frecuencias
        Dictionary<int, int> frecuencias = new Dictionary<int, int>();

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Número,Frecuencia"); // Escribir encabezado en el CSV
            Random random = new Random();
            for (int i = 0; i < cantidad; i++)
            {
                int numero = GenerarPoisson(lambda, random);
                
                // Contar la frecuencia del número
                if (frecuencias.ContainsKey(numero))
                {
                    frecuencias[numero]++;
                }
                else
                {
                    frecuencias[numero] = 1;
                }
            }

            // Escribir los números y sus frecuencias en el archivo
            foreach (var kvp in frecuencias)
            {
                writer.WriteLine($"{kvp.Key},{kvp.Value}"); // Escribir el número y su frecuencia
            }
        }
    }

    // Función para generar un número aleatorio de Poisson
    static int GenerarPoisson(double lambda, Random random)
    {
        double L = Math.Exp(-lambda);
        int k = 0;
        double p = 1.0;

        do
        {
            k++;
            p *= random.NextDouble();
        } while (p > L);

        return k - 1; // Decrementar k para obtener el número de Poisson
    }

}

class Species
{
    private int tiempo;

    public Species(int tiempo)
    {
        this.tiempo = tiempo;
        Console.WriteLine("Una nueva especie nacio en el tiempo:" + tiempo);
    }

    // Nueva función que devuelve una probabilidad
    public double CalcularProbabilidad(int intervalo)
    {
        double lambda = tiempo; // Usamos el tiempo como lambda
        int k = 2; // Queremos la probabilidad de que X sea 2
        double probabilidad = (Math.Exp(-lambda) * Math.Pow(lambda, k)) / Factorial(k);
        return probabilidad;
    }

    // Método para calcular el factorial
    private int Factorial(int n)
    {
        if (n == 0) return 1;
        int resultado = 1;
        for (int i = 1; i <= n; i++)
        {
            resultado *= i;
        }
        return resultado;
    }

    public int GetTiempo()
    {
        return tiempo;
    }
}


// Encontrar

//Funcion de densidad de probabilidad con 500 numeros sacar un histograma pasa que se vea que es una distribucion poassoniana
// First Course of probability
// Sheldon Ross, Models of random, random models
