# Configuración general
set terminal pngcairo size 1600,900 enhanced font "Arial,20"
set output 'output_plot_node_deletion_i1.png'
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
#plot for [i=0:9] sprintf("node_i_%.1f.csv", i*0.1) title sprintf("f_n=%.1f", i*0.1), "node_f_1.csv" title "f_n=1"
#plot "node_i_0.csv" using 4
plot for [i=1:10] "node_i_1.csv" using ($0):i
#plot for [i=1:20] 'node_i_0.csv' using ($0):i
#set output