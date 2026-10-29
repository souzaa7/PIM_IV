using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

public class ConteudoRepositorioMemoria : IConteudoRepositorio
{
    private readonly BancoEmMemoria _banco;

    public ConteudoRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<IReadOnlyList<Conteudo>> ListarAsync(bool somenteAtivos)
    {
        var lista = _banco.Conteudos
            .Where(c => !somenteAtivos || c.Ativo)
            .OrderBy(c => c.Titulo)
            .ToList();
        return Task.FromResult<IReadOnlyList<Conteudo>>(lista);
    }

    public Task<Conteudo?> ObterPorIdAsync(int id) =>
        Task.FromResult(_banco.Conteudos.FirstOrDefault(c => c.Id == id));

    public Task<Conteudo> AdicionarAsync(Conteudo conteudo)
    {
        conteudo.Id = _banco.ProximoId("conteudo");
        _banco.Conteudos.Add(conteudo);
        return Task.FromResult(conteudo);
    }

    public Task AtualizarAsync(Conteudo conteudo)
    {
        // Mesmo comportamento do trigger trg_conteudo_desativado:
        // conteúdo desativado desativa também as suas atividades.
        if (!conteudo.Ativo)
        {
            foreach (var atividade in _banco.Atividades.Where(a => a.IdConteudo == conteudo.Id))
                atividade.Ativo = false;
        }
        return Task.CompletedTask;
    }
}
