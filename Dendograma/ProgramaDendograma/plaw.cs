using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static Random random = new Random();
    static int count_s = 0;

    static int GenerarPLaw()
    {
        double alpha = 2.5;
        double min = 1.0;
        double max = 100000.0;
        int valorGenerado;

        do
        {
            double u = 1.0 - random.NextDouble(); // Evita log(0)
            valorGenerado = (int)Math.Floor(min * Math.Pow(u, 1.0 / (1.0 - alpha)));
        } while (valorGenerado < min || valorGenerado > max); // Reintenta si está fuera del rango
        Console.WriteLine($"{count_s}:{valorGenerado }");
        return valorGenerado;
    }

    static void Main()
    {
        int numSamples = 1000000;
        int numSegments = 500;
        double min = 1.0;
        double max = 100000.0;
        
        // Generar números y contar frecuencias en los segmentos
        List<int> numbers = new List<int>();
        for (int i = 0; i < numSamples; i++)
        {
            numbers.Add(GenerarPLaw());
        }
        
        // Definir los límites de los segmentos
        Dictionary<string, int> frequency = new Dictionary<string, int>();
        for (int k = 1; k <= numSegments; k++)
        {
            double lowerBound = ((min + max) / numSegments) * (k - 1);
            double upperBound = ((min + max) / numSegments) * k;
            frequency[$"{(int)(((min + max) / numSegments) * k)}"] = numbers.Count(num => num >= lowerBound && num < upperBound);
        }

        // Escribir resultados en CSV
        using (StreamWriter writer = new StreamWriter("power_law_distribution.csv"))
        {
            writer.WriteLine("Segmento,Frecuencia");
            foreach (var entry in frequency)
            {
                writer.WriteLine($"{entry.Key},{entry.Value}");
            }
        }
        
        Console.WriteLine("Archivo CSV generado exitosamente.");
    }
}
