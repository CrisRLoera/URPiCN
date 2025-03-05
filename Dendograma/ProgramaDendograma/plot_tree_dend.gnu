set terminal pngcairo size 800,600 enhanced font 'Verdana,10'
set output 'grafico.png'

set title "Frecuencia por Segmento"
set xlabel "Segmento"
set ylabel "Frecuencia"
set logscale x  # Escala logarítmica en el eje X
set logscale y  # Escala logarítmica en el eje Y
set grid
set datafile separator ","

set style data linespoints
plot 'power_law_distribution.csv' using 1:2 with points lw 2 title "Frecuencia"
