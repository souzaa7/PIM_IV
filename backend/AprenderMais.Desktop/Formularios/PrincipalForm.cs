using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Servicos;
using AprenderMais.Aplicacao.Sessao;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using AprenderMais.Dominio.Excecoes;

namespace AprenderMais.Desktop.Formularios;

/// <summary>
/// Janela principal. As abas exibidas dependem do perfil do usuário autenticado.
/// </summary>
public class PrincipalForm : Form
{
    private readonly ConteudoService _conteudos;
    private readonly MatriculaService _matriculas;
    private readonly UsuarioService _usuarios;
    private readonly Usuario _usuario;

    private readonly TabControl _abas = new() { Dock = DockStyle.Fill };
    private readonly DataGridView _gridCatalogo = GridHelper.Criar();
    private readonly DataGridView _gridMatriculas = GridHelper.Criar();
    private readonly DataGridView _gridGestao = GridHelper.Criar();
    private readonly DataGridView _gridUsuarios = GridHelper.Criar();

    // Botões de ação: ficam como campos para refletirem a seleção atual das tabelas.
    private readonly Button _btnMatricular = new() { Text = "Matricular-me no conteúdo selecionado", AutoSize = true };
    private readonly Button _btnCancelarMatricula = new() { Text = "Cancelar matrícula selecionada", AutoSize = true };
    private readonly Button _btnNovoConteudo = new() { Text = "Novo conteúdo...", AutoSize = true };
    private readonly Button _btnAlternarConteudo = new() { AutoSize = true };
    private readonly Button _btnAlternarUsuario = new() { AutoSize = true };

    /// <summary>Verdadeiro quando o usuário clicou em "Sair da conta" (volta ao login).</summary>
    public bool SairDaConta { get; private set; }

    public PrincipalForm(
        SessaoUsuario sessao,
        ConteudoService conteudos,
        MatriculaService matriculas,
        UsuarioService usuarios)
    {
        _conteudos = conteudos;
        _matriculas = matriculas;
        _usuarios = usuarios;
        _usuario = sessao.Atual ?? throw new InvalidOperationException("Nenhum usuário autenticado.");

        Text = "Aprender Mais";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1000, 620);
        MinimumSize = new Size(820, 480);

        _gridCatalogo.SelectionChanged += (_, _) => AtualizarBotoes();
        _gridMatriculas.SelectionChanged += (_, _) => AtualizarBotoes();
        _gridGestao.SelectionChanged += (_, _) => AtualizarBotoes();
        _gridUsuarios.SelectionChanged += (_, _) => AtualizarBotoes();

        // Controle que preenche o espaço é adicionado primeiro; o painel do topo depois.
        Controls.Add(_abas);
        Controls.Add(CriarPainelTopo());

        _abas.TabPages.Add(CriarAbaCatalogo());
        switch (_usuario.Perfil)
        {
            case PerfilUsuario.Aluno:
                _abas.TabPages.Add(CriarAbaMinhasMatriculas());
                break;
            case PerfilUsuario.Secretaria:
                _abas.TabPages.Add(CriarAbaGestaoConteudos());
                break;
            case PerfilUsuario.Admin:
                _abas.TabPages.Add(CriarAbaUsuarios());
                break;
        }

        AtualizarBotoes();

        Shown += async (_, _) => await RecarregarAsync();
    }

    // ------------------------------------------------------------------ montagem da tela

    private Panel CriarPainelTopo()
    {
        var painel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(12, 6, 12, 6)
        };

        var lblUsuario = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Text = $"{_usuario.Nome}  -  {RotuloPerfil(_usuario.Perfil)}"
        };

        var btnSair = new Button
        {
            Dock = DockStyle.Right,
            Width = 120,
            Text = "Sair da conta"
        };
        btnSair.Click += (_, _) =>
        {
            SairDaConta = true;
            Close();
        };

        painel.Controls.Add(lblUsuario);
        painel.Controls.Add(btnSair);
        return painel;
    }

    private TabPage CriarAbaCatalogo()
    {
        var aba = new TabPage("Catálogo de conteúdos");

        GridHelper.Coluna(_gridCatalogo, nameof(ConteudoDto.Titulo), "Título", 220);
        GridHelper.Coluna(_gridCatalogo, nameof(ConteudoDto.Categoria), "Categoria", 120);
        GridHelper.Coluna(_gridCatalogo, nameof(ConteudoDto.Nivel), "Nível", 100);
        GridHelper.Coluna(_gridCatalogo, nameof(ConteudoDto.Duracao), "Duração", 70);
        GridHelper.Coluna(_gridCatalogo, nameof(ConteudoDto.Aulas), "Aulas", 60);

        aba.Controls.Add(_gridCatalogo);

        if (_usuario is Aluno)
        {
            _btnMatricular.Click += async (_, _) => await MatricularAsync();
            aba.Controls.Add(CriarRodape(_btnMatricular));
        }

        return aba;
    }

    private TabPage CriarAbaMinhasMatriculas()
    {
        var aba = new TabPage("Minhas matrículas");

        GridHelper.Coluna(_gridMatriculas, nameof(MatriculaDto.Conteudo), "Conteúdo", 220);
        GridHelper.Coluna(_gridMatriculas, nameof(MatriculaDto.Status), "Situação", 100);
        GridHelper.Coluna(_gridMatriculas, nameof(MatriculaDto.ProgressoTexto), "Progresso", 80);
        GridHelper.Coluna(_gridMatriculas, nameof(MatriculaDto.DataMatricula), "Matriculado em", 100);

        aba.Controls.Add(_gridMatriculas);

        _btnCancelarMatricula.Click += async (_, _) => await CancelarMatriculaAsync();
        aba.Controls.Add(CriarRodape(_btnCancelarMatricula));

        return aba;
    }

    private TabPage CriarAbaGestaoConteudos()
    {
        var aba = new TabPage("Gerenciar conteúdos");

        GridHelper.Coluna(_gridGestao, nameof(ConteudoDto.Titulo), "Título", 220);
        GridHelper.Coluna(_gridGestao, nameof(ConteudoDto.Categoria), "Categoria", 120);
        GridHelper.Coluna(_gridGestao, nameof(ConteudoDto.Nivel), "Nível", 100);
        GridHelper.Coluna(_gridGestao, nameof(ConteudoDto.Duracao), "Duração", 70);
        GridHelper.Coluna(_gridGestao, nameof(ConteudoDto.Aulas), "Aulas", 60);
        GridHelper.Coluna(_gridGestao, nameof(ConteudoDto.Situacao), "Situação", 80);

        aba.Controls.Add(_gridGestao);

        _btnNovoConteudo.Click += async (_, _) => await NovoConteudoAsync();
        _btnAlternarConteudo.Click += async (_, _) => await AlternarConteudoAsync();

        var botoes = new List<Control> { _btnNovoConteudo, _btnAlternarConteudo };
        if (CriarBotaoNovoUsuario() is { } btnNovoUsuario)
            botoes.Add(btnNovoUsuario);

        aba.Controls.Add(CriarRodape(botoes.ToArray()));
        return aba;
    }

    private TabPage CriarAbaUsuarios()
    {
        var aba = new TabPage("Usuários");

        GridHelper.Coluna(_gridUsuarios, nameof(UsuarioDto.Nome), "Nome", 180);
        GridHelper.Coluna(_gridUsuarios, nameof(UsuarioDto.Email), "E-mail", 220);
        GridHelper.Coluna(_gridUsuarios, nameof(UsuarioDto.Perfil), "Perfil", 100);
        GridHelper.Coluna(_gridUsuarios, nameof(UsuarioDto.Situacao), "Situação", 80);

        aba.Controls.Add(_gridUsuarios);

        _btnAlternarUsuario.Click += async (_, _) => await AlternarUsuarioAsync();

        var botoes = new List<Control>();
        if (CriarBotaoNovoUsuario() is { } btnNovoUsuario)
            botoes.Add(btnNovoUsuario);
        botoes.Add(_btnAlternarUsuario);

        aba.Controls.Add(CriarRodape(botoes.ToArray()));
        return aba;
    }

    /// <summary>
    /// Botão de cadastro de usuário. O perfil cadastrável vem do serviço (única fonte da regra);
    /// quem não pode cadastrar ninguém não recebe o botão.
    /// </summary>
    private Button? CriarBotaoNovoUsuario()
    {
        if (UsuarioService.PerfilCadastravel(_usuario) is not { } perfil)
            return null;

        var botao = new Button { Text = RotuloNovoCadastro(perfil) + "...", AutoSize = true };
        botao.Click += async (_, _) => await NovoUsuarioAsync();
        return botao;
    }

    private static FlowLayoutPanel CriarRodape(params Control[] botoes)
    {
        var painel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(8)
        };
        painel.Controls.AddRange(botoes);
        return painel;
    }

    // ------------------------------------------------------------------ carga de dados

    private async Task RecarregarAsync()
    {
        await ExecutarAsync(async () =>
        {
            GridHelper.Preencher(_gridCatalogo, await _conteudos.ListarAtivosAsync(), c => c.Id);

            if (_usuario is Aluno)
                GridHelper.Preencher(_gridMatriculas, await _matriculas.ListarDoAlunoAsync(_usuario), m => m.IdMatricula);

            if (_usuario is Secretaria)
                GridHelper.Preencher(_gridGestao, await _conteudos.ListarTodosAsync(_usuario), c => c.Id);

            if (_usuario is Administrador)
                GridHelper.Preencher(_gridUsuarios, await _usuarios.ListarAsync(_usuario), u => u.Id);

            AtualizarBotoes();
        });
    }

    /// <summary>
    /// Habilita cada botão só quando a ação é possível e deixa o rótulo dizer o que vai acontecer
    /// ("Desativar" ou "Ativar"), em vez de mandar o usuário descobrir pelo aviso de erro.
    /// </summary>
    private void AtualizarBotoes()
    {
        _btnMatricular.Enabled = _gridCatalogo.CurrentRow?.DataBoundItem is ConteudoDto;

        _btnCancelarMatricula.Enabled =
            _gridMatriculas.CurrentRow?.DataBoundItem is MatriculaDto { PodeCancelar: true };

        AtualizarBotaoAlternar(_btnAlternarConteudo,
            (_gridGestao.CurrentRow?.DataBoundItem as ConteudoDto)?.Ativo, "conteúdo");

        AtualizarBotaoAlternar(_btnAlternarUsuario,
            (_gridUsuarios.CurrentRow?.DataBoundItem as UsuarioDto)?.Ativo, "usuário");
    }

    private static void AtualizarBotaoAlternar(Button botao, bool? ativoAtual, string objeto)
    {
        botao.Enabled = ativoAtual.HasValue;
        botao.Text = ativoAtual switch
        {
            true => $"Desativar {objeto}",
            false => $"Ativar {objeto}",
            null => $"Ativar / desativar {objeto}"
        };
    }

    // ------------------------------------------------------------------ ações

    private async Task MatricularAsync()
    {
        if (_gridCatalogo.CurrentRow?.DataBoundItem is not ConteudoDto conteudo)
        {
            Aviso("Selecione um conteúdo no catálogo.");
            return;
        }

        await ExecutarAsync(async () =>
        {
            await _matriculas.MatricularAsync(conteudo.Id, _usuario);
            Informacao($"Matrícula realizada em \"{conteudo.Titulo}\".");
            await RecarregarAsync();
        });
    }

    private async Task CancelarMatriculaAsync()
    {
        if (_gridMatriculas.CurrentRow?.DataBoundItem is not MatriculaDto matricula)
        {
            Aviso("Selecione uma matrícula.");
            return;
        }

        if (!Confirmar($"Cancelar a matrícula em \"{matricula.Conteudo}\"?"))
            return;

        await ExecutarAsync(async () =>
        {
            await _matriculas.CancelarAsync(matricula.IdMatricula, _usuario);
            await RecarregarAsync();
        });
    }

    private async Task NovoConteudoAsync()
    {
        using var janela = new NovoConteudoForm(dto => _conteudos.CriarAsync(dto, _usuario));
        if (janela.ShowDialog(this) == DialogResult.OK)
            await RecarregarAsync();
    }

    private async Task AlternarConteudoAsync()
    {
        if (_gridGestao.CurrentRow?.DataBoundItem is not ConteudoDto conteudo)
        {
            Aviso("Selecione um conteúdo.");
            return;
        }

        if (conteudo.Ativo &&
            !Confirmar($"Desativar o conteúdo \"{conteudo.Titulo}\"?\n\nEle deixa de aparecer no catálogo dos alunos."))
            return;

        await ExecutarAsync(async () =>
        {
            await _conteudos.AlterarAtivoAsync(conteudo.Id, !conteudo.Ativo, _usuario);
            await RecarregarAsync();
        });
    }

    private async Task AlternarUsuarioAsync()
    {
        if (_gridUsuarios.CurrentRow?.DataBoundItem is not UsuarioDto usuario)
        {
            Aviso("Selecione um usuário.");
            return;
        }

        if (usuario.Ativo &&
            !Confirmar($"Desativar o usuário \"{usuario.Nome}\"?\n\nEle não conseguirá entrar no sistema até ser reativado."))
            return;

        await ExecutarAsync(async () =>
        {
            await _usuarios.AlterarAtivoAsync(usuario.Id, !usuario.Ativo, _usuario);
            await RecarregarAsync();
        });
    }

    private async Task NovoUsuarioAsync()
    {
        if (UsuarioService.PerfilCadastravel(_usuario) is not { } perfil)
            return;

        using var janela = new NovoUsuarioForm(
            RotuloNovoCadastro(perfil),
            RotuloPerfil(perfil),
            dto => _usuarios.CriarAsync(dto, _usuario));

        if (janela.ShowDialog(this) != DialogResult.OK)
            return;

        Informacao($"Cadastro realizado: {janela.UsuarioCriado?.Nome} ({RotuloPerfil(perfil)}).");
        await RecarregarAsync();
    }

    // ------------------------------------------------------------------ utilidades

    /// <summary>Executa uma ação e mostra erros de regra de negócio ao usuário sem derrubar o programa.</summary>
    private async Task ExecutarAsync(Func<Task> acao)
    {
        try
        {
            await acao();
        }
        catch (DominioException ex)
        {
            Aviso(ex.Message);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Erro inesperado: " + ex.Message, "Erro",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Aviso(string mensagem) =>
        MessageBox.Show(this, mensagem, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private void Informacao(string mensagem) =>
        MessageBox.Show(this, mensagem, "Aprender Mais", MessageBoxButtons.OK, MessageBoxIcon.Information);

    /// <summary>Pergunta Sim/Não com "Não" como padrão: apertar Enter sem pensar não executa a ação.</summary>
    private bool Confirmar(string mensagem) =>
        MessageBox.Show(this, mensagem, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    private static string RotuloNovoCadastro(PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Secretaria => "Nova secretaria",
        PerfilUsuario.Aluno => "Novo aluno",
        _ => "Novo usuário"
    };

    private static string RotuloPerfil(PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Aluno => "Aluno",
        PerfilUsuario.Secretaria => "Secretaria",
        PerfilUsuario.Admin => "Administrador",
        _ => perfil.ToString()
    };
}
