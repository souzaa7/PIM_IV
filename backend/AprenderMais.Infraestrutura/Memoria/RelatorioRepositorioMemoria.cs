using AprenderMais.Dominio.Consultas;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Infraestrutura.Memoria;

/// <summary>Mesmos cálculos das procedures sp_relatorio_resumo e sp_relatorio_por_conteudo.</summary>
public class RelatorioRepositorioMemoria : IRelatorioRepositorio
{
    private readonly BancoEmMemoria _banco;

    public RelatorioRepositorioMemoria(BancoEmMemoria banco)
    {
        _banco = banco;
    }

    public Task<ResumoPlataforma> ObterResumoAsync()
    {
        var resumo = new ResumoPlataforma(
            TotalAlunos: _banco.Usuarios.OfType<Aluno>().Count(),
            TotalSecretarias: _banco.Usuarios.OfType<Secretaria>().Count(),
            UsuariosInativos: _banco.Usuarios.Count(u => !u.Ativo),
            ConteudosAtivos: _banco.Conteudos.Count(c => c.Ativo),
            MatriculasEmAndamento: _banco.Matriculas.Count(m => m.Status == StatusMatricula.EmAndamento),
            MatriculasConcluidas: _banco.Matriculas.Count(m => m.Status == StatusMatricula.Concluido),
            RespostasPendentes: _banco.Respostas.Count(r => !_banco.Notas.Any(n => n.IdResposta == r.Id)),
            CertificadosEmitidos: _banco.Certificados.Count,
            MediaGeral: _banco.Notas.Count == 0 ? 0 : Math.Round(_banco.Notas.Average(n => n.Valor), 2));

        return Task.FromResult(resumo);
    }

    public Task<IReadOnlyList<DesempenhoConteudo>> ListarDesempenhoPorConteudoAsync()
    {
        var lista = _banco.Conteudos
            .OrderBy(c => c.Titulo)
            .Select(c =>
            {
                var notas = (from n in _banco.Notas
                             join r in _banco.Respostas on n.IdResposta equals r.Id
                             join a in _banco.Atividades on r.IdAtividade equals a.Id
                             where a.IdConteudo == c.Id
                             select n.Valor).ToList();

                return new DesempenhoConteudo(
                    c.Id,
                    c.Titulo,
                    c.Ativo,
                    _banco.Matriculas.Count(m => m.IdConteudo == c.Id && m.Status != StatusMatricula.Cancelado),
                    _banco.Matriculas.Count(m => m.IdConteudo == c.Id && m.Status == StatusMatricula.Concluido),
                    notas.Count == 0 ? 0 : Math.Round(notas.Average(), 2));
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<DesempenhoConteudo>>(lista);
    }
}
