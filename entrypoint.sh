#!/bin/bash



sleep 30s


echo "Ejecutando script de inicialización..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i /docker-entrypoint-initdb.d/init-db.sql

echo "Base de datos inicializada correctamente"
