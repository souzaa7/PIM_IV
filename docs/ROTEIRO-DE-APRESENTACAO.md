# Roteiro de apresentação (≈ 20 minutos)

Sugestão de divisão entre os 4 integrantes. Ajuste conforme o tempo que o professor der.

## Antes de começar (checklist)

- [ ] API rodando: `cd backend` → `dotnet run --project AprenderMais.Api` (deixe o Swagger aberto em <http://localhost:5000/swagger>)
- [ ] Site rodando: `cd web` → `npm run dev` (<http://localhost:5173>)
- [ ] App aberto no celular (Expo Go) ou no navegador (`cd mobile` → `npx expo start` → tecla `w`)
- [ ] Desktop compilado (`dotnet run --project AprenderMais.Desktop`)
- [ ] SSMS aberto com o banco `AprenderMais` (se forem mostrar o SQL Server)
- [ ] A API em modo memória **reinicia os dados** a cada execução: reinicie antes de apresentar para começar "limpo"

## Parte 1: Problema e solução (Julia, 4 min)

1. A organização (Etapa 1): escola de cursos de TI com controle em planilhas e e-mail.
2. Problemas: aluno sem visão do progresso, certificado sem validação, sem app.
3. A solução (Etapa 2): uma plataforma, três versões, uma API. Proposta de valor e modelo freemium.
4. Inclusão (Etapa 3): mostre o **A+ e o alto contraste** no site.

## Parte 2: Arquitetura e banco (Lucas, 4 min)

1. Mostre o diagrama `docs/diagramas/arquitetura/arquitetura.png`.
2. MER (`docs/diagramas/banco/mer.png`): herança de usuário em tabelas, matrícula, resposta e nota.
3. No SSMS:
   ```sql
   EXEC sp_relatorio_resumo;
   EXEC sp_calcular_progresso 1, 2;
   INSERT INTO nota (valor, id_resposta, id_secretaria) VALUES (11, 4, 1);  -- o trigger bloqueia
   SELECT * FROM log_auditoria;
   ```
4. Nuvem e DevOps (Etapa 8): `docker-compose.yml`, CI verde no GitHub e o plano de deploy na Azure.

## Parte 3: Back-end e integração (Vitor, 5 min)

1. Camadas no Visual Studio: Dominio → Aplicacao → Infraestrutura → Api/Desktop.
2. Mostre `Usuario` (abstrata) e `Aluno : Usuario` (herança), e `Matricula.AtualizarProgresso` (encapsulamento).
3. No **Swagger**:
   - `POST /api/auth/login` com `secretaria@aprendermais.com` / `123456` → copiar o `accessToken` → **Authorize**
   - `GET /api/respostas/pendentes` → mostrar a resposta do Bruno
   - `GET /api/relatorios/resumo` logado como secretaria → **403** (autorização por perfil)
4. Abra o **desktop** como secretaria → aba "Corrigir atividades": mesmo dado, mesma regra, sem passar pela API.
5. Rode os testes: `dotnet test backend/AprenderMais.Testes` → 49 aprovados.

## Parte 4: Web e mobile, demonstração do fluxo completo (Gabriel, 6 min)

O fluxo que mostra **tudo integrado**:

1. **App (aluno Marina)**: entrar com `aluno@aprendermais.com` → aba Atividades.
2. **Modo offline**: desligue o Wi-Fi do celular (ou pare a API) → responda "Protótipo de tela acessível" → aparece *"Guardada no celular"* e a faixa amarela.
3. Religue → toque em **Sincronizar** → a resposta vai para *"Aguardando correção"*.
4. **Site (secretaria)**: entrar com `secretaria@aprendermais.com` → Correções → a resposta da Marina está lá → nota 10.
   Mensagem: *"Marina Costa concluiu UX/UI Design e recebeu o certificado AM-2026-…"* (era a última atividade do curso).
5. **App**: sincronizar → aba Início mostra **UX/UI Design 100%** → aba Perfil mostra o **novo certificado**.
6. **Site (público)**: "Validar certificado" → cole o código → *"Certificado válido"*.
7. **Site (admin)**: `admin@aprendermais.com` → Painel → "Certificados emitidos" subiu para 2.

## Encerramento (Julia, 1 min)

- Sprints de 1 semana, backlog e Kanban (`docs/09-gerenciamento-agil.md`).
- O que aprendemos e próximos passos (deploy real na Azure, publicar o app na Play Store).

## Se algo der errado na hora

| Problema | Solução rápida |
|---|---|
| Site mostra "Não foi possível conectar à API" | A API não está rodando: `dotnet run --project AprenderMais.Api` |
| App no celular não conecta | Coloque o IP do computador em `mobile/src/config.js` e rode a API com `--urls http://0.0.0.0:5000` |
| "Sua sessão expirou" | A API foi reiniciada (o token antigo não vale mais): entre de novo |
| A resposta da Marina já foi corrigida em um ensaio | Reinicie a API (modo memória volta aos dados iniciais) |
