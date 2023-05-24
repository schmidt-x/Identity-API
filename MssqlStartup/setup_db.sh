#!/usr/bin/env bash

# wait for the db to startup
# this approach is just enough for now
sleep 20

# these args are passed from Dockerfile
DB_NAME=$1
DB_USER=$2
DB_PASSWORD=$3
DB_SA_PASSWORD=$4

./opt/mssql-tools/bin/sqlcmd \
	-S localhost -U SA -P "$DB_SA_PASSWORD" -d master \
	-v "DB_NAME=$DB_NAME" \
	-v "DB_USER=$DB_USER" \
	-v "DB_PASSWORD=$DB_PASSWORD" \
	-i setup.sql \
	&& echo "Database initialized"