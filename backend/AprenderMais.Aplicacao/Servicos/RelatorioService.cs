using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>Relatórios do painel administrativo.</summary>
public class RelatorioService
{
    private readonly IRelatorioRepositorio _relatorios;

    public RelatorioService(IRelatorioRepositorio relatorios)
    {
        _relatorios = relatorios;
    }

    public async Task<ResumoDto> ResumoAsync(Usuario solicitante)
    {
        Permissoes.ExigirAdmin(solicitante);
        var r = await _relatorios.ObterResumoAsync();

        return new ResumoDto(
            r.TotalAlunos, r.TotalSecretarias, r.UsuariosInativos, r.ConteudosAtivos,
            r.MatriculasEmAndamento, r.MatriculasConcluidas, r.RespostasPendentes,
            r.CertificadosEmitidos, r.MediaGeral);
    }

    public async Task<IReadOnlyList<DesempenhoConteudoDto>> DesempenhoPorConteudoAsync(Usuario solicitante)
    {
        Permissoes.ExigirAdmin(solicitante);
        var lista = await _relatorios.ListarDesempenhoPorConteudoAsync();

        return lista
            .Select(d => new DesempenhoConteudoDto(
                d.IdConteudo, d.Titulo, d.Ativo ? "Ativo" : "Inativo",
                d.Matriculados, d.Concluidos, d.MediaNotas))
            .ToList();
    }
}
