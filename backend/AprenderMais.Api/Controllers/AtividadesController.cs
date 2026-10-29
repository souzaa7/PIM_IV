using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Atividades: criação pela secretaria e lista do aluno (UC03 e UC04).</summary>
[Route("api/atividades")]
[Authorize]
public class AtividadesController : BaseController
{
    private readonly AtividadeService _atividades;

    public AtividadesController(AtividadeService atividades)
    {
        _atividades = atividades;
    }

    /// <summary>Atividades dos conteúdos em que o aluno está matriculado, com a situação de cada uma.</summary>
    [HttpGet("minhas")]
    [Authorize(Roles = Perfis.Aluno)]
    public async Task<ActionResult<IReadOnlyList<AtividadeDoAlunoDto>>> Minhas() =>
        Ok(await _atividades.ListarDoAlunoAsync(await UsuarioLogadoAsync()));

    /// <summary>Secretaria cria uma atividade para um conteúdo.</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<ActionResult<AtividadeDto>> Criar(NovaAtividadeDto dto)
    {
        var criada = await _atividades.CriarAsync(dto, await UsuarioLogadoAsync());
        return StatusCode(StatusCodes.Status201Created, criada);
    }
}
