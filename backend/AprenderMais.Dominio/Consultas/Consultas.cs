namespace AprenderMais.Dominio.Consultas;

// Resultados de consultas (somente leitura). Não são entidades:
// juntam dados de várias tabelas para exibir em tela.

/// <summary>Resposta que ainda não recebeu nota (fila de correção da secretaria).</summary>
public record RespostaPendente(
    int IdResposta,
    string NomeAluno,
    string TituloAtividade,
    string TituloConteudo,
    decimal NotaMaxima,
    string TextoResposta,
    string? ArquivoUrl,
    DateTime DataEnvio);

/// <summary>Indicadores gerais do painel do administrador.</summary>
public record ResumoPlataforma(
    int TotalAlunos,
    int TotalSecretarias,
    int UsuariosInativos,
    int ConteudosAtivos,
    int MatriculasEmAndamento,
    int MatriculasConcluidas,
    int RespostasPendentes,
    int CertificadosEmitidos,
    decimal MediaGeral);

/// <summary>Desempenho de um conteúdo (relatório do administrador).</summary>
public record DesempenhoConteudo(
    int IdConteudo,
    string Titulo,
    bool Ativo,
    int Matriculados,
    int Concluidos,
    decimal MediaNotas);
