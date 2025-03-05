using System;
namespace SpeciesClass {
    public class Species {
        public int id;
        public int creation_time;
        public int creation_time_pure;
        public Species father;
        public Species first_son = null;
        public int first_son_creation_time;
        public Species second_son = null;
        public int second_son_creation_time;
        public int lambda_sons = 5;
        public int created_sons = 0;
        public int temp1;
        public int temp2;

        public Species(int temp, int tiempo, Species father) {
            this.creation_time_pure = temp;
            this.creation_time = tiempo + temp;
            this.father = father;
            int temp1 = GenerarPLaw();
            int temp2 =  GenerarPLaw();


            this.first_son_creation_time = tiempo + temp1;
            this.second_son_creation_time = tiempo + temp2;
        }
        static int GenerarPLaw()
        {
            double alpha = 2.5;
            double min = 1.0;
            double max = 100000.0;
            int valorGenerado;
            Random random =  new Random((int)DateTime.Now.Ticks & 0x0000FFFF);

            do
            {
                double u = 1.0 - random.NextDouble(); // Evita log(0)
                valorGenerado = (int)Math.Floor(min * Math.Pow(u, 1.0 / (1.0 - alpha)));
            } while (valorGenerado < min || valorGenerado > max); // Reintenta si está fuera del rango
            
            return valorGenerado;
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
            this.first_son = new Species(this.first_son_creation_time,t,this);
            return this.first_son;
        }

        public Species CreateSecond(int t){
            this.second_son = new Species(this.second_son_creation_time,t,this);
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