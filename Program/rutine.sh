#!/bin/bash

# Definir archivo de log
LOG_FILE="ejecucion.log"

# Función para registrar eventos en el log
log_event() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" >> "$LOG_FILE"
}

# Iniciar el log
log_event "Iniciando la ejecución de los programas."

# Lista ordenada de ejecución (primero 100, 1000, luego 10000, 100000)
programs=(
    "main_1000000_H.exe" "main_1000000_HL.exe" "main_1000000_M.exe" "main_1000000_L.exe"
)

# Ejecutar cada programa en orden
for program in "${programs[@]}"; do
    if [[ -f "$program" ]]; then
        log_event "Ejecutando $program..."
        nohup ./"$program" &  # Ejecutar en segundo plano
        wait $!  # Esperar a que termine
        if [ $? -eq 0 ]; then
            log_event "$program se ejecutó correctamente."
        else
            log_event "Error: $program falló."
            exit 1
        fi
    else
        log_event "Advertencia: $program no encontrado, se omite."
    fi
done

# Finalizar el log
log_event "Todos los programas se ejecutaron correctamente."
