#!/bin/bash

# Verifica si se pasó un archivo como argumento
if [ -z "$1" ]; then
    echo "Por favor, proporciona el archivo .cs a compilar."
    exit 1
fi

# Variables
cs_file="$1"
exe_file="main.exe"
csv_file="project.csv"
gnuplot_script="plot_base.gnu"

# Compila el archivo .cs con mcs
echo "Compilando $cs_file..."
mcs "$cs_file" -out:"$exe_file"
if [ $? -ne 0 ]; then
    echo "Error en la compilación."
    exit 1
fi

# Ejecuta el .exe con mono
echo "Ejecutando $exe_file..."
mono "$exe_file"
if [ $? -ne 0 ]; then
    echo "Error al ejecutar el programa."
    exit 1
fi

# Verifica si se generó el archivo .csv
if [ ! -f "$csv_file" ]; then
    echo "El archivo $csv_file no se generó."
    exit 1
fi

# Ejecuta gnuplot si existe el script .gnuplot
if [ -f "$gnuplot_script" ]; then
    echo "Ejecutando gnuplot con el archivo $csv_file..."
    gnuplot "$gnuplot_script"
else
    echo "No se encontró el archivo $gnuplot_script para gnuplot."
    exit 1
fi

echo "Proceso completado exitosamente."
