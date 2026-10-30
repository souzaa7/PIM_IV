# Documentação do PIM IV

Cada arquivo corresponde a uma etapa do **Manual do PIM IV**. O conteúdo serve de base para o documento final em ABNT.

| Etapa | Documento | Pontos |
|---|---|---|
| 1 | [Caracterização da organização](01-caracterizacao-da-organizacao.md) | 1,0 |
| 2 | [Planejamento da solução tecnológica](02-planejamento-da-solucao.md) (Empreendedorismo em TI) | 1,0 |
| 3 | [Responsabilidade social e diversidade](03-responsabilidade-social-e-diversidade.md) | 0,5 |
| 4 | [Solução Web](04-solucao-web.md) (Desenvolvimento Web com .NET) | 2,0 |
| 5 | [Solução Mobile](05-solucao-mobile.md) (Desenvolvimento Mobile) | 2,0 |
| 6 | [Arquitetura de Software](06-arquitetura-de-software.md) (Programação Aplicada em .NET) | — |
| 7 | [Banco de Dados](07-banco-de-dados.md) (Programação de Banco de Dados) | 1,0 |
| 8 | [Nuvem e DevOps](08-nuvem-e-devops.md) (Cloud Computing e DevOps) | 0,5 |
| 9 | [Gerenciamento Ágil](09-gerenciamento-agil.md) (Gerenciamento de Projetos Ágil) | 1,0 |
| — | [Referências (ABNT)](referencias.md) | 1,0 (ABNT) |

## Para estudar e apresentar

| Arquivo | Para quê |
|---|---|
| [GUIA-DE-ESTUDO.md](GUIA-DE-ESTUDO.md) | Explica o sistema inteiro: o caminho de um clique, back-end, banco, web, mobile, desktop, glossário e perguntas do professor |
| [ROTEIRO-DE-APRESENTACAO.md](ROTEIRO-DE-APRESENTACAO.md) | Divisão das falas e demonstração passo a passo |
| [entrega/](entrega/README.md) | Documento final em Word (ABNT), pronto para revisar e postar |

## Diagramas

| Pasta | Conteúdo | Ferramenta |
|---|---|---|
| [`diagramas/casos-de-uso`](diagramas/casos-de-uso) | Casos de uso, classes e sequência (PIM III) | Astah UML |
| [`diagramas/atividade`](diagramas/atividade/README.md) | Diagramas de atividade UC01 a UC11 | PlantUML |
| [`diagramas/banco`](diagramas/banco) | MER do PIM IV (SQL Server) e DER original do PIM III | PlantUML / dbdiagram.io |
| [`diagramas/arquitetura`](diagramas/arquitetura) | Arquitetura da solução e camadas | PlantUML |
| [`diagramas/mobile`](diagramas/mobile) | Navegação e sincronização do app | PlantUML |
| [`diagramas/nuvem`](diagramas/nuvem) | Implantação na Azure | PlantUML |

Os arquivos `.puml` são texto: edite e gere a imagem de novo em <https://www.plantuml.com/plantuml> ou com a extensão PlantUML do VS Code.

## Capturas de tela

- [`imagens/web`](imagens/web): site React (3 perfis, alto contraste, celular)
- [`imagens/mobile`](imagens/mobile): app Expo (inclusive o modo offline)
