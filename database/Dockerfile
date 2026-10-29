# Imagem auxiliar que roda os scripts do banco no SQL Server do docker-compose.
FROM mcr.microsoft.com/mssql-tools
COPY *.sql /scripts/
COPY criar-banco.sh /scripts/
CMD ["bash", "/scripts/criar-banco.sh"]
