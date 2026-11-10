namespace AprenderMais.Dominio.Enums;

/// <summary>Perfis de acesso; os valores correspondem aos ids gravados no banco.</summary>
public enum PerfilUsuario
{
    Aluno = 1,
    Secretaria = 2,
    Admin = 3
}

/// <summary>Situação de uma matrícula do aluno em um conteúdo.</summary>
public enum StatusMatricula
{
    EmAndamento = 1,
    Concluido = 2,
    Cancelado = 3
}
