using System.Data;
using AprenderMais.Dominio.Consultas;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

/// <summary>Relatórios: chama as procedures sp_relatorio_resumo e sp_relatorio_por_conteudo.</summary>
public class RelatorioRepositorioSql : IRelatorioRepositorio
{
    private readonly ConexaoSql _conexao;

    public RelatorioRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<ResumoPlataforma> ObterResumoAsync()
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("sp_relatorio_resumo", conexao)
        {
            CommandType = CommandType.StoredProcedure
        };
        await using var leitor = await comando.ExecuteReaderAsync();
        await leitor.ReadAsync();

        return new ResumoPlataforma(
            leitor.GetInt32(leitor.GetOrdinal("total_alunos")),
            leitor.GetInt32(leitor.GetOrdinal("total_secretarias")),
            leitor.GetInt32(leitor.GetOrdinal("usuarios_inativos")),
            leitor.GetInt32(leitor.GetOrdinal("conteudos_ativos")),
            leitor.GetInt32(leitor.GetOrdinal("matriculas_em_andamento")),
            leitor.GetInt32(leitor.GetOrdinal("matriculas_concluidas")),
            leitor.GetInt32(leitor.GetOrdinal("respostas_pendentes")),
            leitor.GetInt32(leitor.GetOrdinal("certificados_emitidos")),
            leitor.GetDecimal(leitor.GetOrdinal("media_geral")));
    }

    public async Task<IReadOnlyList<DesempenhoConteudo>> ListarDesempenhoPorConteudoAsync()
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("sp_relatorio_por_conteudo", conexao)
        {
            CommandType = CommandType.StoredProcedure
        };
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<DesempenhoConteudo>();
        while (await leitor.ReadAsync())
        {
            lista.Add(new DesempenhoConteudo(
                leitor.GetInt32(leitor.GetOrdinal("id_conteudo")),
                leitor.GetString(leitor.GetOrdinal("titulo")),
                leitor.GetBoolean(leitor.GetOrdinal("ativo")),
                leitor.GetInt32(leitor.GetOrdinal("matriculados")),
                leitor.GetInt32(leitor.GetOrdinal("concluidos")),
                leitor.GetDecimal(leitor.GetOrdinal("media_notas"))));
        }
        return lista;
    }
}
