using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>Certificados de conclusão: o aluno consulta os seus e qualquer pessoa valida pelo código.</summary>
public class CertificadoService
{
    private readonly ICertificadoRepositorio _certificados;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IConteudoRepositorio _conteudos;
    private readonly IRelogio _relogio;

    public CertificadoService(
        ICertificadoRepositorio certificados,
        IUsuarioRepositorio usuarios,
        IConteudoRepositorio conteudos,
        IRelogio relogio)
    {
        _certificados = certificados;
        _usuarios = usuarios;
        _conteudos = conteudos;
        _relogio = relogio;
    }

    public async Task<IReadOnlyList<CertificadoDto>> ListarDoAlunoAsync(Usuario solicitante)
    {
        var aluno = Permissoes.ExigirAluno(solicitante);
        var resultado = new List<CertificadoDto>();

        foreach (var certificado in await _certificados.ListarPorAlunoAsync(aluno.IdAluno))
            resultado.Add(await ParaDtoAsync(certificado, aluno.Nome));

        return resultado;
    }

    /// <summary>Validação pública: uma empresa confere se o certificado é verdadeiro.</summary>
    public async Task<CertificadoDto> ValidarAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DominioException("Informe o código do certificado.");

        var certificado = await _certificados.ObterPorCodigoAsync(codigo.Trim().ToUpperInvariant())
            ?? throw new DominioException("Certificado não encontrado. Confira o código digitado.");

        var aluno = await _usuarios.ObterPorIdAlunoAsync(certificado.IdAluno);
        return await ParaDtoAsync(certificado, aluno?.Nome ?? "-");
    }

    private async Task<CertificadoDto> ParaDtoAsync(Certificado c, string nomeAluno)
    {
        var conteudo = await _conteudos.ObterPorIdAsync(c.IdConteudo);
        return new CertificadoDto(
            c.Id,
            nomeAluno,
            conteudo?.Titulo ?? "-",
            c.CodigoValidacao,
            _relogio.ParaBrasilia(c.DataEmissao).ToString("dd/MM/yyyy"));
    }
}
