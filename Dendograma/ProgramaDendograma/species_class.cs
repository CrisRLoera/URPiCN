using System;
namespace SpeciesClass {
    public class Species {
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

            if (temp1 < temp2) {
                this.first_son_creation_time = temp1;
                this.second_son_creation_time = temp2;
            } else {
                this.first_son_creation_time = temp2;
                this.second_son_creation_time = temp1;
            }
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
}