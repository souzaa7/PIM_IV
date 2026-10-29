using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Conteúdos (cursos): catálogo público e gestão pela secretaria (UC03, UC07, UC08, UC09).</summary>
[Route("api/conteudos")]
public class ConteudosController : BaseController
{
    private readonly ConteudoService _conteudos;
    private readonly AtividadeService _atividades;

    public ConteudosController(ConteudoService conteudos, AtividadeService atividades)
    {
        _conteudos = conteudos;
        _atividades = atividades;
    }

    /// <summary>Catálogo de conteúdos ativos (aberto a visitantes).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<ConteudoDto>>> ListarAtivos() =>
        Ok(await _conteudos.ListarAtivosAsync());

    /// <summary>Detalhe de um conteúdo ativo.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ConteudoDto>> Obter(int id) =>
        Ok(await _conteudos.ObterAsync(id));

    /// <summary>Atividades ativas de um conteúdo.</summary>
    [HttpGet("{id:int}/atividades")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<AtividadeDto>>> Atividades(int id) =>
        Ok(await _atividades.ListarPorConteudoAsync(id));

    /// <summary>Todos os conteúdos, inclusive inativos (secretaria).</summary>
    [HttpGet("todos")]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<ActionResult<IReadOnlyList<ConteudoDto>>> ListarTodos() =>
        Ok(await _conteudos.ListarTodosAsync(await UsuarioLogadoAsync()));

    /// <summary>UC07 - Adicionar conteúdo.</summary>
    [HttpPost]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<ActionResult<ConteudoDto>> Criar(NovoConteudoDto dto)
    {
        var criado = await _conteudos.CriarAsync(dto, await UsuarioLogadoAsync());
        return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
    }

    /// <summary>UC08 - Editar conteúdo.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<ActionResult<ConteudoDto>> Editar(int id, NovoConteudoDto dto) =>
        Ok(await _conteudos.EditarAsync(id, dto, await UsuarioLogadoAsync()));

    /// <summary>UC09 - Desativar ou reativar conteúdo.</summary>
    [HttpPatch("{id:int}/ativo")]
    [Authorize(Roles = Perfis.Secretaria)]
    public async Task<IActionResult> AlterarAtivo(int id, AlterarAtivoRequisicao requisicao)
    {
        await _conteudos.AlterarAtivoAsync(id, requisicao.Ativo, await UsuarioLogadoAsync());
        return NoContent();
    }
}
