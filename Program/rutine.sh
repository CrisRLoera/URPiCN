#!/bin/bash

# Definir archivo de log
LOG_FILE="ejecucion.log"

# Función para registrar los eventos
log_event() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" >> $LOG_FILE
}

# Iniciar el log
log_event "Iniciando la ejecución de los programas."

# Ejecutar main_H.exe en segundo plano con nohup
log_event "Ejecutando main_H.exe..."
nohup mono main_H.exe &
# Esperar a que termine main_H.exe antes de continuar
wait $!
if [ $? -eq 0 ]; then
    log_event "main_H.exe se ejecutó correctamente."
else
    log_event "Error: main_H.exe falló."
    exit 1
fi

# Finalizar el log
log_event "Todos los programas se ejecutaron correctamente."
