using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

public class MatriculaRepositorioMemoria : IMatriculaRepositorio
{
    private readonly BancoEmMemoria _banco;

    public MatriculaRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<Matricula?> ObterPorIdAsync(int id) =>
        Task.FromResult(_banco.Matriculas.FirstOrDefault(m => m.Id == id));

    public Task<Matricula?> ObterPorAlunoEConteudoAsync(int idAluno, int idConteudo) =>
        Task.FromResult(_banco.Matriculas.FirstOrDefault(m =>
            m.IdAluno == idAluno && m.IdConteudo == idConteudo));

    public Task<IReadOnlyList<Matricula>> ListarPorAlunoAsync(int idAluno)
    {
        var lista = _banco.Matriculas
            .Where(m => m.IdAluno == idAluno)
            .OrderBy(m => m.DataMatricula)
            .ToList();
        return Task.FromResult<IReadOnlyList<Matricula>>(lista);
    }

    public Task<Matricula> AdicionarAsync(Matricula matricula)
    {
        // Mesma restrição UNIQUE (id_aluno, id_conteudo) do banco.
        if (_banco.Matriculas.Any(m => m.IdAluno == matricula.IdAluno && m.IdConteudo == matricula.IdConteudo))
            throw new DominioException("O aluno já possui matrícula neste conteúdo.");

        matricula.Id = _banco.ProximoId("matricula");
        _banco.Matriculas.Add(matricula);
        return Task.FromResult(matricula);
    }

    // Os objetos já são os mesmos da lista; um banco real gravaria as alterações aqui.
    public Task AtualizarAsync(Matricula matricula) => Task.CompletedTask;

    /// <summary>Mesma conta da procedure sp_calcular_progresso.</summary>
    public Task<(int Avaliadas, int Total)> ContarAtividadesAsync(int idAluno, int idConteudo)
    {
        var atividades = _banco.Atividades
            .Where(a => a.IdConteudo == idConteudo && a.Ativo)
            .Select(a => a.Id)
            .ToList();

        int avaliadas = _banco.Respostas
            .Where(r => r.IdAluno == idAluno && atividades.Contains(r.IdAtividade))
            .Count(r => _banco.Notas.Any(n => n.IdResposta == r.Id));

        return Task.FromResult((avaliadas, atividades.Count));
    }
}
