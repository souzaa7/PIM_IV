using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

public class NotaRepositorioMemoria : INotaRepositorio
{
    private readonly BancoEmMemoria _banco;

    public NotaRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<Nota?> ObterPorRespostaAsync(int idResposta) =>
        Task.FromResult(_banco.Notas.FirstOrDefault(n => n.IdResposta == idResposta));

    public Task<Nota> AdicionarAsync(Nota nota)
    {
        nota.Id = _banco.ProximoId("nota");
        _banco.Notas.Add(nota);
        return Task.FromResult(nota);
    }
}
