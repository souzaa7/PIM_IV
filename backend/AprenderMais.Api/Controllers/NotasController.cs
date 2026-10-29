using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>UC05 - Acessar notas.</summary>
[Route("api/notas")]
[Authorize(Roles = Perfis.Aluno)]
public class NotasController : BaseController
{
    private readonly NotaService _notas;

    public NotasController(NotaService notas)
    {
        _notas = notas;
    }

    /// <summary>Boletim do aluno logado.</summary>
    [HttpGet("minhas")]
    public async Task<ActionResult<IReadOnlyList<NotaDto>>> Minhas() =>
        Ok(await _notas.ListarDoAlunoAsync(await UsuarioLogadoAsync()));
}
