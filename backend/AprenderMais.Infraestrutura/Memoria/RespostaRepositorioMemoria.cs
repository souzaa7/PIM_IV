using AprenderMais.Dominio.Consultas;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

public class RespostaRepositorioMemoria : IRespostaRepositorio
{
    private readonly BancoEmMemoria _banco;

    public RespostaRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<Resposta?> ObterPorIdAsync(int id) =>
        Task.FromResult(_banco.Respostas.FirstOrDefault(r => r.Id == id));

    public Task<Resposta?> ObterPorAlunoEAtividadeAsync(int idAluno, int idAtividade) =>
        Task.FromResult(_banco.Respostas.FirstOrDefault(r => r.IdAluno == idAluno && r.IdAtividade == idAtividade));

    public Task<IReadOnlyList<Resposta>> ListarPorAlunoAsync(int idAluno) =>
        Task.FromResult<IReadOnlyList<Resposta>>(_banco.Respostas.Where(r => r.IdAluno == idAluno).ToList());

    /// <summary>Equivalente à view vw_respostas_pendentes.</summary>
    public Task<IReadOnlyList<RespostaPendente>> ListarPendentesAsync()
    {
        var lista =
            from r in _banco.Respostas
            where !_banco.Notas.Any(n => n.IdResposta == r.Id)
            join at in _banco.Atividades on r.IdAtividade equals at.Id
            join c in _banco.Conteudos on at.IdConteudo equals c.Id
            join a in _banco.Usuarios.OfType<Aluno>() on r.IdAluno equals a.IdAluno
            select new RespostaPendente(r.Id, a.Nome, at.Titulo, c.Titulo, at.NotaMaxima,
                                        r.TextoResposta, r.ArquivoUrl, r.DataEnvio);

        return Task.FromResult<IReadOnlyList<RespostaPendente>>(lista.ToList());
    }

    public Task<Resposta> AdicionarAsync(Resposta resposta)
    {
        // Mesma restrição UNIQUE (id_aluno, id_atividade) do banco.
        if (_banco.Respostas.Any(r => r.IdAluno == resposta.IdAluno && r.IdAtividade == resposta.IdAtividade))
            throw new DominioException("Você já enviou a resposta desta atividade.");

        resposta.Id = _banco.ProximoId("resposta");
        _banco.Respostas.Add(resposta);
        return Task.FromResult(resposta);
    }
}
