using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

public class ConteudoRepositorioSql : IConteudoRepositorio
{
    private readonly ConexaoSql _conexao;

    public ConteudoRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<IReadOnlyList<Conteudo>> ListarAsync(bool somenteAtivos)
    {
        string sql = "SELECT * FROM conteudo"
                   + (somenteAtivos ? " WHERE ativo = 1" : "")
                   + " ORDER BY titulo";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<Conteudo>();
        while (await leitor.ReadAsync())
            lista.Add(Mapear(leitor));
        return lista;
    }

    public async Task<Conteudo?> ObterPorIdAsync(int id)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("SELECT * FROM conteudo WHERE id_conteudo = @id", conexao);
        comando.Parameters.AddWithValue("@id", id);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    public async Task<Conteudo> AdicionarAsync(Conteudo conteudo)
    {
        const string sql = @"
            INSERT INTO conteudo (titulo, descricao, categoria, nivel, duracao, aulas, ativo, id_secretaria)
            VALUES (@titulo, @descricao, @categoria, @nivel, @duracao, @aulas, @ativo, @id_secretaria);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        PreencherParametros(comando, conteudo);
        comando.Parameters.AddWithValue("@id_secretaria", conteudo.IdSecretaria);

        conteudo.Id = (int)(await comando.ExecuteScalarAsync())!;
        return conteudo;
    }

    public async Task AtualizarAsync(Conteudo conteudo)
    {
        const string sql = @"
            UPDATE conteudo
               SET titulo = @titulo, descricao = @descricao, categoria = @categoria,
                   nivel = @nivel, duracao = @duracao, aulas = @aulas, ativo = @ativo
             WHERE id_conteudo = @id";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        PreencherParametros(comando, conteudo);
        comando.Parameters.AddWithValue("@id", conteudo.Id);
        await comando.ExecuteNonQueryAsync();   // trigger trg_conteudo_desativado cuida das atividades
    }

    private static void PreencherParametros(SqlCommand comando, Conteudo c)
    {
        comando.Parameters.AddWithValue("@titulo", c.Titulo);
        comando.Parameters.AddWithValue("@descricao", LeitorExtensoes.ValorOuDbNull(c.Descricao));
        comando.Parameters.AddWithValue("@categoria", c.Categoria);
        comando.Parameters.AddWithValue("@nivel", c.Nivel);
        comando.Parameters.AddWithValue("@duracao", c.Duracao);
        comando.Parameters.AddWithValue("@aulas", c.Aulas);
        comando.Parameters.AddWithValue("@ativo", c.Ativo);
    }

    private static Conteudo Mapear(SqlDataReader leitor) => new()
    {
        Id = leitor.GetInt32(leitor.GetOrdinal("id_conteudo")),
        Titulo = leitor.GetString(leitor.GetOrdinal("titulo")),
        Descricao = leitor.TextoOuNulo("descricao"),
        Categoria = leitor.GetString(leitor.GetOrdinal("categoria")),
        Nivel = leitor.GetString(leitor.GetOrdinal("nivel")),
        Duracao = leitor.GetString(leitor.GetOrdinal("duracao")),
        Aulas = leitor.GetInt32(leitor.GetOrdinal("aulas")),
        Ativo = leitor.GetBoolean(leitor.GetOrdinal("ativo")),
        DataCriacao = leitor.GetDateTime(leitor.GetOrdinal("data_criacao")),
        IdSecretaria = leitor.GetInt32(leitor.GetOrdinal("id_secretaria"))
    };
}
