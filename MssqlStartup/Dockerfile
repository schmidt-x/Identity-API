FROM mcr.microsoft.com/mssql/server:2022-latest

# these args are passed from compose.yaml 
# and propagated throughout the entire database initialization process
ARG DB_NAME
ARG DB_USER
ARG DB_PASSWORD
ARG DB_PID
ARG DB_SA_PASSWORD

ENV ACCEPT_EULA=Y
ENV MSSQL_PID $DB_PID
ENV MSSQL_SA_PASSWORD $DB_SA_PASSWORD

COPY ["MssqlStartup/setup_db.sh", "setup_db.sh"]
COPY ["MssqlStartup/setup.sql", "setup.sql"]

RUN ./opt/mssql/bin/sqlservr & ./setup_db.sh "$DB_NAME" "$DB_USER" "$DB_PASSWORD" "$DB_SA_PASSWORD" 