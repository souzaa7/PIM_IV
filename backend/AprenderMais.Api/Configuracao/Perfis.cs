namespace AprenderMais.Api.Configuracao;

/// <summary>Nomes dos perfis usados no [Authorize(Roles = ...)]. Iguais ao enum PerfilUsuario.</summary>
public static class Perfis
{
    public const string Aluno = "Aluno";
    public const string Secretaria = "Secretaria";
    public const string Admin = "Admin";
}
