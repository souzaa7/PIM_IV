#!/bin/bash
# Espera o SQL Server ficar pronto e executa os scripts na ordem.
for i in $(seq 1 30); do
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1 && break
  echo "Aguardando o SQL Server... ($i)"
  sleep 3
done

# Se o banco já existe, não recria (não apaga os dados).
# Confere uma tabela criada no fim (se uma execução anterior parou no meio, roda de novo).
EXISTE=$(/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -h -1 -Q "SET NOCOUNT ON; SELECT CASE WHEN OBJECT_ID('AprenderMais.dbo.certificado') IS NULL THEN 0 ELSE 1 END")
if [ "$(echo $EXISTE | tr -d ' ')" = "1" ]; then
  echo "Banco AprenderMais já existe."
  exit 0
fi

for arquivo in /scripts/0*.sql; do
  echo "Executando $arquivo"
  /opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "$SA_PASSWORD" -b -f 65001 -i "$arquivo" || exit 1
done
echo "Banco criado com sucesso."
