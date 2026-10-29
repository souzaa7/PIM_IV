using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Pacote com todos os dados do aluno de uma vez (usado pelo app mobile para funcionar offline).</summary>
public record PacoteSincronizacao(
    UsuarioDto Usuario,
    IReadOnlyList<ConteudoDto> Conteudos,
    IReadOnlyList<MatriculaDto> Matriculas,
    IReadOnlyList<AtividadeDoAlunoDto> Atividades,
    IReadOnlyList<NotaDto> Notas,
    IReadOnlyList<CertificadoDto> Certificados,
    DateTime DataSincronizacao);

[Route("api/sincronizacao")]
[Authorize(Roles = Perfis.Aluno)]
public class SincronizacaoController : BaseController
{
    private readonly ConteudoService _conteudos;
    private readonly MatriculaService _matriculas;
    private readonly AtividadeService _atividades;
    private readonly NotaService _notas;
    private readonly CertificadoService _certificados;

    public SincronizacaoController(
        ConteudoService conteudos,
        MatriculaService matriculas,
        AtividadeService atividades,
        NotaService notas,
        CertificadoService certificados)
    {
        _conteudos = conteudos;
        _matriculas = matriculas;
        _atividades = atividades;
        _notas = notas;
        _certificados = certificados;
    }

    /// <summary>Devolve tudo que o app precisa guardar no celular.</summary>
    [HttpGet]
    public async Task<ActionResult<PacoteSincronizacao>> Baixar()
    {
        var aluno = await UsuarioLogadoAsync();

        return new PacoteSincronizacao(
            UsuarioService.ParaDto(aluno),
            await _conteudos.ListarAtivosAsync(),
            await _matriculas.ListarDoAlunoAsync(aluno),
            await _atividades.ListarDoAlunoAsync(aluno),
            await _notas.ListarDoAlunoAsync(aluno),
            await _certificados.ListarDoAlunoAsync(aluno),
            DateTime.UtcNow);
    }
}
