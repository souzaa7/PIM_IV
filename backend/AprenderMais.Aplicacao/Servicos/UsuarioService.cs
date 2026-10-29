using System.Net.Mail;
using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>Casos de uso UC02 (cadastro), UC10 (cadastrar secretaria) e UC11 (desativar cadastros).</summary>
public class UsuarioService
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IPasswordHasher _hasher;

    public UsuarioService(IUsuarioRepositorio usuarios, IPasswordHasher hasher)
    {
        _usuarios = usuarios;
        _hasher = hasher;
    }

    /// <summary>
    /// Perfil que o solicitante pode cadastrar: administrador cadastra secretaria,
    /// secretaria cadastra aluno. Retorna null quando não pode cadastrar ninguém.
    /// A tela usa este método para decidir se mostra o botão de cadastro.
    /// </summary>
    public static PerfilUsuario? PerfilCadastravel(Usuario solicitante) => solicitante switch
    {
        Administrador => PerfilUsuario.Secretaria,
        Secretaria => PerfilUsuario.Aluno,
        _ => null
    };

    /// <summary>UC02 - O próprio visitante se cadastra como aluno (não precisa estar logado).</summary>
    public async Task<UsuarioDto> CadastrarAlunoAsync(NovoUsuarioDto dto)
    {
        var aluno = new Aluno();
        PreencherEValidar(aluno, dto);

        await _usuarios.AdicionarAsync(aluno);
        return ParaDto(aluno);
    }

    /// <summary>UC10 - Cadastro feito por um usuário logado (ADM cria secretaria; secretaria cria aluno).</summary>
    public async Task<UsuarioDto> CriarAsync(NovoUsuarioDto dto, Usuario solicitante)
    {
        var perfil = PerfilCadastravel(solicitante)
            ?? throw new DominioException("Você não tem permissão para cadastrar usuários.");

        Usuario novoUsuario = perfil == PerfilUsuario.Secretaria ? new Secretaria() : new Aluno();
        PreencherEValidar(novoUsuario, dto);

        await _usuarios.AdicionarAsync(novoUsuario);
        return ParaDto(novoUsuario);
    }

    public async Task<IReadOnlyList<UsuarioDto>> ListarAsync(Usuario solicitante)
    {
        Permissoes.ExigirAdmin(solicitante);

        var lista = await _usuarios.ListarAsync();
        return lista.OrderBy(u => u.Nome).Select(ParaDto).ToList();
    }

    /// <summary>UC11 - Desativar (ou reativar) cadastro.</summary>
    public async Task AlterarAtivoAsync(int idUsuario, bool ativo, Usuario solicitante)
    {
        Permissoes.ExigirAdmin(solicitante);

        if (idUsuario == solicitante.Id && !ativo)
            throw new DominioException("Você não pode desativar o seu próprio usuário.");

        var usuario = await _usuarios.ObterPorIdAsync(idUsuario)
            ?? throw new DominioException("Usuário não encontrado.");

        usuario.Ativo = ativo;
        await _usuarios.AtualizarAsync(usuario);
    }

    public static UsuarioDto ParaDto(Usuario u) =>
        new(u.Id, u.Nome, u.Email, RotuloPerfil(u.Perfil), u.Ativo);

    public static string RotuloPerfil(PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Aluno => "Aluno",
        PerfilUsuario.Secretaria => "Secretaria",
        PerfilUsuario.Admin => "Administrador",
        _ => perfil.ToString()
    };

    /// <summary>Regras de cadastro comuns a todos os perfis.</summary>
    private void PreencherEValidar(Usuario usuario, NovoUsuarioDto dto)
    {
        if (dto is null)
            throw new DominioException("Informe os dados do usuário.");

        if (string.IsNullOrWhiteSpace(dto.Cpf) ||
            string.IsNullOrWhiteSpace(dto.Nome) ||
            string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Senha))
        {
            throw new DominioException("Preencha CPF, nome, e-mail e senha.");
        }

        // Guarda só os números do CPF (o banco usa VARCHAR(11)).
        string cpf = new string(dto.Cpf.Where(char.IsDigit).ToArray());
        string nome = dto.Nome.Trim();
        string email = dto.Email.Trim().ToLowerInvariant();

        if (cpf.Length != 11)
            throw new DominioException("O CPF deve conter 11 dígitos.");

        if (nome.Length < 2)
            throw new DominioException("Informe um nome válido.");

        try
        {
            _ = new MailAddress(email);
        }
        catch (FormatException)
        {
            throw new DominioException("Informe um e-mail válido.");
        }

        if (dto.Senha.Length < 8)
            throw new DominioException("A senha deve ter pelo menos 8 caracteres.");

        usuario.Cpf = cpf;
        usuario.Nome = nome;
        usuario.Email = email;
        usuario.SenhaHash = _hasher.Hash(dto.Senha);
        usuario.Ativo = true;
    }
}
