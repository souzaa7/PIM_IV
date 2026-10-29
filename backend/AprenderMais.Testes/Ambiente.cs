using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Aplicacao.Servicos;
using AprenderMais.Aplicacao.Sessao;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Infraestrutura.Memoria;
using AprenderMais.Infraestrutura.Seguranca;

namespace AprenderMais.Testes;

/// <summary>
/// Relógio "parado" em 29/10/2026 12:00 (Brasília) para que os testes de prazo
/// deem sempre o mesmo resultado, não importa o dia em que forem executados.
/// </summary>
internal class RelogioFixo : IRelogio
{
    public DateTime AgoraUtc { get; set; } = new(2026, 10, 29, 15, 0, 0, DateTimeKind.Utc);
    public DateTime AgoraBrasilia => AgoraUtc.AddHours(-3);
    public DateTime ParaBrasilia(DateTime utc) => utc.AddHours(-3);
}

/// <summary>
/// Monta o sistema completo em memória, com os dados de demonstração, para cada teste.
/// </summary>
internal class Ambiente
{
    public BancoEmMemoria Banco { get; } = new();
    public IPasswordHasher Hasher { get; } = new BCryptPasswordHasher();
    public RelogioFixo Relogio { get; } = new();

    public UsuarioRepositorioMemoria Usuarios { get; }
    public ConteudoRepositorioMemoria Conteudos { get; }
    public MatriculaRepositorioMemoria Matriculas { get; }
    public AtividadeRepositorioMemoria Atividades { get; }
    public RespostaRepositorioMemoria Respostas { get; }
    public NotaRepositorioMemoria Notas { get; }
    public CertificadoRepositorioMemoria Certificados { get; }
    public RelatorioRepositorioMemoria Relatorios { get; }

    public Ambiente()
    {
        DadosIniciais.Popular(Banco, Hasher);
        Usuarios = new UsuarioRepositorioMemoria(Banco);
        Conteudos = new ConteudoRepositorioMemoria(Banco);
        Matriculas = new MatriculaRepositorioMemoria(Banco);
        Atividades = new AtividadeRepositorioMemoria(Banco);
        Respostas = new RespostaRepositorioMemoria(Banco);
        Notas = new NotaRepositorioMemoria(Banco);
        Certificados = new CertificadoRepositorioMemoria(Banco);
        Relatorios = new RelatorioRepositorioMemoria(Banco);
    }

    public AutenticacaoService CriarAutenticacao() => new(Usuarios, Hasher, new SessaoUsuario());
    public ConteudoService CriarConteudoService() => new(Conteudos);
    public MatriculaService CriarMatriculaService() => new(Matriculas, Conteudos, Relogio);
    public UsuarioService CriarUsuarioService() => new(Usuarios, Hasher);
    public AtividadeService CriarAtividadeService() => new(Atividades, Conteudos, Matriculas, Respostas, Notas, Relogio);
    public RespostaService CriarRespostaService() => new(Respostas, Atividades, Matriculas, Notas, Certificados, Relogio);
    public NotaService CriarNotaService() => new(Respostas, Notas, Atividades, Conteudos, Relogio);
    public CertificadoService CriarCertificadoService() => new(Certificados, Usuarios, Conteudos, Relogio);
    public RelatorioService CriarRelatorioService() => new(Relatorios);

    public async Task<T> ObterAsync<T>(string email) where T : Usuario
    {
        var usuario = await Usuarios.ObterPorEmailAsync(email);
        return (T)usuario!;
    }
}
