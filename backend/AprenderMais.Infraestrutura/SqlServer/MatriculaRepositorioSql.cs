using System.Data;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

public class MatriculaRepositorioSql : IMatriculaRepositorio
{
    private readonly ConexaoSql _conexao;

    public MatriculaRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public async Task<Matricula?> ObterPorIdAsync(int id)
    {
        var lista = await ConsultarAsync("SELECT * FROM matricula WHERE id_matricula = @a", id);
        return lista.FirstOrDefault();
    }

    public async Task<Matricula?> ObterPorAlunoEConteudoAsync(int idAluno, int idConteudo)
    {
        var lista = await ConsultarAsync(
            "SELECT * FROM matricula WHERE id_aluno = @a AND id_conteudo = @b", idAluno, idConteudo);
        return lista.FirstOrDefault();
    }

    public Task<IReadOnlyList<Matricula>> ListarPorAlunoAsync(int idAluno) =>
        ConsultarAsync("SELECT * FROM matricula WHERE id_aluno = @a ORDER BY data_matricula", idAluno);

    public async Task<Matricula> AdicionarAsync(Matricula matricula)
    {
        const string sql = @"
            INSERT INTO matricula (id_aluno, id_conteudo, status, progresso, data_matricula)
            VALUES (@id_aluno, @id_conteudo, @status, @progresso, @data_matricula);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@id_aluno", matricula.IdAluno);
        comando.Parameters.AddWithValue("@id_conteudo", matricula.IdConteudo);
        comando.Parameters.AddWithValue("@status", StatusNoBanco(matricula.Status));
        comando.Parameters.AddWithValue("@progresso", matricula.Progresso);
        comando.Parameters.AddWithValue("@data_matricula", matricula.DataMatricula);

        matricula.Id = (int)(await comando.ExecuteScalarAsync())!;
        return matricula;
    }

    public async Task AtualizarAsync(Matricula matricula)
    {
        const string sql = @"
            UPDATE matricula
               SET status = @status, progresso = @progresso,
                   data_matricula = @data_matricula, data_conclusao = @data_conclusao
             WHERE id_matricula = @id";

        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@status", StatusNoBanco(matricula.Status));
        comando.Parameters.AddWithValue("@progresso", matricula.Progresso);
        comando.Parameters.AddWithValue("@data_matricula", matricula.DataMatricula);
        comando.Parameters.AddWithValue("@data_conclusao", LeitorExtensoes.ValorOuDbNull(matricula.DataConclusao));
        comando.Parameters.AddWithValue("@id", matricula.Id);
        await comando.ExecuteNonQueryAsync();
    }

    /// <summary>Chama a procedure sp_calcular_progresso.</summary>
    public async Task<(int Avaliadas, int Total)> ContarAtividadesAsync(int idAluno, int idConteudo)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("sp_calcular_progresso", conexao)
        {
            CommandType = CommandType.StoredProcedure
        };
        comando.Parameters.AddWithValue("@id_aluno", idAluno);
        comando.Parameters.AddWithValue("@id_conteudo", idConteudo);

        await using var leitor = await comando.ExecuteReaderAsync();
        await leitor.ReadAsync();
        return (leitor.GetInt32(0), leitor.GetInt32(1));
    }

    private async Task<IReadOnlyList<Matricula>> ConsultarAsync(string sql, int a, int b = 0)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@a", a);
        comando.Parameters.AddWithValue("@b", b);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<Matricula>();
        while (await leitor.ReadAsync())
        {
            lista.Add(new Matricula
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("id_matricula")),
                IdAluno = leitor.GetInt32(leitor.GetOrdinal("id_aluno")),
                IdConteudo = leitor.GetInt32(leitor.GetOrdinal("id_conteudo")),
                Status = StatusDoBanco(leitor.GetString(leitor.GetOrdinal("status"))),
                Progresso = leitor.GetDecimal(leitor.GetOrdinal("progresso")),
                DataMatricula = leitor.GetDateTime(leitor.GetOrdinal("data_matricula")),
                DataConclusao = leitor.DataOuNulo("data_conclusao")
            });
        }
        return lista;
    }

    // O banco guarda o status como texto; o C# usa enum.
    private static string StatusNoBanco(StatusMatricula status) => status switch
    {
        StatusMatricula.Concluido => "concluido",
        StatusMatricula.Cancelado => "cancelado",
        _ => "em_andamento"
    };

    private static StatusMatricula StatusDoBanco(string status) => status switch
    {
        "concluido" => StatusMatricula.Concluido,
        "cancelado" => StatusMatricula.Cancelado,
        _ => StatusMatricula.EmAndamento
    };
}
