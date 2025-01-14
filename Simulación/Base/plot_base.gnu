# Configuración general
set terminal pngcairo size 1600,900 enhanced font "Arial,20"
set output 'output_plot_all.png'
set grid
#set title "C" font "Arial,14"
set xlabel "Iteraciones"
set ylabel "x_{i}"

# Configuración del formato CSV
set datafile separator ","
#set decimal locale
#set filename = "datos.csv"
# Estilo de líneas y colores
set style data lines
set key outside right box

set xrange [0:700]

# Plotear todas las columnas en un solo comando plot
plot for [i=1:10] 'base_all.csv' using ($0):i title sprintf("x_{%d}", i)
#plot "base.csv" using 1 with linespoints title "Media de y[i]"

set output