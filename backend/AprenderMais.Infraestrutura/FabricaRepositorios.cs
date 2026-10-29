using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Repositorios;
using AprenderMais.Infraestrutura.Memoria;
using AprenderMais.Infraestrutura.SqlServer;

namespace AprenderMais.Infraestrutura;

/// <summary>Conjunto de repositórios usados pelo sistema.</summary>
public record Repositorios(
    IUsuarioRepositorio Usuarios,
    IConteudoRepositorio Conteudos,
    IMatriculaRepositorio Matriculas,
    IAtividadeRepositorio Atividades,
    IRespostaRepositorio Respostas,
    INotaRepositorio Notas,
    ICertificadoRepositorio Certificados,
    IRelatorioRepositorio Relatorios);

/// <summary>
/// Escolhe ONDE os dados ficam, conforme a configuração:
///   "Memoria"   - listas em memória com dados de demonstração (não precisa instalar nada);
///   "SqlServer" - banco real criado pelos scripts da pasta database/.
/// API e Desktop usam esta mesma fábrica; o resto do sistema não sabe qual foi escolhido.
/// </summary>
public static class FabricaRepositorios
{
    public static Repositorios Criar(string? tipoBanco, string? connectionString, IPasswordHasher hasher)
    {
        if (string.Equals(tipoBanco, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var conexao = new ConexaoSql(connectionString ?? string.Empty);
            return new Repositorios(
                new UsuarioRepositorioSql(conexao),
                new ConteudoRepositorioSql(conexao),
                new MatriculaRepositorioSql(conexao),
                new AtividadeRepositorioSql(conexao),
                new RespostaRepositorioSql(conexao),
                new NotaRepositorioSql(conexao),
                new CertificadoRepositorioSql(conexao),
                new RelatorioRepositorioSql(conexao));
        }

        var banco = new BancoEmMemoria();
        DadosIniciais.Popular(banco, hasher);

        return new Repositorios(
            new UsuarioRepositorioMemoria(banco),
            new ConteudoRepositorioMemoria(banco),
            new MatriculaRepositorioMemoria(banco),
            new AtividadeRepositorioMemoria(banco),
            new RespostaRepositorioMemoria(banco),
            new NotaRepositorioMemoria(banco),
            new CertificadoRepositorioMemoria(banco),
            new RelatorioRepositorioMemoria(banco));
    }
}
