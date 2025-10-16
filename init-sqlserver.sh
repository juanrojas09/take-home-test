#!/bin/bash

/opt/mssql/bin/sqlservr &


/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -C -d master -i /init-db.sql

if [ $? -eq 0 ]; then
  echo 'Database init successfully'
else
  echo 'Error initializating db'
fi

wait