namespace AprenderMais.Aplicacao.Dtos;

// Objetos de transferência: o que a interface (Web, Mobile, Desktop) enxerga.
// As telas nunca manipulam as entidades do domínio diretamente.

// ------------------------------------------------------------------ Conteúdos
public record ConteudoDto(
    int Id,
    string Titulo,
    string? Descricao,
    string Categoria,
    string Nivel,
    string Duracao,
    int Aulas,
    bool Ativo)
{
    public string Situacao => Ativo ? "Ativo" : "Inativo";
}

public record NovoConteudoDto(
    string Titulo,
    string? Descricao,
    string Categoria,
    string Nivel,
    string Duracao,
    int Aulas);

// ------------------------------------------------------------------ Matrículas
public record MatriculaDto(
    int IdMatricula,
    int IdConteudo,
    string Conteudo,
    string Status,
    decimal Progresso,
    string DataMatricula,
    bool PodeCancelar)
{
    public string ProgressoTexto => $"{Progresso:0.##}%";
}

// ------------------------------------------------------------------ Usuários
public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo)
{
    public string Situacao => Ativo ? "Ativo" : "Inativo";
}

public record NovoUsuarioDto(
    string Cpf,
    string Nome,
    string Email,
    string Senha);

// ------------------------------------------------------------------ Atividades
public record AtividadeDto(
    int Id,
    int IdConteudo,
    string Titulo,
    string? Descricao,
    string DataEntrega,
    decimal NotaMaxima,
    bool PrazoEncerrado);

public record NovaAtividadeDto(
    int IdConteudo,
    string Titulo,
    string? Descricao,
    DateTime DataEntrega,
    decimal NotaMaxima);

/// <summary>Atividade vista pelo aluno, com a situação da entrega dele.</summary>
public record AtividadeDoAlunoDto(
    int IdAtividade,
    int IdConteudo,
    string Conteudo,
    string Titulo,
    string? Descricao,
    string DataEntrega,
    decimal NotaMaxima,
    string Situacao,          // Pendente, Prazo encerrado, Aguardando correção, Avaliada
    bool PodeEnviar,
    decimal? Nota,
    string? Feedback);

// ------------------------------------------------------------------ Respostas e notas
public record EnviarRespostaDto(
    int IdAtividade,
    string Texto,
    string? ArquivoUrl);

public record RespostaPendenteDto(
    int IdResposta,
    string Aluno,
    string Atividade,
    string Conteudo,
    decimal NotaMaxima,
    string Texto,
    string? ArquivoUrl,
    string DataEnvio);

public record AvaliarRespostaDto(
    decimal Valor,
    string? Feedback);

public record NotaDto(
    string Conteudo,
    string Atividade,
    decimal Valor,
    decimal NotaMaxima,
    string? Feedback,
    string DataAvaliacao);

// ------------------------------------------------------------------ Certificados e relatórios
public record CertificadoDto(
    int Id,
    string Aluno,
    string Conteudo,
    string CodigoValidacao,
    string DataEmissao);

public record ResumoDto(
    int TotalAlunos,
    int TotalSecretarias,
    int UsuariosInativos,
    int ConteudosAtivos,
    int MatriculasEmAndamento,
    int MatriculasConcluidas,
    int RespostasPendentes,
    int CertificadosEmitidos,
    decimal MediaGeral);

public record DesempenhoConteudoDto(
    int IdConteudo,
    string Titulo,
    string Situacao,
    int Matriculados,
    int Concluidos,
    decimal MediaNotas);

/// <summary>Resultado da correção: como ficou a matrícula do aluno.</summary>
public record AvaliacaoResultadoDto(
    decimal ProgressoDoAluno,
    bool ConteudoConcluido,
    string? CodigoCertificado);
