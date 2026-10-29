using System.Data;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;
using Microsoft.Data.SqlClient;

namespace AprenderMais.Infraestrutura.SqlServer;

/// <summary>Usuários: leitura pela view vw_usuario_perfil e cadastro pela procedure sp_cadastrar_usuario.</summary>
public class UsuarioRepositorioSql : IUsuarioRepositorio
{
    private const string SelectBase = "SELECT * FROM vw_usuario_perfil";
    private readonly ConexaoSql _conexao;

    public UsuarioRepositorioSql(ConexaoSql conexao)
    {
        _conexao = conexao;
    }

    public Task<Usuario?> ObterPorEmailAsync(string email) =>
        ObterUmAsync($"{SelectBase} WHERE email = @valor", email);

    public Task<Usuario?> ObterPorIdAsync(int id) =>
        ObterUmAsync($"{SelectBase} WHERE id_usuario = @valor", id);

    public Task<Usuario?> ObterPorIdAlunoAsync(int idAluno) =>
        ObterUmAsync($"{SelectBase} WHERE id_aluno = @valor", idAluno);

    public async Task<IReadOnlyList<Usuario>> ListarAsync()
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand($"{SelectBase} ORDER BY nome", conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var lista = new List<Usuario>();
        while (await leitor.ReadAsync())
            lista.Add(Mapear(leitor));
        return lista;
    }

    public async Task<Usuario> AdicionarAsync(Usuario usuario)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand("sp_cadastrar_usuario", conexao)
        {
            CommandType = CommandType.StoredProcedure
        };

        comando.Parameters.AddWithValue("@cpf", usuario.Cpf);
        comando.Parameters.AddWithValue("@nome", usuario.Nome);
        comando.Parameters.AddWithValue("@email", usuario.Email);
        comando.Parameters.AddWithValue("@senha_hash", usuario.SenhaHash);
        comando.Parameters.AddWithValue("@perfil", PerfilNoBanco(usuario));
        var idUsuario = comando.Parameters.Add("@id_usuario", SqlDbType.Int);
        idUsuario.Direction = ParameterDirection.Output;
        var idPerfil = comando.Parameters.Add("@id_perfil", SqlDbType.Int);
        idPerfil.Direction = ParameterDirection.Output;

        try
        {
            await comando.ExecuteNonQueryAsync();
        }
        catch (SqlException ex) when (ex.Number is 50001 or 50002)
        {
            // Erros de negócio lançados pela procedure (e-mail ou CPF repetido).
            throw new DominioException(ex.MensagemDoBanco());
        }
        catch (SqlException ex) when (ex.EhRegistroDuplicado())
        {
            throw new DominioException("Já existe um usuário com este e-mail ou CPF.");
        }

        usuario.Id = (int)idUsuario.Value;
        switch (usuario)
        {
            case Aluno aluno: aluno.IdAluno = (int)idPerfil.Value; break;
            case Secretaria secretaria: secretaria.IdSecretaria = (int)idPerfil.Value; break;
            case Administrador admin: admin.IdAdmin = (int)idPerfil.Value; break;
        }
        return usuario;
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(
            "UPDATE usuario SET nome = @nome, email = @email, ativo = @ativo WHERE id_usuario = @id", conexao);

        comando.Parameters.AddWithValue("@nome", usuario.Nome);
        comando.Parameters.AddWithValue("@email", usuario.Email);
        comando.Parameters.AddWithValue("@ativo", usuario.Ativo);
        comando.Parameters.AddWithValue("@id", usuario.Id);
        try
        {
            await comando.ExecuteNonQueryAsync();   // o trigger trg_usuario_auditoria registra no log
        }
        catch (SqlException ex) when (ex.EhRegistroDuplicado())
        {
            throw new DominioException("Já existe um usuário com este e-mail.");
        }
    }

    private async Task<Usuario?> ObterUmAsync(string sql, object valor)
    {
        await using var conexao = await _conexao.AbrirAsync();
        await using var comando = new SqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("@valor", valor);
        await using var leitor = await comando.ExecuteReaderAsync();

        return await leitor.ReadAsync() ? Mapear(leitor) : null;
    }

    /// <summary>Cria o objeto certo (Aluno, Secretaria ou Administrador) conforme a coluna perfil.</summary>
    private static Usuario Mapear(SqlDataReader leitor)
    {
        string perfil = leitor.GetString(leitor.GetOrdinal("perfil"));

        Usuario usuario = perfil switch
        {
            "aluno" => new Aluno { IdAluno = leitor.InteiroOuNulo("id_aluno") ?? 0 },
            "secretaria" => new Secretaria { IdSecretaria = leitor.InteiroOuNulo("id_secretaria") ?? 0 },
            _ => new Administrador { IdAdmin = leitor.InteiroOuNulo("id_admin") ?? 0 }
        };

        usuario.Id = leitor.GetInt32(leitor.GetOrdinal("id_usuario"));
        usuario.Cpf = leitor.GetString(leitor.GetOrdinal("cpf"));
        usuario.Nome = leitor.GetString(leitor.GetOrdinal("nome"));
        usuario.Email = leitor.GetString(leitor.GetOrdinal("email"));
        usuario.SenhaHash = leitor.GetString(leitor.GetOrdinal("senha_hash"));
        usuario.Ativo = leitor.GetBoolean(leitor.GetOrdinal("ativo"));
        usuario.DataCadastro = leitor.GetDateTime(leitor.GetOrdinal("data_cadastro"));
        return usuario;
    }

    private static string PerfilNoBanco(Usuario usuario) => usuario switch
    {
        Aluno => "aluno",
        Secretaria => "secretaria",
        _ => "admin"
    };
}
