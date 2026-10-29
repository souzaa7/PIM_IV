using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Respostas das atividades: envio pelo aluno (UC04) e correção pela secretaria (UC06).</summary>
[Route("api/respostas")]
[Authorize]
public class RespostasController : BaseController
{
    private readonly RespostaService _respostas;

    public RespostasController(RespostaService respostas)
    {
        _respostas = respostas;
    }

    /// <summary>UC04 - Aluno envia a resposta de uma atividade.</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.Aluno)]
    public async Task<IActionResult> Enviar(EnviarRespostaDto dto)
    {
        await _respostas.EnviarAsync(dto, await UsuarioLogadoAsync());
        return StatusCode(StatusCodes.Status201Created, new { mensagem = "Resposta enviada com sucesso." });
    }

    /// <summary>UC06 - Respostas aguardando correção.</summary>
    [HttpGet("pendentes")]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<ActionResult<IReadOnlyList<RespostaPendenteDto>>> Pendentes() =>
        Ok(await _respostas.ListarPendentesAsync(await UsuarioLogadoAsync()));

    /// <summary>UC06 - Secretaria dá a nota (e o sistema recalcula o progresso do aluno).</summary>
    [HttpPost("{id:int}/nota")]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<ActionResult<AvaliacaoResultadoDto>> Avaliar(int id, AvaliarRespostaDto dto) =>
        Ok(await _respostas.AvaliarAsync(id, dto, await UsuarioLogadoAsync()));
}
