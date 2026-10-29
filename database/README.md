# Banco de dados – SQL Server

Scripts do banco `AprenderMais`. Explicação completa do modelo, das procedures e dos triggers em [`docs/07-banco-de-dados.md`](../docs/07-banco-de-dados.md).

| Ordem | Arquivo | O que faz |
|---|---|---|
| 1 | `01_criar_banco.sql` | Cria o banco |
| 2 | `02_tabelas.sql` | 10 tabelas, chaves, restrições e índices |
| 3 | `03_views.sql` | 3 views |
| 4 | `04_procedures.sql` | 5 stored procedures |
| 5 | `05_triggers.sql` | 4 triggers |
| 6 | `06_dados_iniciais.sql` | Dados de demonstração (senha `123456`) |
| — | `script_completo.sql` | Tudo acima em um arquivo só |

## Como criar o banco

**Opção A – SQL Server instalado no Windows (SSMS ou Azure Data Studio)**

1. Instale o [SQL Server Express](https://www.microsoft.com/pt-br/sql-server/sql-server-downloads) e o SSMS (gratuitos).
2. Abra `script_completo.sql` no SSMS e clique em **Executar** (F5).
3. Na API (`backend/AprenderMais.Api/appsettings.json`), troque:
   ```json
   "Banco": { "Tipo": "SqlServer" },
   "ConnectionStrings": { "AprenderMais": "Server=localhost\\SQLEXPRESS;Database=AprenderMais;Trusted_Connection=True;TrustServerCertificate=True" }
   ```

**Opção B – Docker (sem instalar o SQL Server)**

Na raiz do repositório: `docker compose up --build`. O serviço `banco-scripts` executa estes arquivos automaticamente.

## Testando as procedures no SSMS

```sql
USE AprenderMais;
EXEC sp_relatorio_resumo;
EXEC sp_relatorio_por_conteudo;
EXEC sp_calcular_progresso @id_aluno = 1, @id_conteudo = 2;   -- Marina em UX/UI: 1 de 2
SELECT * FROM vw_respostas_pendentes;
SELECT * FROM log_auditoria;                                   -- preenchido pelos triggers

-- Trigger em ação: tenta dar nota 11 (máximo 10) -> erro 50010
INSERT INTO nota (valor, id_resposta, id_secretaria) VALUES (11, 4, 1);
```

## Para regerar o script completo (depois de mudar algum arquivo)

PowerShell, nesta pasta:

```powershell
Get-Content 01_*.sql,02_*.sql,03_*.sql,04_*.sql,05_*.sql,06_*.sql | Set-Content script_completo.sql -Encoding utf8
```
