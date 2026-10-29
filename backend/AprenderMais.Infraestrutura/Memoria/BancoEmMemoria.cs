using AprenderMais.Dominio.Entidades;

namespace AprenderMais.Infraestrutura.Memoria;

/// <summary>
/// "Banco de dados" de demonstração: guarda tudo em listas enquanto o programa está aberto.
/// Serve para rodar o sistema sem instalar o SQL Server (modo "Memoria" do appsettings).
/// </summary>
public class BancoEmMemoria
{
    private readonly Dictionary<string, int> _contadores = new();
    private readonly object _trava = new();

    public List<Usuario> Usuarios { get; } = new();
    public List<Conteudo> Conteudos { get; } = new();
    public List<Atividade> Atividades { get; } = new();
    public List<Matricula> Matriculas { get; } = new();
    public List<Resposta> Respostas { get; } = new();
    public List<Nota> Notas { get; } = new();
    public List<Certificado> Certificados { get; } = new();

    /// <summary>Simula o IDENTITY do banco: devolve o próximo id da "tabela".</summary>
    public int ProximoId(string tabela)
    {
        lock (_trava)
        {
            _contadores.TryGetValue(tabela, out var atual);
            atual++;
            _contadores[tabela] = atual;
            return atual;
        }
    }
}
