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
 plot for [i=1:20] 'node_f_0.csv' using ($0):i 
# plot "node_f_0.csv" title "f_n=0", "node_f_0.1.csv" title "f_n=0.1"
#plot "node_f_0.csv" title "f_n=0", "node_f_0.1.csv" title "f_n=0.1", "node_f_0.2.csv" title "f_n=0.2", "node_f_0.3.csv" title "f_n=0.3", "node_f_0.4.csv" title "f_n=0.4", "node_f_0.5.csv" title "f_n=0.5", "node_f_0.6.csv" title "f_n=0.6", "node_f_0.7.csv" title "f_n=0.7",, "node_f_0.8.csv" title "f_n=0.8", "node_f_0.9.csv" title "f_n=0.9", "node_f_1.csv" title "f_n=1"
# Plotear todas las columnas en un solo comando plot
#plot for [i=0:9] sprintf("node_f_%.1f.csv", i*0.1) title sprintf("f_n=%.1f", i*0.1), "node_f_1.csv" title "f_n=1"
#set output