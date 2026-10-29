# Etapa 1 – Caracterização da organização

> Organização estudada no PIM III e mantida no PIM IV: **Aprender Mais – Educação Tecnológica**, escola de cursos livres de tecnologia com sede em São José dos Campos (SP).

## Segmento de atuação

Educação, no ramo de **cursos livres de tecnologia da informação** para iniciantes e estudantes de nível intermediário. A escola atende pessoas de qualquer faixa etária que querem entrar ou crescer na área de TI, de forma presencial e remota.

## Produtos e serviços

| Curso | Categoria | Nível | Carga |
|---|---|---|---|
| Engenharia de Software | Engenharia | Iniciante | 18h (28 aulas) |
| UX/UI Design | Design | Intermediário | 14h (24 aulas) |
| Microsoft 365 | Produtividade | Iniciante | 10h (18 aulas) |
| Fundamentos de T.I. | Base técnica | Iniciante | 12h (21 aulas), em reformulação |

Cada curso tem atividades avaliativas corrigidas pela secretaria acadêmica. Quem conclui todas as atividades recebe o **certificado de conclusão**, que pode ser usado em processos seletivos e em horas complementares.

## Estrutura organizacional

```text
                 Direção / Administração do sistema (ADM)
                                  │
        ┌─────────────────────────┼─────────────────────────┐
 Secretaria acadêmica       Conteudistas / tutores       Suporte de TI
 (matrículas, correções,    (produzem as aulas e as      (infraestrutura,
  atividades, certificados)  atividades)                  contas e acessos)
                                  │
                               Alunos
```

Os três perfis do sistema refletem essa estrutura: **Administrador** (direção e TI), **Secretaria** (secretaria acadêmica e tutores) e **Aluno**.

## Principais processos

1. **Captação e cadastro**: o interessado conhece os cursos e se cadastra (UC02).
2. **Matrícula e estudo**: o aluno escolhe o curso e acessa conteúdos e atividades (UC03).
3. **Entrega de atividades**: o aluno envia as respostas dentro do prazo (UC04).
4. **Correção**: a secretaria corrige e dá a nota com feedback (UC06).
5. **Acompanhamento**: o aluno consulta as notas e o progresso (UC05).
6. **Certificação**: o certificado é emitido ao concluir o curso.
7. **Gestão de conteúdo**: a secretaria cria, edita e desativa cursos e atividades (UC07 a UC09).
8. **Gestão de acessos**: o administrador cadastra secretarias e desativa cadastros (UC10 e UC11).

Os fluxos de cada processo estão nos [diagramas de atividade](diagramas/atividade/README.md).

## Problemas identificados (levantamento do PIM III)

- Matrículas e notas controladas em **planilhas e e-mail**, com retrabalho e risco de perda.
- O aluno não sabe quanto falta para concluir o curso; ele precisa perguntar para a secretaria.
- As entregas chegam por e-mail fora do prazo e sem padrão, e é difícil saber o que já foi corrigido.
- Certificados feitos manualmente em editor de texto, **sem forma de validação** por empresas.
- Não há acesso pelo celular, e muitos alunos estudam no transporte, com internet instável.
- Ex-funcionários continuavam com acesso aos materiais, porque não havia controle de desativação.
- A direção não tem indicadores (alunos ativos, conclusões, médias).

## Oportunidades de melhoria por meio da tecnologia

| Problema | Oportunidade (o que o sistema faz) |
|---|---|
| Planilhas e e-mail | Banco de dados único (SQL Server) com API acessada por web, mobile e desktop |
| Aluno sem visão do progresso | Barra de progresso calculada automaticamente a cada correção |
| Entregas desorganizadas | Envio pelo sistema com controle de prazo e fila de correção |
| Certificado sem validação | Certificado emitido automaticamente, com código validado em página pública |
| Internet instável | App mobile com sincronização offline |
| Acessos indevidos | Desativação de cadastros com bloqueio imediato e log de auditoria |
| Falta de indicadores | Painel do administrador com relatórios gerados por procedures |
