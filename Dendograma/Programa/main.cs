using System;

class Dendrogram
{
    static void Main(string[] args)
    {
        int t = 0;
        int existing_species = 0;

        Species[] species = new Species[20];

        // Crear una instancia de la clase Random
        Random random = new Random();

        // Generar un número aleatorio entre 0 y 1
        double p = random.NextDouble();
        
        species[existing_species] = new Species(t);
        do
        {

            if (p < 0.5)
            {
                species[existing_species] = new Species(t);
                existing_species = CountSpecies(species);
            }
            p = random.NextDouble();
            t++;
        } while (existing_species < 20);
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

}

class Species
{
    private int tiempo;

    public Species(int tiempo)
    {
        this.tiempo = tiempo;
        Console.WriteLine("Una nueva especie nacio en el tiempo:" + tiempo);
    }
}