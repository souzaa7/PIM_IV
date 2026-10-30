# Guia de estudo: como o Aprender Mais funciona

Este guia explica **o sistema inteiro**, do clique no botão até o banco de dados, para que qualquer integrante do time consiga apresentar e responder perguntas. Leia na ordem: cada parte usa o que foi explicado antes.

**Sumário**

1. [A ideia em 1 minuto](#1-a-ideia-em-1-minuto)
2. [Mapa do repositório](#2-mapa-do-repositório)
3. [O caminho de um clique (do botão ao banco e de volta)](#3-o-caminho-de-um-clique)
4. [Back-end em C#](#4-back-end-em-c)
5. [Banco de dados SQL Server](#5-banco-de-dados-sql-server)
6. [Front-end web em React](#6-front-end-web-em-react)
7. [Integração front ↔ back](#7-integração-front--back)
8. [App mobile](#8-app-mobile)
9. [Desktop](#9-desktop)
10. [Testes, CI e Docker](#10-testes-ci-e-docker)
11. [Glossário](#11-glossário)
12. [Perguntas que o professor pode fazer](#12-perguntas-que-o-professor-pode-fazer)

---

## 1. A ideia em 1 minuto

Pense num **restaurante**:

| No restaurante | No sistema | Pasta |
|---|---|---|
| Cliente que faz o pedido | **Telas**: site, app e desktop | `web/`, `mobile/`, `backend/AprenderMais.Desktop` |
| Garçom que leva o pedido | **API REST**: recebe pedidos pela internet | `backend/AprenderMais.Api` |
| Cozinheiro que segue a receita | **Serviços e regras de negócio** | `backend/AprenderMais.Aplicacao` e `AprenderMais.Dominio` |
| Despensa | **Banco de dados** | `database/` (SQL Server) |
| Quem busca os ingredientes na despensa | **Repositórios** | `backend/AprenderMais.Infraestrutura` |

O cliente nunca entra na cozinha: **o site e o app só falam com a API**, nunca direto com o banco. Assim, as regras (por exemplo, "nota não pode passar de 10") ficam em **um lugar só** e valem para as três versões.

```text
 Site React ─┐
             ├──HTTP/JSON──▶  API (.NET) ──▶ Serviços ──▶ Repositórios ──▶ SQL Server
 App Expo ───┘                                  ▲
 Desktop WinForms ──────────────────────────────┘ (usa os serviços direto, sem passar pela API)
```

---

## 2. Mapa do repositório

```text
PIM_IV/
├── backend/                        TUDO EM C# (.NET 10)
│   ├── AprenderMais.Dominio/         "o que o negócio é": classes Aluno, Matricula, Nota... e suas regras
│   ├── AprenderMais.Aplicacao/       "o que o sistema faz": um serviço por assunto (RespostaService...)
│   ├── AprenderMais.Infraestrutura/  "onde os dados ficam": SQL Server ou memória
│   ├── AprenderMais.Api/             "a porta de entrada da internet": controllers REST
│   ├── AprenderMais.Desktop/         programa Windows (telas WinForms)
│   └── AprenderMais.Testes/          49 testes automáticos
├── web/                            site em React (JavaScript)
├── mobile/                         app em React Native + Expo (JavaScript)
├── database/                       scripts .sql do SQL Server
└── docs/                           documentação das 9 etapas + diagramas + este guia
```

---

## 3. O caminho de um clique

O melhor jeito de entender o sistema é seguir **um clique** até o fim. Vamos usar o **UC06 – a secretaria dá nota para a resposta do Bruno**.

### Passo 1: o botão no site (React)

`web/src/paginas/secretaria/Correcoes.jsx`

```jsx
async function salvar(evento, resposta) {
  evento.preventDefault()
  const resultado = await respostas.avaliar(resposta.idResposta, Number(valor), feedback)
  setMensagem({ tipo: 'sucesso', texto: `Nota salva. Progresso ... ${resultado.progressoDoAluno}%.` })
}
```

A tela não sabe nada de banco: ela só chama a função `respostas.avaliar(...)`.

### Passo 2: a chamada HTTP

`web/src/servicos/api.js`

```js
avaliar: (id, valor, feedback) =>
  requisicao(`/api/respostas/${id}/nota`, { metodo: 'POST', corpo: { valor, feedback } }),
```

A função `requisicao` faz um `fetch`, ou seja, envia pela rede:

```http
POST http://localhost:5000/api/respostas/4/nota
Authorization: Bearer CfDJ8DPtxw...   ← token de quem está logado
Content-Type: application/json

{ "valor": 8, "feedback": "Bom" }
```

### Passo 3: a API recebe (Controller)

`backend/AprenderMais.Api/Controllers/RespostasController.cs`

```csharp
[HttpPost("{id:int}/nota")]
[Authorize(Roles = Perfis.Secretaria)]          // só secretaria entra aqui
public async Task<ActionResult<AvaliacaoResultadoDto>> Avaliar(int id, AvaliarRespostaDto dto) =>
    Ok(await _respostas.AvaliarAsync(id, dto, await UsuarioLogadoAsync()));
```

- `[HttpPost("{id:int}/nota")]` liga a URL ao método.
- `[Authorize(Roles = ...)]` faz o ASP.NET Core conferir o token e o perfil. Se for um aluno, a resposta é **403** e o método nem roda.
- O JSON do corpo vira automaticamente o objeto `AvaliarRespostaDto`.
- O controller **não tem regra de negócio**: ele só repassa para o serviço.

### Passo 4: a regra de negócio (Serviço)

`backend/AprenderMais.Aplicacao/Servicos/RespostaService.cs`, resumido:

```csharp
public async Task<AvaliacaoResultadoDto> AvaliarAsync(int idResposta, AvaliarRespostaDto dto, Usuario solicitante)
{
    var secretaria = Permissoes.ExigirSecretaria(solicitante);          // confere o perfil de novo
    var resposta   = await _respostas.ObterPorIdAsync(idResposta) ?? throw ...;
    if (await _notas.ObterPorRespostaAsync(idResposta) is not null)
        throw new DominioException("Esta resposta já foi avaliada.");

    // 1) grava a nota
    var nota = new Nota { Valor = dto.Valor, ... };
    nota.Validar(atividade.NotaMaxima);                                  // regra dentro da entidade
    await _notas.AdicionarAsync(nota);

    // 2) recalcula o progresso
    var (avaliadas, total) = await _matriculas.ContarAtividadesAsync(...);
    matricula.AtualizarProgresso(avaliadas, total, agora);
    await _matriculas.AtualizarAsync(matricula);

    // 3) se chegou a 100%, emite o certificado
    if (matricula.Status == StatusMatricula.Concluido)
        await _certificados.EmitirAsync(...);
}
```

O serviço usa **interfaces** (`_notas`, `_matriculas`...). Ele não sabe se os dados estão no SQL Server ou na memória.

### Passo 5: a regra dentro da classe (Domínio)

`backend/AprenderMais.Dominio/Entidades/Matricula.cs`

```csharp
public void AtualizarProgresso(int atividadesAvaliadas, int totalAtividades, DateTime agoraUtc)
{
    Progresso = Math.Round(100m * atividadesAvaliadas / totalAtividades, 2);
    if (Progresso >= 100m)
    {
        Status = StatusMatricula.Concluido;
        DataConclusao ??= agoraUtc;
    }
}
```

Isso é **encapsulamento**: quem sabe como uma matrícula é concluída é a própria classe `Matricula`.

### Passo 6: o banco (Repositório + SQL)

`backend/AprenderMais.Infraestrutura/SqlServer/MatriculaRepositorioSql.cs`

```csharp
await using var comando = new SqlCommand("sp_calcular_progresso", conexao)
{
    CommandType = CommandType.StoredProcedure
};
comando.Parameters.AddWithValue("@id_aluno", idAluno);
```

Ao gravar a nota, o **trigger** `trg_nota_validar` confere de novo se o valor está entre 0 e a nota máxima. É uma segunda proteção, para o caso de alguém mexer direto no banco.

### Passo 7: a volta

O serviço devolve `{ progressoDoAluno: 50, conteudoConcluido: false }`. O controller transforma isso em JSON com o código **200**, e o React mostra *"Nota salva. Progresso de Bruno Lima em Engenharia de Software: 50%."*

Na próxima vez que o Bruno abrir o **app**, ele recebe a nota e o progresso novo pela sincronização.

> **Resumo para a apresentação:** Tela → `api.js` → HTTP → Controller (confere o token) → Serviço (regras) → Entidade (regra da classe) → Repositório → SQL Server (procedure/trigger) → volta como JSON → tela atualiza.

---

## 4. Back-end em C#

### 4.1 As 4 camadas e por que existem

| Camada | Pergunta que responde | Pode depender de |
|---|---|---|
| **Dominio** | O que é um aluno, uma nota, uma matrícula? Quais regras eles têm? | ninguém |
| **Aplicacao** | O que o sistema faz? (matricular, avaliar, emitir certificado) | Dominio |
| **Infraestrutura** | Onde e como os dados são guardados? | Dominio, Aplicacao |
| **Api / Desktop** | Como o usuário acessa? | todas |

**Vantagem prática:** trocamos o banco (memória ↔ SQL Server) **sem alterar uma linha** dos serviços nem das telas. Só muda a configuração.

### 4.2 Orientação a objetos no projeto

- **Herança**: `Aluno`, `Secretaria` e `Administrador` herdam de `Usuario` (`Dominio/Entidades/Usuario.cs`). Todos têm nome, e-mail e senha, e cada um tem o seu id de perfil.
- **Classe abstrata e polimorfismo**: `Usuario` é `abstract` e tem `public abstract PerfilUsuario Perfil { get; }`. Cada filha responde diferente (`Aluno` → `PerfilUsuario.Aluno`).
- **Encapsulamento**: as regras ficam nos métodos das entidades (`Matricula.Cancelar()`, `Nota.Validar()`, `Conteudo.Validar()`).
- **Interfaces**: `IUsuarioRepositorio` diz *o que* precisa existir (`ObterPorEmailAsync`...). `UsuarioRepositorioSql` e `UsuarioRepositorioMemoria` dizem *como*.
- **Exceção própria**: `DominioException` representa uma regra violada. A mensagem dela é mostrada para o usuário.

### 4.3 Injeção de dependência (`Api/Program.cs`)

O `Program.cs` é a "montagem" do sistema:

```csharp
var repositorios = FabricaRepositorios.Criar(
    builder.Configuration["Banco:Tipo"],                     // "Memoria" ou "SqlServer"
    builder.Configuration.GetConnectionString("AprenderMais"),
    hasher);

builder.Services.AddSingleton(repositorios.Usuarios);        // registra cada repositório
builder.Services.AddScoped<RespostaService>();               // e cada serviço
```

Quando o ASP.NET Core cria o `RespostasController`, ele vê que o construtor pede um `RespostaService` e cria o serviço automaticamente, já com os repositórios dentro. Ninguém dá `new` manualmente.

### 4.4 Autenticação (login e token)

1. `POST /api/auth/login` com e-mail e senha.
2. `AutenticacaoService.EntrarAsync` busca o usuário e compara a senha com **BCrypt** (`BCrypt.Verify`). A senha no banco é um *hash*, não dá para "descriptografar".
3. O `AuthController` monta as **claims** (id, nome, e-mail, perfil) e chama `SignIn(...)`. O ASP.NET Core gera um **token criptografado** e devolve `{ accessToken, expiresIn }`.
4. O front guarda o token e envia em toda requisição: `Authorization: Bearer <token>`.
5. `[Authorize(Roles = "Secretaria")]` lê o perfil de dentro do token.
6. `BaseController.UsuarioLogadoAsync()` busca o usuário no banco a cada requisição. Se o ADM o desativou, o token deixa de valer.

### 4.5 Tratamento de erros (`Api/Configuracao/TratamentoDeErros.cs`)

É um *middleware*, um código que "envolve" todas as requisições:

- `DominioException` → **400** com `{ "mensagem": "A nota não pode ser maior que a nota máxima da atividade." }`
- `UnauthorizedAccessException` → **401**
- qualquer outro erro → **500** com mensagem genérica (o detalhe vai para o log)

Por isso o React só precisa ler `dados.mensagem` para mostrar o erro.

### 4.6 Os dois "bancos" (`Infraestrutura/FabricaRepositorios.cs`)

- **Memoria**: listas em C# com os mesmos dados do script SQL (`Memoria/DadosIniciais.cs`). Serve para rodar sem instalar o SQL Server, para os testes e para demonstração. Os dados voltam ao início quando a API reinicia.
- **SqlServer**: ADO.NET puro (`SqlConnection`, `SqlCommand`, `SqlDataReader`). Todo comando usa **parâmetros** (`@email`), o que impede SQL Injection.

---

## 5. Banco de dados SQL Server

### 5.1 Tabelas (`database/02_tabelas.sql`)

```text
usuario ─┬─ aluno ──────┬── matricula ── conteudo ── atividade
         │              ├── resposta ─────────────────┘   │
         │              └── certificado                    │
         ├─ secretaria ─┬── conteudo (quem publicou)       │
         │              └── nota ── resposta ◀─────────────┘
         └─ administrador
log_auditoria (preenchida pelos triggers)
```

- A **herança** do C# virou **tabelas de especialização**: `aluno`, `secretaria` e `administrador` têm um `id_usuario` que aponta para `usuario`.
- As **restrições** (*constraints*) garantem dados corretos:
  - `UNIQUE (email)`: não há dois usuários com o mesmo e-mail.
  - `UNIQUE (id_aluno, id_conteudo)`: um aluno não se matricula duas vezes no mesmo curso.
  - `UNIQUE (id_aluno, id_atividade)`: uma resposta por atividade.
  - `CHECK (status IN ('em_andamento','concluido','cancelado'))`
  - `CHECK (progresso BETWEEN 0 AND 100)`
- Os **índices** (`CREATE INDEX`) aceleram as buscas mais comuns, como os conteúdos ativos e as matrículas de um aluno.

### 5.2 View = consulta salva com nome

```sql
CREATE OR ALTER VIEW vw_respostas_pendentes AS
SELECT r.id_resposta, u.nome AS nome_aluno, at.titulo AS titulo_atividade, ...
FROM resposta r JOIN aluno a ... JOIN usuario u ... JOIN atividade at ...
WHERE NOT EXISTS (SELECT 1 FROM nota n WHERE n.id_resposta = r.id_resposta);
```

Em vez de repetir esse JOIN grande no C#, o repositório faz só `SELECT * FROM vw_respostas_pendentes`.

### 5.3 Procedure = função guardada no banco

```sql
EXEC sp_calcular_progresso @id_aluno = 1, @id_conteudo = 2;
-- avaliadas | total
--     1     |   2
```

`sp_cadastrar_usuario` é o melhor exemplo para mostrar **transação**: ela grava em `usuario` **e** em `aluno` juntos. Se a segunda parte falhar, a primeira é desfeita (`BEGIN TRANSACTION ... COMMIT` + `SET XACT_ABORT ON`).

### 5.4 Trigger = código que roda sozinho quando a tabela muda

```sql
CREATE OR ALTER TRIGGER trg_conteudo_desativado ON conteudo AFTER UPDATE AS
BEGIN
    UPDATE at SET ativo = 0
      FROM atividade at JOIN inserted i ... JOIN deleted d ...
     WHERE d.ativo = 1 AND i.ativo = 0;      -- estava ativo e foi desativado
END
```

- `deleted` = como a linha **era antes**; `inserted` = como **ficou depois**.
- Demonstração ao vivo: desative um curso e rode `SELECT * FROM log_auditoria`.

### 5.5 Por que a regra está no C# **e** no banco?

- No **C#**: para dar uma **mensagem amigável** antes de tentar gravar.
- No **banco**: como **última defesa**, para o caso de alguém inserir dados direto no SSMS ou de surgir outro sistema no futuro.

---

## 6. Front-end web em React

### 6.1 O básico de React em 4 ideias

1. **Componente** = função que devolve HTML (JSX). Ex.: `function Notas() { return <h1>Notas</h1> }`
2. **Estado** (`useState`) = variável que, quando muda, faz a tela redesenhar. Ex.: `const [texto, setTexto] = useState('')`
3. **Efeito** (`useEffect`) = código que roda quando a tela aparece. Usamos para buscar dados da API.
4. **Props** = parâmetros que um componente passa para outro. Ex.: `<Progresso valor={50} />`

### 6.2 Como o site é organizado

| Arquivo | Papel |
|---|---|
| `main.jsx` | Liga o React na página e envolve tudo com o `BrowserRouter` (rotas) e o `AuthProvider` (login) |
| `App.jsx` | **Mapa de rotas**: qual endereço abre qual página |
| `servicos/api.js` | **Única porta** para a API. Coloca o token e trata erros |
| `servicos/useCarregar.js` | Hook reaproveitável: chama a API, guarda `dados`, `erro` e `carregando` |
| `contexto/AuthContext.jsx` | Guarda quem está logado para todas as páginas (`useAuth()`) |
| `componentes/RotaProtegida.jsx` | Se não está logado, manda para `/entrar`; se o perfil está errado, manda para a página certa |
| `componentes/Layout.jsx` | Topo, menu (diferente para cada perfil) e rodapé |
| `paginas/...` | Uma página por tela |

### 6.3 Uma página completa explicada (`paginas/aluno/Notas.jsx`)

```jsx
export default function Notas() {
  const { dados, erro, carregando } = useCarregar(notas.minhas)   // 1) busca GET /api/notas/minhas
  const media = dados?.length ? dados.reduce((s, n) => s + n.valor, 0) / dados.length : null  // 2) calcula

  return (
    <>
      <h1>Notas</h1>
      <Estado carregando={carregando} erro={erro} vazio={dados?.length === 0} ... />   {/* 3) estados */}
      <table> {dados.map((n) => <tr>...</tr>)} </table>                               {/* 4) lista */}
    </>
  )
}
```

### 6.4 Acessibilidade (Etapa 3)

`componentes/Acessibilidade.jsx` muda o `font-size` da página (A− / A+) e liga `data-contraste="alto"`. O CSS (`index.css`) troca todas as cores pelas variáveis do alto contraste. As escolhas ficam salvas no `localStorage`.

---

## 7. Integração front ↔ back

| Conceito | No projeto |
|---|---|
| **HTTP** | Protocolo de comunicação. Os métodos usados: GET (buscar), POST (criar), PUT (editar), PATCH (alterar um campo), DELETE (cancelar) |
| **REST** | Padrão de URLs por recurso: `/api/conteudos`, `/api/conteudos/3`, `/api/conteudos/3/ativo` |
| **JSON** | Formato dos dados. O C# usa `PascalCase` (`ProgressoDoAluno`) e o JSON sai em `camelCase` (`progressoDoAluno`) automaticamente |
| **Token** | Vai no cabeçalho `Authorization: Bearer ...` (`api.js` faz isso sozinho) |
| **Status code** | 200 OK, 201 criado, 204 sem conteúdo, 400 regra violada, 401 sem login, 403 sem permissão |
| **CORS** | O navegador bloqueia chamadas entre portas diferentes (5173 → 5000). A API libera as origens listadas em `appsettings.json` → `Cors:OrigensPermitidas` |
| **DTO** | Objeto "de transporte" (`Aplicacao/Dtos/Dtos.cs`). A API nunca devolve a entidade inteira; por exemplo, o `senha_hash` nunca sai |
| **Swagger** | Página `/swagger` que lista todos os endpoints e deixa testar. Ótima para a demonstração |

**Endereço da API no front:**
- Web: `web/.env` → `VITE_API_URL` (padrão `http://localhost:5000`)
- Mobile: `mobile/src/config.js`

---

## 8. App mobile

### 8.1 React Native e Expo

- **React Native** = React para celular. Em vez de `<div>` e `<p>`, usamos `<View>` e `<Text>`, e o estilo é escrito em JavaScript (`StyleSheet.create`).
- **Expo** = ferramenta que facilita: com o app **Expo Go** você lê um QR Code e o app abre no seu celular, sem Android Studio.
- A lógica (estado, hooks, `fetch`) é **igual à do site**.

### 8.2 Navegação (`App.js`)

- **Pilha** (*Stack*): Login → Cadastro; Abas → Responder (uma tela "por cima" da outra, com botão voltar).
- **Abas** (*Bottom Tabs*): Início, Cursos, Atividades, Notas, Perfil.
- Se `logado` é falso, a pilha só tem Login e Cadastro. Ao entrar, ela troca para as abas.

### 8.3 Sincronização offline (o diferencial)

Três peças:

| Arquivo | Função |
|---|---|
| `servicos/armazenamento.js` | **AsyncStorage**, a "memória" do celular que não se apaga ao fechar o app. Guarda o token, o pacote de dados e a fila |
| `servicos/sincronizacao.js` | Envia a fila e baixa o pacote |
| `contexto/AppContext.js` | Guarda o estado (online/offline, fila, dados) para todas as telas |

O que acontece quando o aluno responde **sem internet**:

```js
// sincronizacao.js (resumido)
export async function enviarResposta(idAtividade, texto) {
  try {
    await api.enviarResposta(idAtividade, texto)   // tenta enviar
    return 'enviada'
  } catch (erro) {
    if (!(erro instanceof SemConexaoErro)) throw erro   // erro de regra (ex.: prazo): mostra ao aluno
    fila.push({ idAtividade, texto })                   // sem internet: guarda na fila
    return 'fila'
  }
}
```

Na sincronização seguinte (ao abrir o app, ao puxar a tela ou ao tocar em "Sincronizar"):
1. envia cada item da fila;
2. chama `GET /api/sincronizacao`, que traz **tudo de uma vez** (cursos, matrículas, atividades, notas, certificados);
3. salva esse pacote no celular. Se a internet cair, as telas continuam mostrando a última cópia.

---

## 9. Desktop

- **Windows Forms** (C#): janelas com `TabControl`, `DataGridView` (tabelas) e `Button`. As telas foram montadas em código (`Formularios/PrincipalForm.cs`), sem o designer arrastar-e-soltar, para ficar fácil de ler.
- **Diferença importante:** o desktop **não chama a API**. Ele usa **os mesmos serviços** (`RespostaService`, `ConteudoService`...) direto, porque é C# também. As regras continuam idênticas, porque o código é literalmente o mesmo.
- O banco é escolhido em `AprenderMais.Desktop/appsettings.json`, igual à API.
- Abas por perfil:
  - **Aluno**: Catálogo, Minhas matrículas, Minhas notas.
  - **Secretaria**: Gerenciar conteúdos, Corrigir atividades (abre a janela `AvaliarRespostaForm`).
  - **Admin**: Usuários.

---

## 10. Testes, CI e Docker

- **Testes** (`backend/AprenderMais.Testes`, xUnit): cada `[Fact]` monta o sistema em memória (`Ambiente.cs`), executa uma ação e confere o resultado com `Assert`. Exemplo: `Concluir_todas_as_atividades_emite_certificado`. O relógio é fixo (`RelogioFixo`) para os testes de prazo darem sempre o mesmo resultado.
  ```bash
  dotnet test backend/AprenderMais.Testes
  ```
- **CI** (`.github/workflows/ci.yml`): a cada push, o GitHub compila o back-end, roda os testes, compila o desktop num Windows, faz lint e build do site, gera o bundle do app e monta as imagens Docker. O selo verde no README mostra que está tudo passando.
- **Docker**: `docker compose up --build` sobe 4 containers (SQL Server, scripts do banco, API e site). Cada `Dockerfile` tem duas etapas: uma compila, a outra só roda (imagem menor).

---

## 11. Glossário

| Termo | Explicação curta |
|---|---|
| API REST | Conjunto de endereços (URLs) que recebem e devolvem JSON |
| Endpoint | Um endereço da API, ex.: `GET /api/notas/minhas` |
| Controller | Classe C# que recebe as requisições de um assunto |
| DTO | Objeto só para transportar dados entre API e tela |
| Entidade | Classe que representa algo do negócio (Aluno, Nota) |
| Repositório | Classe que lê e grava dados de uma entidade |
| Injeção de dependência | O framework entrega os objetos que uma classe precisa pelo construtor |
| Middleware | Código que roda em toda requisição (ex.: tratamento de erros) |
| Token Bearer | "Crachá" digital que prova quem está logado |
| Claim | Informação dentro do token (id, perfil...) |
| Hash (BCrypt) | Transformação da senha que não pode ser desfeita |
| CORS | Regra do navegador sobre chamadas entre sites diferentes |
| Hook (React) | Função que começa com `use` (`useState`, `useEffect`, `useCarregar`) |
| Context (React) | Dados compartilhados entre várias telas sem passar de uma em uma |
| AsyncStorage | Armazenamento permanente do celular |
| Procedure | Rotina SQL guardada no banco e chamada por nome |
| Trigger | Rotina SQL que roda sozinha quando uma tabela muda |
| View | Consulta SQL salva com um nome |
| Constraint | Regra da tabela (PK, FK, UNIQUE, CHECK, DEFAULT) |
| CI/CD | Integração contínua (testar a cada push) / entrega contínua (publicar automaticamente) |
| Container | "Caixa" com o programa e tudo que ele precisa para rodar (Docker) |

---

## 12. Perguntas que o professor pode fazer

**Por que três versões (web, mobile, desktop) não duplicam código?**
Porque as regras estão nos projetos `Dominio` e `Aplicacao`. A API e o desktop usam esses projetos diretamente, e o site e o app usam a API. A regra "nota não pode passar de 10" está escrita **uma vez** (`Nota.Validar`).

**Onde está a orientação a objetos?**
Herança `Usuario` → `Aluno/Secretaria/Administrador`; classe abstrata e polimorfismo na propriedade `Perfil`; encapsulamento nas regras das entidades; interfaces nos repositórios.

**Como funciona a autenticação?**
Senha conferida com BCrypt, token Bearer gerado pelo ASP.NET Core com o perfil dentro, `[Authorize(Roles)]` em cada rota, e o usuário é conferido no banco a cada requisição (quem foi desativado perde o acesso na hora).

**E se alguém tentar SQL Injection?**
Todos os comandos usam parâmetros (`@email`), nunca concatenação de texto.

**Para que servem os triggers se o C# já valida?**
São a última defesa: protegem os dados mesmo se alguém alterar o banco fora do sistema. Além disso, registram a auditoria automaticamente.

**Como o app funciona sem internet?**
Ele guarda no AsyncStorage o último pacote baixado de `/api/sincronizacao` e uma fila de respostas. Quando a conexão volta, envia a fila e baixa tudo de novo.

**Como vocês garantem que nada quebrou?**
49 testes automáticos e o CI no GitHub Actions, que compila e testa tudo a cada push.

**Como seria colocar em produção?**
Imagens Docker publicadas no Azure Container Registry, API no App Service, banco no Azure SQL, site no Static Web Apps, segredos no Key Vault e monitoramento com Application Insights e `/saude` (Etapa 8).

**Por que SQL Server e não MySQL, como no PIM III?**
Melhor integração com .NET e Azure (Azure SQL), e suporte completo a procedures, triggers e transações em T-SQL, que são cobrados na disciplina.

**Qual foi a maior dificuldade?**
Sugestão de resposta: manter as mesmas regras nas três versões e fazer o modo offline do app, que exigiu a fila de respostas e o tratamento de quando a API recusa uma resposta guardada (por exemplo, prazo vencido).
