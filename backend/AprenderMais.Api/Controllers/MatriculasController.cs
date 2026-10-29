using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Matrículas do aluno nos conteúdos.</summary>
[Route("api/matriculas")]
[Authorize(Roles = Perfis.Aluno)]
public class MatriculasController : BaseController
{
    private readonly MatriculaService _matriculas;

    public MatriculasController(MatriculaService matriculas)
    {
        _matriculas = matriculas;
    }

    /// <summary>Matrículas do aluno logado, com progresso.</summary>
    [HttpGet("minhas")]
    public async Task<ActionResult<IReadOnlyList<MatriculaDto>>> Minhas() =>
        Ok(await _matriculas.ListarDoAlunoAsync(await UsuarioLogadoAsync()));

    /// <summary>Matricula o aluno em um conteúdo (ou reativa uma matrícula cancelada).</summary>
    [HttpPost]
    public async Task<ActionResult<MatriculaDto>> Matricular(MatricularRequisicao requisicao)
    {
        var matricula = await _matriculas.MatricularAsync(requisicao.IdConteudo, await UsuarioLogadoAsync());
        return StatusCode(StatusCodes.Status201Created, matricula);
    }

    /// <summary>Cancela uma matrícula em andamento.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _matriculas.CancelarAsync(id, await UsuarioLogadoAsync());
        return NoContent();
    }
}
