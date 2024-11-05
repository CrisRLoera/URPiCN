# Configuración general
set terminal pngcairo size 1600,900 enhanced font "Arial,12"
set output 'output_plot.png'
set grid
set title "Visualización de las 26 columnas" font "Arial,14"
set xlabel "Índice" font "Arial,12"
set ylabel "Valor" font "Arial,12"

# Configuración del formato CSV
set datafile separator ","
set decimal locale

# Estilo de líneas y colores
set style data lines
set key outside right box


set xrange [0:200]

# Plotear todas las columnas en un solo comando plot
plot for [i=1:26] 'project.csv' using ($0):i title sprintf('Col %d', i) lt i
# plot 'project.csv' using 1:2 with linespoints title "Media de y[i]"

