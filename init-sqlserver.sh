#!/bin/bash

# Iniciar SQL Server en segundo plano
/opt/mssql/bin/sqlservr &




echo 'Ejecutando script de inicializacion de base de datos...'
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -d master -i /init-db.sql

if [ $? -eq 0 ]; then
  echo 'Base de datos inicializada correctamente'
else
  echo 'Error al inicializar la base de datos'
fi

wait