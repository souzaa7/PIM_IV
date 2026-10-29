using System.Security.Claims;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Autenticação: login, cadastro do aluno e dados do usuário logado (UC01 e UC02).</summary>
[Route("api/auth")]
public class AuthController : BaseController
{
    private readonly AutenticacaoService _autenticacao;
    private readonly UsuarioService _usuarios;

    public AuthController(AutenticacaoService autenticacao, UsuarioService usuarios)
    {
        _autenticacao = autenticacao;
        _usuarios = usuarios;
    }

    /// <summary>
    /// UC01 - Login. Devolve um token (accessToken) que deve ser enviado nas próximas
    /// requisições no cabeçalho: Authorization: Bearer {accessToken}
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequisicao requisicao)
    {
        var usuario = await _autenticacao.EntrarAsync(requisicao.Email, requisicao.Senha);

        // "Claims" são as informações gravadas dentro do token.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil.ToString())   // Aluno, Secretaria ou Admin
        };
        var identidade = new ClaimsIdentity(claims, BearerTokenDefaults.AuthenticationScheme);

        // O próprio ASP.NET Core gera e criptografa o token e responde:
        // { tokenType, accessToken, expiresIn, refreshToken }
        return SignIn(new ClaimsPrincipal(identidade), BearerTokenDefaults.AuthenticationScheme);
    }

    /// <summary>UC02 - Cadastro público: o visitante cria a própria conta de aluno.</summary>
    [HttpPost("cadastro")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioDto>> Cadastrar(NovoUsuarioDto dto)
    {
        var criado = await _usuarios.CadastrarAlunoAsync(dto);
        return StatusCode(StatusCodes.Status201Created, criado);
    }

    /// <summary>Dados do usuário dono do token (usado pelo front depois do login).</summary>
    [HttpGet("eu")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> Eu()
    {
        var usuario = await UsuarioLogadoAsync();
        return UsuarioService.ParaDto(usuario);
    }
}
