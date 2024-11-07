set terminal pngcairo
set datafile separator ","
set output 'plot_node_deletion.png'
set xlabel "Fracción de nodos eliminados"
set ylabel "Media de los valores finales de y[i]"
set title "Media de y[i] vs Fracción de nodos eliminados"
set grid
set yrange [0:1]  # Ajusta este rango según tus datos reales

plot "project.csv" using 1:2 with linespoints title "Media de y[i]"
