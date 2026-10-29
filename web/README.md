# Web – React + JavaScript (Vite)

Site da plataforma para os três perfis. Explicação completa em [`docs/04-solucao-web.md`](../docs/04-solucao-web.md).

## Como rodar

Pré-requisito: [Node.js 22](https://nodejs.org/) e a **API rodando** (veja [`backend/README.md`](../backend/README.md)).

```bash
cd web
npm install
npm run dev
```

Abra <http://localhost:5173>. Se a API estiver em outro endereço, copie `.env.example` para `.env` e ajuste `VITE_API_URL`.

| Comando | Para quê |
|---|---|
| `npm run dev` | Servidor de desenvolvimento |
| `npm run build` | Gera a versão de produção em `dist/` |
| `npm run lint` | Verifica o código (ESLint) |

## Estrutura

```text
src/
├── main.jsx                 Ponto de entrada (Router + AuthProvider)
├── App.jsx                  Mapa de rotas
├── index.css                Estilos (tema "caderno" + alto contraste)
├── servicos/api.js          Todas as chamadas à API
├── servicos/useCarregar.js  Hook de carregamento de dados
├── contexto/AuthContext.jsx Login e usuário logado
├── componentes/             Layout, menu, rota protegida, acessibilidade, avisos
└── paginas/
    ├── publico/             Início, Entrar, Cadastro, Validar certificado
    ├── aluno/               Meus estudos, Cursos, Atividades, Notas, Certificados
    ├── secretaria/          Correções, Conteúdos, Nova atividade, Cadastrar aluno
    └── admin/               Painel, Usuários
```
