set datafile separator ","
set terminal pngcairo
set style data histogram
set style histogram rowstacked
set boxwidth 0.9
set grid y
set title "Histograma de test.csv"
set xlabel "Número"
set ylabel "Frecuencia"
set output "histograma.png"
plot "test.csv" using 1:2 title "Frecuencia" with boxes
