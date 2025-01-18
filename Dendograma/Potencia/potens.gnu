set terminal pngcairo
#set output 'histograma_log.png'
set output 'histograma.png'

set datafile separator ","

set style data histogram
set style fill solid 1.0
set boxwidth 0.9

set xlabel "Valores"
set ylabel "Frecuencia (escala logarítmica)"
#set title "Histograma de Potencia (Escala Logarítmica)"
set title "Histograma de Potencia"

# Configurar el eje Y en escala logarítmica
#set logscale y
#set logscale x

# Usar la primera columna para los valores y la segunda para las frecuencias
plot "potens.csv" using 1:2 title "Frecuencia" with boxes