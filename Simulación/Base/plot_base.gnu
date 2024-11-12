# Configuración general
set terminal pngcairo size 1600,900 enhanced font "Arial,12"
set output 'output_plot.png'
set grid
#set title "C" font "Arial,14"
set xlabel "Iteraciones" font "Arial,12"
set ylabel "x_{i}" font "Arial,12"

# Configuración del formato CSV
set datafile separator ","
set decimal locale
#set filename = "datos.csv"
# Estilo de líneas y colores
set style data lines
set key outside right box

#set xrange [0:700]

# Plotear todas las columnas en un solo comando plot
# plot for [i=1:26] 'project2.csv' using ($0):i 
plot filename using 1 with linespoints title "Media de y[i]"

set output