using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

public class AtividadeRepositorioMemoria : IAtividadeRepositorio
{
    private readonly BancoEmMemoria _banco;

    public AtividadeRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<IReadOnlyList<Atividade>> ListarPorConteudoAsync(int idConteudo, bool somenteAtivas)
    {
        var lista = _banco.Atividades
            .Where(a => a.IdConteudo == idConteudo && (!somenteAtivas || a.Ativo))
            .OrderBy(a => a.DataEntrega)
            .ToList();
        return Task.FromResult<IReadOnlyList<Atividade>>(lista);
    }

    public Task<Atividade?> ObterPorIdAsync(int id) =>
        Task.FromResult(_banco.Atividades.FirstOrDefault(a => a.Id == id));

    public Task<Atividade> AdicionarAsync(Atividade atividade)
    {
        atividade.Id = _banco.ProximoId("atividade");
        _banco.Atividades.Add(atividade);
        return Task.FromResult(atividade);
    }
}
