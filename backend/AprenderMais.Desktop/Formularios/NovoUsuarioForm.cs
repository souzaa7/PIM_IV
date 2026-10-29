using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Dominio.Excecoes;

namespace AprenderMais.Desktop.Formularios;

/// <summary>
/// Janela de cadastro de usuário. O perfil não é escolhido aqui: quem decide é a regra do
/// serviço (administrador cadastra secretaria, secretaria cadastra aluno). A janela só
/// recebe o nome do perfil para mostrar ao usuário e só fecha com sucesso quando o
/// serviço aceita os dados.
/// </summary>
public class NovoUsuarioForm : Form
{
    private readonly Func<NovoUsuarioDto, Task<UsuarioDto>> _salvar;
    private int _proximaLinha;

    private readonly TextBox _nome = new();
    private readonly TextBox _email = new();
    private readonly TextBox _cpf = new();
    private readonly TextBox _senha = new();
    private readonly TextBox _confirmacao = new();
    private readonly Button _btnSalvar = new();
    private readonly Button _btnCancelar = new();

    /// <summary>Usuário cadastrado, disponível depois que a janela fecha com DialogResult.OK.</summary>
    public UsuarioDto? UsuarioCriado { get; private set; }

    public NovoUsuarioForm(string tituloJanela, string perfil, Func<NovoUsuarioDto, Task<UsuarioDto>> salvar)
    {
        _salvar = salvar;

        Text = tituloJanela;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(460, 340);

        _cpf.PlaceholderText = "000.000.000-00";
        _cpf.MaxLength = 14;

        _nome.MaxLength = 150;
        _email.MaxLength = 150;

        _senha.UseSystemPasswordChar = true;
        _senha.PlaceholderText = "Mínimo de 8 caracteres";
        _confirmacao.UseSystemPasswordChar = true;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        AdicionarLinha(layout, "Perfil", new Label
        {
            Text = perfil,
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold)
        });
        AdicionarLinha(layout, "Nome", _nome);
        AdicionarLinha(layout, "E-mail", _email);
        AdicionarLinha(layout, "CPF", _cpf);
        AdicionarLinha(layout, "Senha", _senha);
        AdicionarLinha(layout, "Confirmar senha", _confirmacao);

        _btnSalvar.Text = "Salvar";
        _btnSalvar.Size = new Size(100, 32);
        _btnSalvar.Click += Salvar_Click;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Size = new Size(100, 32);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        var botoes = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true
        };
        botoes.Controls.Add(_btnCancelar);
        botoes.Controls.Add(_btnSalvar);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(botoes, 0, _proximaLinha++);
        layout.SetColumnSpan(botoes, 2);

        Controls.Add(layout);

        AcceptButton = _btnSalvar;
        CancelButton = _btnCancelar;
        ActiveControl = _nome; // o cursor já abre no primeiro campo editável
    }

    private void AdicionarLinha(TableLayoutPanel layout, string rotulo, Control campo)
    {
        var linha = _proximaLinha++;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = rotulo,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 12, 8)
        }, 0, linha);

        if (campo is TextBox)
            campo.Dock = DockStyle.Fill;
        else
            campo.Anchor = AnchorStyles.Left;

        campo.Margin = new Padding(0, 4, 0, 4);
        layout.Controls.Add(campo, 1, linha);
    }

    private async void Salvar_Click(object? sender, EventArgs e)
    {
        // A confirmação de senha é regra de digitação (evita errar a senha ao cadastrar),
        // por isso fica na tela e não no serviço.
        if (_senha.Text != _confirmacao.Text)
        {
            MessageBox.Show(this, "A confirmação não confere com a senha.", "Dados inválidos",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _confirmacao.Clear();
            _confirmacao.Focus();
            return;
        }

        _btnSalvar.Enabled = false;

        try
        {
            var dto = new NovoUsuarioDto(_cpf.Text, _nome.Text, _email.Text, _senha.Text);

            UsuarioCriado = await _salvar(dto);
            DialogResult = DialogResult.OK;
        }
        catch (DominioException ex)
        {
            MessageBox.Show(this, ex.Message, "Dados inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Erro inesperado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _btnSalvar.Enabled = true;
        }
    }
}
