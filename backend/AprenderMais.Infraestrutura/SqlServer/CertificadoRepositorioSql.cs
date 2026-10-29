using System.Data;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

public class CertificadoRepositorioSql : ICertificadoRepositorio
{
    private readonly ConexaoSql _conexao;

    public CertificadoRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<IReadOnlyList<Certificado>> ListarPorAlunoAsync(int idAluno)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(
            "SELECT * FROM certificado WHERE id_aluno = @id ORDER BY data_emissao", conexao);
        comando.Parameters.AddWithValue("@id", idAluno);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<Certificado>();
        while (await leitor.ReadAsync())
            lista.Add(Mapear(leitor));
        return lista;
    }

    public async Task<Certificado?> ObterPorCodigoAsync(string codigo)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(
            "SELECT * FROM certificado WHERE codigo_validacao = @codigo", conexao);
        comando.Parameters.AddWithValue("@codigo", codigo);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    /// <summary>Chama a procedure sp_emitir_certificado.</summary>
    public async Task<Certificado> EmitirAsync(int idAluno, int idConteudo)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("sp_emitir_certificado", conexao)
        {
            CommandType = CommandType.StoredProcedure
        };
        comando.Parameters.AddWithValue("@id_aluno", idAluno);
        comando.Parameters.AddWithValue("@id_conteudo", idConteudo);
        await using var leitor = await comando.ExecuteReaderAsync();

        await leitor.ReadAsync();
        return Mapear(leitor);
    }

    private static Certificado Mapear(SqlDataReader leitor) => new()
    {
        Id = leitor.GetInt32(leitor.GetOrdinal("id_certificado")),
        IdAluno = leitor.GetInt32(leitor.GetOrdinal("id_aluno")),
        IdConteudo = leitor.GetInt32(leitor.GetOrdinal("id_conteudo")),
        CodigoValidacao = leitor.GetString(leitor.GetOrdinal("codigo_validacao")),
        DataEmissao = leitor.GetDateTime(leitor.GetOrdinal("data_emissao"))
    };
}
