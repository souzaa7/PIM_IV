using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

public class AtividadeRepositorioSql : IAtividadeRepositorio
{
    private readonly ConexaoSql _conexao;

    public AtividadeRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<IReadOnlyList<Atividade>> ListarPorConteudoAsync(int idConteudo, bool somenteAtivas)
    {
        string sql = "SELECT * FROM atividade WHERE id_conteudo = @id"
                   + (somenteAtivas ? " AND ativo = 1" : "")
                   + " ORDER BY data_entrega";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@id", idConteudo);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<Atividade>();
        while (await leitor.ReadAsync())
            lista.Add(Mapear(leitor));
        return lista;
    }

    public async Task<Atividade?> ObterPorIdAsync(int id)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("SELECT * FROM atividade WHERE id_atividade = @id", conexao);
        comando.Parameters.AddWithValue("@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<Atividade> AdicionarAsync(Atividade atividade)
    {
        const string sql = @"
            INSERT INTO atividade (titulo, descricao, data_entrega, nota_maxima, id_conteudo)
            VALUES (@titulo, @descricao, @data_entrega, @nota_maxima, @id_conteudo);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@titulo", atividade.Titulo);
        comando.Parameters.AddWithValue("@descricao", LeitorExtensoes.ValorOuDbNull(atividade.Descricao));
        comando.Parameters.AddWithValue("@data_entrega", atividade.DataEntrega);
        comando.Parameters.AddWithValue("@nota_maxima", atividade.NotaMaxima);
        comando.Parameters.AddWithValue("@id_conteudo", atividade.IdConteudo);

        atividade.Id = (int)(await comando.ExecuteScalarAsync())!;
        return atividade;
    }

    private static Atividade Mapear(SqlDataReader leitor) => new()
    {
        Id = leitor.GetInt32(leitor.GetOrdinal("id_atividade")),
        Titulo = leitor.GetString(leitor.GetOrdinal("titulo")),
        Descricao = leitor.TextoOuNulo("descricao"),
        DataEntrega = leitor.GetDateTime(leitor.GetOrdinal("data_entrega")),
        NotaMaxima = leitor.GetDecimal(leitor.GetOrdinal("nota_maxima")),
        Ativo = leitor.GetBoolean(leitor.GetOrdinal("ativo")),
        DataCriacao = leitor.GetDateTime(leitor.GetOrdinal("data_criacao")),
        IdConteudo = leitor.GetInt32(leitor.GetOrdinal("id_conteudo"))
    };
}
