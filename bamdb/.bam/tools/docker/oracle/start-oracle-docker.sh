docker run --name bam-oracle -e "ORACLE_PASSWORD={OraclePassword}" -p 5432:5432 -d container-registry.oracle.com/database/express:21.3.0-xe