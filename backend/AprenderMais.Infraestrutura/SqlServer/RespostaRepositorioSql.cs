using AprenderMais.Dominio.Consultas;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

public class RespostaRepositorioSql : IRespostaRepositorio
{
    private readonly ConexaoSql _conexao;

    public RespostaRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<Resposta?> ObterPorIdAsync(int id) =>
        (await ConsultarAsync("SELECT * FROM resposta WHERE id_resposta = @a", id)).FirstOrDefault();

    public async Task<Resposta?> ObterPorAlunoEAtividadeAsync(int idAluno, int idAtividade) =>
        (await ConsultarAsync("SELECT * FROM resposta WHERE id_aluno = @a AND id_atividade = @b", idAluno, idAtividade))
        .FirstOrDefault();

    public Task<IReadOnlyList<Resposta>> ListarPorAlunoAsync(int idAluno) =>
        ConsultarAsync("SELECT * FROM resposta WHERE id_aluno = @a ORDER BY data_envio", idAluno);

    /// <summary>Lê a view vw_respostas_pendentes.</summary>
    public async Task<IReadOnlyList<RespostaPendente>> ListarPendentesAsync()
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("SELECT * FROM vw_respostas_pendentes ORDER BY data_envio", conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<RespostaPendente>();
        while (await leitor.ReadAsync())
        {
            lista.Add(new RespostaPendente(
                leitor.GetInt32(leitor.GetOrdinal("id_resposta")),
                leitor.GetString(leitor.GetOrdinal("nome_aluno")),
                leitor.GetString(leitor.GetOrdinal("titulo_atividade")),
                leitor.GetString(leitor.GetOrdinal("titulo_conteudo")),
                leitor.GetDecimal(leitor.GetOrdinal("nota_maxima")),
                leitor.GetString(leitor.GetOrdinal("texto_resposta")),
                leitor.TextoOuNulo("arquivo_url"),
                leitor.GetDateTime(leitor.GetOrdinal("data_envio"))));
        }
        return lista;
    }

    public async Task<Resposta> AdicionarAsync(Resposta resposta)
    {
        const string sql = @"
            INSERT INTO resposta (texto_resposta, arquivo_url, data_envio, id_atividade, id_aluno)
            VALUES (@texto, @arquivo, @data_envio, @id_atividade, @id_aluno);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@texto", resposta.TextoResposta);
        comando.Parameters.AddWithValue("@arquivo", LeitorExtensoes.ValorOuDbNull(resposta.ArquivoUrl));
        comando.Parameters.AddWithValue("@data_envio", resposta.DataEnvio);
        comando.Parameters.AddWithValue("@id_atividade", resposta.IdAtividade);
        comando.Parameters.AddWithValue("@id_aluno", resposta.IdAluno);

        try
        {
            resposta.Id = (int)(await comando.ExecuteScalarAsync())!;
        }
        catch (SqlException ex) when (ex.Number is 50020 or 50021)
        {
            // Mensagens do trigger trg_resposta_validar.
            throw new DominioException(ex.MensagemDoBanco());
        }
        catch (SqlException ex) when (ex.EhRegistroDuplicado())
        {
            throw new DominioException("Você já enviou a resposta desta atividade.");
        }
        return resposta;
    }

    private async Task<IReadOnlyList<Resposta>> ConsultarAsync(string sql, int a, int b = 0)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@a", a);
        comando.Parameters.AddWithValue("@b", b);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<Resposta>();
        while (await leitor.ReadAsync())
        {
            lista.Add(new Resposta
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("id_resposta")),
                TextoResposta = leitor.GetString(leitor.GetOrdinal("texto_resposta")),
                ArquivoUrl = leitor.TextoOuNulo("arquivo_url"),
                DataEnvio = leitor.GetDateTime(leitor.GetOrdinal("data_envio")),
                IdAtividade = leitor.GetInt32(leitor.GetOrdinal("id_atividade")),
                IdAluno = leitor.GetInt32(leitor.GetOrdinal("id_aluno"))
            });
        }
        return lista;
    }
}
