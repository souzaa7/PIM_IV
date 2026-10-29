namespace AprenderMais.Desktop;

/// <summary>
/// Cria tabelas (DataGridView) somente leitura com colunas definidas em código.
/// </summary>
internal static class GridHelper
{
    public static DataGridView Criar() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        BackgroundColor = SystemColors.Window,
        BorderStyle = BorderStyle.None
    };

    public static void Coluna(DataGridView grid, string propriedade, string titulo, float peso = 100f)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propriedade,
            HeaderText = titulo,
            FillWeight = peso
        });
    }

    /// <summary>
    /// Troca os dados da tabela mantendo a linha que estava selecionada (identificada pelo Id).
    /// Sem isso, cada recarga jogaria a seleção de volta para a primeira linha.
    /// </summary>
    public static void Preencher<T>(DataGridView grid, IEnumerable<T> itens, Func<T, int> obterId)
        where T : class
    {
        int? idSelecionado = grid.CurrentRow?.DataBoundItem is T atual
            ? obterId(atual)
            : null;

        grid.DataSource = itens.ToList();

        if (idSelecionado is null)
            return;

        foreach (DataGridViewRow linha in grid.Rows)
        {
            if (linha.DataBoundItem is T item && obterId(item) == idSelecionado)
            {
                // Definir a célula atual também rola a tabela até a linha, se necessário.
                grid.CurrentCell = linha.Cells[0];
                linha.Selected = true;
                return;
            }
        }
    }
}
