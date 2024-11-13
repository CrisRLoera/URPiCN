# Configuración general
set terminal pngcairo size 1600,900 enhanced font "Arial,20"
set output 'output_plot_node_deletion.png'
set grid
#set title "C" font "Arial,14"
set xlabel "Iteraciones" font "Arial,20"
set ylabel "x_{i}" font "Arial,20"

# Configuración del formato CSV
set datafile separator ","
#set decimal locale
#set filename = "datos.csv"
# Estilo de líneas y colores
#set style data lines
set key outside right box

#set xrange [0:700]

# Plotear todas las columnas en un solo comando plot
# plot for [i=1:26] 'project2.csv' using ($0):i 
plot "node_f_0.csv" title "f_n=0", "node_f_0.1.csv" title "f_n=0.1"

#set output