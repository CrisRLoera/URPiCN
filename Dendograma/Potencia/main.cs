using System;
using System.IO;
using System.Collections.Generic;

class Program {
    static void Main() {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        int result = 0;
        int times = 100000;
        Random random = new Random();
        double myX = 0.0;
        Dictionary<double, int> contadorResultados = new Dictionary<double, int>();

        using (StreamWriter writerPotensLaw = new StreamWriter($"potens.csv")) {
            for (int i = 0; i < times; i++) {
                myX = random.NextDouble();
                result = GenerarPLaw(myX);
                
                // Contar las apariciones de 'result'
                if (contadorResultados.ContainsKey(result)) {
                    contadorResultados[result]++;
                } else {
                    contadorResultados[result] = 1;
                }

                // Guardar myX, result y el contador de result en el archivo
                writerPotensLaw.WriteLine($"{result},{contadorResultados[result]}");
                //Console.WriteLine($"Time:{i} complete");
            }
        }

        // Imprimir la frecuencia de cada resultado
        /*
        foreach (var par in contadorResultados) {
            Console.WriteLine($"Valor: {par.Key}, Frecuencia: {par.Value}");
        }*/

        stopwatch.Stop();
        Console.WriteLine($"Tiempo de ejecución: {stopwatch.Elapsed.TotalSeconds} segundos");
    }
    static int GenerarPLaw(double u)
    {
        double alpha = 2.5;
        double min = 0.0;
        double max = 10000.0;
        double Py = min + (max - min) * (1 - Math.Pow(u, 1 / alpha));
        return Convert.ToInt32(Py);
    }
}