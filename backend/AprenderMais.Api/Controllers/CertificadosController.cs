using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Certificados de conclusão.</summary>
[Route("api/certificados")]
public class CertificadosController : BaseController
{
    private readonly CertificadoService _certificados;

    public CertificadosController(CertificadoService certificados)
    {
        _certificados = certificados;
    }

    /// <summary>Certificados do aluno logado.</summary>
    [HttpGet("meus")]
    [Authorize(Roles = Perfis.Aluno)]
    public async Task<ActionResult<IReadOnlyList<CertificadoDto>>> Meus() =>
        Ok(await _certificados.ListarDoAlunoAsync(await UsuarioLogadoAsync()));

    /// <summary>Validação pública de um certificado pelo código (ex.: AM-2026-7F3A9C21).</summary>
    [HttpGet("validar/{codigo}")]
    [AllowAnonymous]
    public async Task<ActionResult<CertificadoDto>> Validar(string codigo) =>
        Ok(await _certificados.ValidarAsync(codigo));
}
