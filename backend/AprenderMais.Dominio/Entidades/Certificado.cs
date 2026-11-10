namespace AprenderMais.Dominio.Entidades;

/// <summary>Comprovante emitido quando o aluno conclui um conteúdo.</summary>
public class Certificado
{
    public int Id { get; set; }
    public int IdAluno { get; set; }
    public int IdConteudo { get; set; }
    public string CodigoValidacao { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
}
