# Back-end – C# / .NET 10

Solução `AprenderMais.slnx` com a API REST, o sistema desktop e os testes. A arquitetura está explicada em [`docs/06-arquitetura-de-software.md`](../docs/06-arquitetura-de-software.md).

```text
backend/
├── AprenderMais.Dominio/          Entidades, regras de negócio e interfaces dos repositórios
├── AprenderMais.Aplicacao/        Serviços (um por assunto), DTOs e permissões
├── AprenderMais.Infraestrutura/   Repositórios SQL Server (ADO.NET) e em Memória, BCrypt, relógio
├── AprenderMais.Api/              API REST (ASP.NET Core): controllers, token Bearer, Swagger
├── AprenderMais.Desktop/          Sistema desktop (Windows Forms)
└── AprenderMais.Testes/           49 testes automatizados (xUnit)
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (carga "ASP.NET e desenvolvimento Web" + "Desenvolvimento para desktop com .NET") **ou** VS Code com C# Dev Kit
- SQL Server (opcional: sem ele o sistema roda **em memória** com os dados de demonstração)

## Rodando a API

```bash
cd backend
dotnet run --project AprenderMais.Api
```

- Swagger: <http://localhost:5000/swagger>
- Health check: <http://localhost:5000/saude>

Por padrão (`appsettings.json` → `"Banco": { "Tipo": "Memoria" }`) os dados ficam em memória e voltam ao estado inicial a cada execução. Para usar o SQL Server, crie o banco ([`database/README.md`](../database/README.md)) e mude para `"SqlServer"`.

**Testando pelo Swagger:** em `POST /api/auth/login`, envie `{ "email": "aluno@aprendermais.com", "senha": "123456" }`. Copie o `accessToken`, clique em **Authorize** e cole o token. As rotas do aluno passam a funcionar.

## Rodando o desktop (Windows)

```bash
dotnet run --project AprenderMais.Desktop
```

Ou abra `AprenderMais.slnx` no Visual Studio, defina **AprenderMais.Desktop** como projeto de inicialização e pressione F5. O banco é escolhido em `AprenderMais.Desktop/appsettings.json`, igual ao da API.

| Perfil | O que aparece no desktop |
|---|---|
| Aluno | Catálogo, Minhas matrículas, Minhas notas |
| Secretaria | Catálogo, Gerenciar conteúdos, Corrigir atividades, Novo aluno |
| Administrador | Catálogo, Usuários (cadastrar secretaria, ativar/desativar) |

## Rodando os testes

```bash
dotnet test AprenderMais.Testes
```

## Contas de demonstração (senha `123456`)

| Perfil | E-mail |
|---|---|
| Aluno | aluno@aprendermais.com (Marina) e bruno@aprendermais.com (Bruno) |
| Secretaria | secretaria@aprendermais.com |
| Administrador | admin@aprendermais.com |
| Aluno inativo (não consegue entrar) | ana@aprendermais.com |
