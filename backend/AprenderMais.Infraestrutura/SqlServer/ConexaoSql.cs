using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

/// <summary>
/// Guarda a connection string e cria conexões com o SQL Server.
/// Cada repositório abre a conexão, executa o comando e fecha (using).
/// </summary>
public class ConexaoSql
{
    private readonly string _connectionString;

    public ConexaoSql(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A connection string do SQL Server não foi configurada.");
        _connectionString = connectionString;
    }

    public async Task<SqlConnection> AbrirAsync()
    {
        var conexao = new SqlConnection(_connectionString);
        await conexao.OpenAsync();
        return conexao;
    }
}

/// <summary>Pequenos atalhos para ler colunas que podem ser NULL.</summary>
internal static class LeitorExtensoes
{
    public static string? TextoOuNulo(this SqlDataReader leitor, string coluna)
    {
        int i = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(i) ? null : leitor.GetString(i);
    }

    public static int? InteiroOuNulo(this SqlDataReader leitor, string coluna)
    {
        int i = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(i) ? null : leitor.GetInt32(i);
    }

    public static DateTime? DataOuNulo(this SqlDataReader leitor, string coluna)
    {
        int i = leitor.GetOrdinal(coluna);
        return leitor.IsDBNull(i) ? null : leitor.GetDateTime(i);
    }

    /// <summary>
    /// Mensagem escrita no THROW da procedure/trigger. O SQL Server pode anexar
    /// outras mensagens técnicas depois dela; por isso pegamos só a primeira.
    /// </summary>
    public static string MensagemDoBanco(this SqlException ex) =>
        ex.Errors.Count > 0 ? ex.Errors[0].Message : ex.Message;

    /// <summary>Erro 2627/2601: violação de UNIQUE (ex.: dois pedidos iguais ao mesmo tempo).</summary>
    public static bool EhRegistroDuplicado(this SqlException ex) => ex.Number is 2627 or 2601;

    /// <summary>Converte DBNull para null ao passar parâmetros.</summary>
    public static object ValorOuDbNull(object? valor) => valor ?? DBNull.Value;
}
