docker run --name bam-mssql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD={MsSqlPassword}" -e "MSSQL_PID=Express" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest