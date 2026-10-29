using AprenderMais.Api.Configuracao;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AprenderMais.Api.Controllers;

/// <summary>Relatórios do painel do administrador.</summary>
[Route("api/relatorios")]
[Authorize(Roles = Perfis.Admin)]
public class RelatoriosController : BaseController
{
    private readonly RelatorioService _relatorios;

    public RelatoriosController(RelatorioService relatorios)
    {
        _relatorios = relatorios;
    }

    /// <summary>Indicadores gerais da plataforma.</summary>
    [HttpGet("resumo")]
    public async Task<ActionResult<ResumoDto>> Resumo() =>
        Ok(await _relatorios.ResumoAsync(await UsuarioLogadoAsync()));

    /// <summary>Matriculados, concluídos e média de notas por conteúdo.</summary>
    [HttpGet("conteudos")]
    public async Task<ActionResult<IReadOnlyList<DesempenhoConteudoDto>>> PorConteudo() =>
        Ok(await _relatorios.DesempenhoPorConteudoAsync(await UsuarioLogadoAsync()));
}
