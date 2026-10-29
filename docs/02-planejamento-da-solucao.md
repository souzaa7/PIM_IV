# Etapa 2 – Planejamento da solução tecnológica

*Disciplina: Empreendedorismo em TI*

## Problema a ser resolvido

A Aprender Mais perde tempo e alunos porque os processos acadêmicos (matrícula, entrega, correção, notas e certificados) são manuais e espalhados em planilhas e e-mails. Além disso, o aluno não tem acesso pelo celular nem visão clara do próprio progresso.

## Objetivos da solução

1. Centralizar os processos acadêmicos em **um único sistema** com banco de dados corporativo.
2. Oferecer o sistema em **três plataformas** (web, mobile e desktop), todas usando a mesma API.
3. Calcular o progresso e **emitir certificados automaticamente**, com validação pública.
4. Permitir que o aluno estude e responda atividades **mesmo sem internet**.
5. Dar à direção **indicadores** para tomar decisões.
6. Garantir **acessibilidade e inclusão** (Etapa 3).

## Público-alvo

| Segmento | Necessidade principal |
|---|---|
| Jovens que querem o primeiro emprego em TI | Cursos práticos e certificado reconhecido |
| Adultos em transição de carreira | Estudar no próprio ritmo, inclusive pelo celular |
| Estudantes de escola pública e de baixa renda | Acesso gratuito ou de baixo custo e uso com pouca internet |
| Pessoas com deficiência visual ou baixa visão | Interface acessível |
| Equipe da escola (secretaria e direção) | Menos trabalho manual e mais controle |

## Proposta de valor

> **"Aprenda tecnologia no seu ritmo, do primeiro clique ao certificado."**
> Um ambiente simples e acessível, em que o aluno sabe exatamente quanto falta para concluir, recebe correções feitas por pessoas e ganha um certificado que qualquer empresa pode validar.

## Modelo de negócio (Business Model Canvas)

| Bloco | Descrição |
|---|---|
| **Segmentos de clientes** | Público-alvo acima; empresas parceiras que contratam turmas |
| **Proposta de valor** | Cursos práticos, progresso visível, certificado validável, app offline, acessibilidade |
| **Canais** | Site, app mobile, redes sociais, parcerias com escolas públicas e ONGs |
| **Relacionamento** | Feedback individual nas correções, suporte pela secretaria |
| **Fontes de receita** | Modelo **freemium**: cursos de base gratuitos e cursos completos com certificado pagos (mensalidade); turmas fechadas para empresas (B2B) |
| **Recursos-chave** | Plataforma (esta solução), conteudistas, secretaria acadêmica |
| **Atividades-chave** | Produção de conteúdo, correção de atividades, manutenção da plataforma |
| **Parcerias-chave** | Escolas públicas, ONGs, empresas de tecnologia da região do Vale do Paraíba |
| **Estrutura de custos** | Nuvem (Azure), equipe pedagógica, marketing, desenvolvimento |

## Análise de viabilidade (estimativa)

**Custos mensais de infraestrutura** (estimativa com base nos planos de entrada do Azure; os valores devem ser conferidos na calculadora de preços do Azure no momento da contratação):

| Item | Plano | Estimativa mensal |
|---|---|---|
| Azure App Service (API) | B1 | US$ 13 |
| Azure SQL Database | Basic / S0 | US$ 5 a 15 |
| Azure Static Web Apps (site) | Gratuito | US$ 0 |
| Application Insights | Até 5 GB de logs | US$ 0 |
| Domínio e certificado HTTPS | | R$ 5 |
| **Total aproximado** | | **US$ 20 a 30 (≈ R$ 110 a 170)** |

**Receitas (cenário conservador)**: com 60 alunos pagantes a R$ 49,90 por mês, a receita é de R$ 2.994 por mês. Ela cobre a infraestrutura com folga e ajuda a pagar a equipe pedagógica. O ponto de equilíbrio da infraestrutura é de **apenas 4 alunos pagantes**.

**Viabilidade técnica**: a equipe já domina as tecnologias escolhidas (C#/.NET, React, SQL Server), que fazem parte do curso. Todas têm versões gratuitas para desenvolvimento.

**Conclusão**: a solução é **viável** técnica e financeiramente. O maior custo é a equipe pedagógica, não a tecnologia.

## Benefícios esperados

- Redução do tempo da secretaria com controle manual (sem planilhas paralelas).
- Aluno sempre sabe seu progresso, o que reduz desistências.
- Certificados confiáveis, com validação em segundos.
- Atendimento a alunos com pouca internet (app offline).
- Dados para decisões da direção (painel de indicadores).

## Diferenciais competitivos

1. **Sincronização offline** no app: o aluno responde no ônibus e o envio acontece depois.
2. **Certificado com código de validação** público.
3. **Acessibilidade de verdade**: fonte para baixa visão (Atkinson Hyperlegible), alto contraste e tamanho de texto ajustável.
4. **Correção humana com feedback**, e não apenas questionário automático.
5. **Três plataformas** (web, mobile e desktop) sobre a mesma API e as mesmas regras de negócio.
