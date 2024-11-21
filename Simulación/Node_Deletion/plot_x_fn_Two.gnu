# Configuración general
set terminal pngcairo size 1600,900 enhanced font "Arial,20"
#set terminal pdf 
set output 'output_plot_x_fn.pdf'
set output 'output_plot_x_fn.png'
set grid
#set title "C" font "Arial,14"
set xlabel "f_{n}" font "Arial,20"
set ylabel "{\\<}x{\\>}" font "Arial,20"
set yrange [0:*]
set ytics 0,2,10
set yrange [0:10]

# Configuración del formato CSV
set datafile separator ","
#set decimal locale

# Estilo de líneas y colores
#set style data lines
#set key outside right box

#set xrange [0:700]

#plot for [i=0:9] sprintf("total_%d.csv", i) title sprintf("total_i=%d", i)
plot for [i=0:20] sprintf("total_%d_L.csv", i) using 2:1 with linespoints notitle, for [j=0:19] sprintf("total_%d_H.csv", j) using 2:1 with linespoints notitle


#plot "total_0.csv" using 2:1 with lines

#set output