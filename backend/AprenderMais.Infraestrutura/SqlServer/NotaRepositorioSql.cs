using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

public class NotaRepositorioSql : INotaRepositorio
{
    private readonly ConexaoSql _conexao;

    public NotaRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<Nota?> ObterPorRespostaAsync(int idResposta)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("SELECT * FROM nota WHERE id_resposta = @id", conexao);
        comando.Parameters.AddWithValue("@id", idResposta);
        await using var leitor = await comando.ExecuteReaderAsync();

        if (!await leitor.ReadAsync())
            return null;

        return new Nota
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("id_nota")),
            Valor = leitor.GetDecimal(leitor.GetOrdinal("valor")),
            Feedback = leitor.TextoOuNulo("feedback"),
            DataAvaliacao = leitor.GetDateTime(leitor.GetOrdinal("data_avaliacao")),
            IdResposta = leitor.GetInt32(leitor.GetOrdinal("id_resposta")),
            IdSecretaria = leitor.GetInt32(leitor.GetOrdinal("id_secretaria"))
        };
    }

    public async Task<Nota> AdicionarAsync(Nota nota)
    {
        const string sql = @"
            INSERT INTO nota (valor, feedback, data_avaliacao, id_resposta, id_secretaria)
            VALUES (@valor, @feedback, @data, @id_resposta, @id_secretaria);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@valor", nota.Valor);
        comando.Parameters.AddWithValue("@feedback", LeitorExtensoes.ValorOuDbNull(nota.Feedback));
        comando.Parameters.AddWithValue("@data", nota.DataAvaliacao);
        comando.Parameters.AddWithValue("@id_resposta", nota.IdResposta);
        comando.Parameters.AddWithValue("@id_secretaria", nota.IdSecretaria);

        try
        {
            nota.Id = (int)(await comando.ExecuteScalarAsync())!;
        }
        catch (SqlException ex) when (ex.Number == 50010)
        {
            // Mensagem do trigger trg_nota_validar.
            throw new DominioException(ex.MensagemDoBanco());
        }
        catch (SqlException ex) when (ex.EhRegistroDuplicado())
        {
            throw new DominioException("Esta resposta já foi avaliada.");
        }
        return nota;
    }
}
