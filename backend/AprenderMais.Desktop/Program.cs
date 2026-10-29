
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Aplicacao.Servicos;
using AprenderMais.Aplicacao.Sessao;
using AprenderMais.Desktop.Formularios;
using AprenderMais.Infraestrutura;
using AprenderMais.Infraestrutura.Relogio;
using AprenderMais.Infraestrutura.Seguranca;
using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace AprenderMais.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            ExecutarAplicacao();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ocorreu um erro inesperado ao executar o AprenderMais.\n\n" +
                $"Detalhes: {ex.Message}",
                "Erro na aplicação",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void ExecutarAplicacao()
    {
        // Raiz de composição: centraliza a criação das dependências.
        // O arquivo appsettings.json decide se os dados ficam em memória ou no SQL Server.
        var (tipoBanco, connectionString) = LerConfiguracao();

        IPasswordHasher hasher = new BCryptPasswordHasher();
        IRelogio relogio = new RelogioSistema();

        var repositorios = FabricaRepositorios.Criar(tipoBanco, connectionString, hasher);
        var usuarios = repositorios.Usuarios;
        var conteudos = repositorios.Conteudos;
        var matriculas = repositorios.Matriculas;

        var sessao = new SessaoUsuario();

        var autenticacao = new AutenticacaoService(
            usuarios,
            hasher,
            sessao);

        var conteudoService = new ConteudoService(conteudos);

        var matriculaService = new MatriculaService(
            matriculas,
            conteudos,
            relogio);

        var usuarioService = new UsuarioService(
            usuarios,
            hasher);

        var respostaService = new RespostaService(
            repositorios.Respostas,
            repositorios.Atividades,
            matriculas,
            repositorios.Notas,
            repositorios.Certificados,
            relogio);

        var notaService = new NotaService(
            repositorios.Respostas,
            repositorios.Notas,
            repositorios.Atividades,
            conteudos,
            relogio);

        // Permite voltar ao login após sair da conta.
        // O aplicativo encerra quando o usuário fecha o login
        // ou escolhe sair da conta na janela principal.
        while (true)
        {
            using (var login = new LoginForm(autenticacao))
            {
                if (login.ShowDialog() != DialogResult.OK)
                {
                    break;
                }
            }

            bool sairDaConta;

            using (var principal = new PrincipalForm(
                sessao,
                conteudoService,
                matriculaService,
                usuarioService,
                respostaService,
                notaService))
            {
                Application.Run(principal);
                sairDaConta = principal.SairDaConta;
            }

            autenticacao.Sair();

            if (!sairDaConta)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Lê appsettings.json (ao lado do .exe). Se o arquivo não existir, usa o modo memória.
    /// </summary>
    private static (string? TipoBanco, string? ConnectionString) LerConfiguracao()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(caminho))
            return ("Memoria", null);

        using var json = JsonDocument.Parse(File.ReadAllText(caminho));
        var raiz = json.RootElement;

        string? tipo = raiz.TryGetProperty("Banco", out var banco) && banco.TryGetProperty("Tipo", out var t)
            ? t.GetString()
            : "Memoria";

        string? conexao = raiz.TryGetProperty("ConnectionStrings", out var cs) && cs.TryGetProperty("AprenderMais", out var c)
            ? c.GetString()
            : null;

        return (tipo, conexao);
    }
}
