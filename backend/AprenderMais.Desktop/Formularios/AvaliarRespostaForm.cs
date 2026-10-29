using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Dominio.Excecoes;

namespace AprenderMais.Desktop.Formularios;

/// <summary>
/// Janela de correção (UC06): mostra a resposta do aluno e pede nota + feedback.
/// Só fecha com sucesso quando o serviço aceita a nota.
/// </summary>
public class AvaliarRespostaForm : Form
{
    private readonly Func<AvaliarRespostaDto, Task<AvaliacaoResultadoDto>> _salvar;

    private readonly NumericUpDown _nota = new();
    private readonly TextBox _feedback = new();
    private readonly Button _btnSalvar = new();
    private readonly Button _btnCancelar = new();

    public AvaliacaoResultadoDto? Resultado { get; private set; }

    public AvaliarRespostaForm(RespostaPendenteDto resposta, Func<AvaliarRespostaDto, Task<AvaliacaoResultadoDto>> salvar)
    {
        _salvar = salvar;

        Text = "Corrigir atividade";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 460);

        var cabecalho = new Label
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(16, 12, 16, 0),
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Text = $"{resposta.Aluno} - {resposta.Atividade}\n{resposta.Conteudo} (enviada em {resposta.DataEnvio})"
        };

        var texto = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Text = resposta.Texto + (resposta.ArquivoUrl is null ? "" : $"\r\n\r\nArquivo: {resposta.ArquivoUrl}")
        };

        _nota.DecimalPlaces = 1;
        _nota.Increment = 0.5m;
        _nota.Minimum = 0;
        _nota.Maximum = resposta.NotaMaxima;
        _nota.Value = 0;

        _feedback.Multiline = true;
        _feedback.Height = 70;
        _feedback.Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.Controls.Add(new Label { Text = "Resposta", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 0);
        layout.Controls.Add(texto, 1, 0);
        layout.Controls.Add(new Label { Text = $"Nota (0 a {resposta.NotaMaxima:0.#})", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        layout.Controls.Add(_nota, 1, 1);
        layout.Controls.Add(new Label { Text = "Feedback", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 2);
        layout.Controls.Add(_feedback, 1, 2);

        _btnSalvar.Text = "Salvar nota";
        _btnSalvar.Size = new Size(110, 32);
        _btnSalvar.Click += Salvar_Click;

        _btnCancelar.Text = "Cancelar";
        _btnCancelar.Size = new Size(100, 32);
        _btnCancelar.DialogResult = DialogResult.Cancel;

        var botoes = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 52,
            Padding = new Padding(8)
        };
        botoes.Controls.Add(_btnCancelar);
        botoes.Controls.Add(_btnSalvar);

        Controls.Add(layout);
        Controls.Add(botoes);
        Controls.Add(cabecalho);

        AcceptButton = _btnSalvar;
        CancelButton = _btnCancelar;
    }

    private async void Salvar_Click(object? sender, EventArgs e)
    {
        _btnSalvar.Enabled = false;
        try
        {
            Resultado = await _salvar(new AvaliarRespostaDto(_nota.Value, _feedback.Text));
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (DominioException ex)
        {
            MessageBox.Show(this, ex.Message, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            _btnSalvar.Enabled = true;
        }
    }
}
