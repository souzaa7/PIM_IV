using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Gestão de usuários: cadastrar secretaria (UC10) e desativar cadastros (UC11).</summary>
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : BaseController
{
    private readonly UsuarioService _usuarios;

    public UsuariosController(UsuarioService usuarios)
    {
        _usuarios = usuarios;
    }

    /// <summary>Lista todos os usuários (administrador).</summary>
    [HttpGet]
    [Authorize(Roles = Perfis.Admin)]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> Listar() =>
        Ok(await _usuarios.ListarAsync(await UsuarioLogadoAsync()));

    /// <summary>
    /// Cadastra um usuário: o administrador cadastra SECRETARIA (UC10);
    /// a secretaria cadastra ALUNO.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Perfis.Admin + "," + Perfis.Secretaria)]
    public async Task<ActionResult<UsuarioDto>> Criar(NovoUsuarioDto dto)
    {
        var criado = await _usuarios.CriarAsync(dto, await UsuarioLogadoAsync());
        return StatusCode(StatusCodes.Status201Created, criado);
    }

    /// <summary>UC11 - Desativa ou reativa um usuário.</summary>
    [HttpPatch("{id:int}/ativo")]
    [Authorize(Roles = Perfis.Admin)]
    public async Task<IActionResult> AlterarAtivo(int id, AlterarAtivoRequisicao requisicao)
    {
        await _usuarios.AlterarAtivoAsync(id, requisicao.Ativo, await UsuarioLogadoAsync());
        return NoContent();
    }
}
