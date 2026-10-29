using AprenderMais.Dominio.Consultas;
using AprenderMais.Dominio.Entidades;

namespace AprenderMais.Dominio.Repositorios;

// Contratos de acesso a dados. O Domínio define O QUE precisa;
// a Infraestrutura decide COMO (em memória ou SQL Server).

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorIdAlunoAsync(int idAluno);
    Task<IReadOnlyList<Usuario>> ListarAsync();
    Task<Usuario> AdicionarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
}

public interface IConteudoRepositorio
{
    Task<IReadOnlyList<Conteudo>> ListarAsync(bool somenteAtivos);
    Task<Conteudo?> ObterPorIdAsync(int id);
    Task<Conteudo> AdicionarAsync(Conteudo conteudo);
    Task AtualizarAsync(Conteudo conteudo);
}

public interface IMatriculaRepositorio
{
    Task<Matricula?> ObterPorIdAsync(int id);
    Task<Matricula?> ObterPorAlunoEConteudoAsync(int idAluno, int idConteudo);
    Task<IReadOnlyList<Matricula>> ListarPorAlunoAsync(int idAluno);
    Task<Matricula> AdicionarAsync(Matricula matricula);
    Task AtualizarAsync(Matricula matricula);

    /// <summary>Atividades ativas do conteúdo já avaliadas para o aluno e o total de atividades ativas.</summary>
    Task<(int Avaliadas, int Total)> ContarAtividadesAsync(int idAluno, int idConteudo);
}

public interface IAtividadeRepositorio
{
    Task<IReadOnlyList<Atividade>> ListarPorConteudoAsync(int idConteudo, bool somenteAtivas);
    Task<Atividade?> ObterPorIdAsync(int id);
    Task<Atividade> AdicionarAsync(Atividade atividade);
}

public interface IRespostaRepositorio
{
    Task<Resposta?> ObterPorIdAsync(int id);
    Task<Resposta?> ObterPorAlunoEAtividadeAsync(int idAluno, int idAtividade);
    Task<IReadOnlyList<Resposta>> ListarPorAlunoAsync(int idAluno);
    Task<IReadOnlyList<RespostaPendente>> ListarPendentesAsync();
    Task<Resposta> AdicionarAsync(Resposta resposta);
}

public interface INotaRepositorio
{
    Task<Nota?> ObterPorRespostaAsync(int idResposta);
    Task<Nota> AdicionarAsync(Nota nota);
}

public interface ICertificadoRepositorio
{
    Task<IReadOnlyList<Certificado>> ListarPorAlunoAsync(int idAluno);
    Task<Certificado?> ObterPorCodigoAsync(string codigo);

    /// <summary>Emite o certificado; se já existir, devolve o existente.</summary>
    Task<Certificado> EmitirAsync(int idAluno, int idConteudo);
}

public interface IRelatorioRepositorio
{
    Task<ResumoPlataforma> ObterResumoAsync();
    Task<IReadOnlyList<DesempenhoConteudo>> ListarDesempenhoPorConteudoAsync();
}
