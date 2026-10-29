# Como o time trabalha neste repositório

## Branches

- `main`: versão estável. Ninguém faz commit direto nela.
- `feature/<assunto>`: uma branch por história do backlog (ex.: `feature/us13-mobile-offline`).
- `fix/<assunto>`: correções (ex.: `fix/nota-decimal`).

Fluxo: criar a branch, fazer commits, abrir um **pull request**, deixar outro integrante revisar e fazer o merge. O CI (GitHub Actions) precisa estar verde para o merge.

## Mensagens de commit

Formato `tipo: descrição curta no imperativo`, em português:

| Tipo | Uso | Exemplo |
|---|---|---|
| `feat` | Nova funcionalidade | `feat: adiciona fila offline de respostas no app` |
| `fix` | Correção de bug | `fix: corrige formato decimal da nota` |
| `docs` | Documentação | `docs: escreve etapa 8 (nuvem e DevOps)` |
| `test` | Testes | `test: cobre emissão de certificado` |
| `refactor` | Melhoria sem mudar comportamento | `refactor: extrai Permissoes dos serviços` |
| `chore` | Configuração, dependências, CI | `chore: adiciona workflow de CI` |

## Antes de abrir o pull request

- [ ] `dotnet test backend/AprenderMais.Testes` passando
- [ ] `npm run lint` e `npm run build` sem erros no `web/`
- [ ] App mobile abrindo (`npx expo start`)
- [ ] Documentação da etapa atualizada em `docs/`
