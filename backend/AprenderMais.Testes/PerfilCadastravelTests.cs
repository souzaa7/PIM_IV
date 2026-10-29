using AprenderMais.Aplicacao.Servicos;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using Xunit;

namespace AprenderMais.Testes;

/// <summary>
/// A tela decide se mostra o botão de cadastro a partir de UsuarioService.PerfilCadastravel,
/// então a regra "quem cadastra quem" precisa ficar coberta aqui.
/// </summary>
public class PerfilCadastravelTests
{
    [Fact]
    public void Administrador_cadastra_secretaria()
    {
        var perfil = UsuarioService.PerfilCadastravel(new Administrador());

        Assert.Equal((PerfilUsuario?)PerfilUsuario.Secretaria, perfil);
    }

    [Fact]
    public void Secretaria_cadastra_aluno()
    {
        var perfil = UsuarioService.PerfilCadastravel(new Secretaria());

        Assert.Equal((PerfilUsuario?)PerfilUsuario.Aluno, perfil);
    }

    [Fact]
    public void Aluno_nao_cadastra_ninguem()
    {
        var perfil = UsuarioService.PerfilCadastravel(new Aluno());

        Assert.Null(perfil);
    }
}
