using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>
/// Verificações de perfil usadas pelos serviços.
/// Mesmo que a tela esconda um botão, o serviço confere de novo quem está pedindo.
/// </summary>
public static class Permissoes
{
    public static Aluno ExigirAluno(Usuario solicitante) =>
        solicitante as Aluno ?? throw new DominioException("Esta operação é exclusiva de alunos.");

    public static Secretaria ExigirSecretaria(Usuario solicitante) =>
        solicitante as Secretaria ?? throw new DominioException("Somente a secretaria pode realizar esta operação.");

    public static Administrador ExigirAdmin(Usuario solicitante) =>
        solicitante as Administrador ?? throw new DominioException("Somente o administrador pode realizar esta operação.");
}
