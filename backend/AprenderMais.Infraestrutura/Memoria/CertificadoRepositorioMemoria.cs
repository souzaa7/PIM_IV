using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

public class CertificadoRepositorioMemoria : ICertificadoRepositorio
{
    private readonly BancoEmMemoria _banco;

    public CertificadoRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<IReadOnlyList<Certificado>> ListarPorAlunoAsync(int idAluno) =>
        Task.FromResult<IReadOnlyList<Certificado>>(_banco.Certificados.Where(c => c.IdAluno == idAluno).ToList());

    public Task<Certificado?> ObterPorCodigoAsync(string codigo) =>
        Task.FromResult(_banco.Certificados.FirstOrDefault(c =>
            string.Equals(c.CodigoValidacao, codigo, StringComparison.OrdinalIgnoreCase)));

    /// <summary>Mesmo comportamento da procedure sp_emitir_certificado.</summary>
    public Task<Certificado> EmitirAsync(int idAluno, int idConteudo)
    {
        var existente = _banco.Certificados.FirstOrDefault(c => c.IdAluno == idAluno && c.IdConteudo == idConteudo);
        if (existente is not null)
            return Task.FromResult(existente);

        var novo = new Certificado
        {
            Id = _banco.ProximoId("certificado"),
            IdAluno = idAluno,
            IdConteudo = idConteudo,
            CodigoValidacao = $"AM-{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            DataEmissao = DateTime.UtcNow
        };
        _banco.Certificados.Add(novo);
        return Task.FromResult(novo);
    }
}
