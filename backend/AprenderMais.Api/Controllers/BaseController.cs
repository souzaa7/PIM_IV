using System.Security.Claims;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Base de todos os controllers: rota /api/... e acesso ao usuário logado.</summary>
[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>Busca no banco o usuário dono do token enviado no cabeçalho Authorization.</summary>
    protected async Task<Usuario> UsuarioLogadoAsync()
    {
        string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out int idUsuario))
            throw new UnauthorizedAccessException("Faça login para continuar.");

        var usuarios = HttpContext.RequestServices.GetRequiredService<IUsuarioRepositorio>();
        var usuario = await usuarios.ObterPorIdAsync(idUsuario);

        // Se o ADM desativou o usuário, o token antigo deixa de valer.
        if (usuario is null || !usuario.Ativo)
            throw new UnauthorizedAccessException("Sessão inválida. Entre novamente.");

        return usuario;
    }
}
