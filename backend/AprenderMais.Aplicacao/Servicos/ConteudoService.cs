using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>Casos de uso UC03, UC07, UC08 e UC09 (conteúdos).</summary>
public class ConteudoService
{
    private readonly IConteudoRepositorio _conteudos;

    public ConteudoService(IConteudoRepositorio conteudos)
    {
        _conteudos = conteudos;
    }

    /// <summary>Catálogo visível para todos (inclusive visitantes).</summary>
    public async Task<IReadOnlyList<ConteudoDto>> ListarAtivosAsync()
    {
        var lista = await _conteudos.ListarAsync(somenteAtivos: true);
        return lista.Select(ParaDto).ToList();
    }

    public async Task<ConteudoDto> ObterAsync(int idConteudo)
    {
        var conteudo = await _conteudos.ObterPorIdAsync(idConteudo);
        if (conteudo is null || !conteudo.Ativo)
            throw new DominioException("Conteúdo não encontrado.");
        return ParaDto(conteudo);
    }

    /// <summary>Todos os conteúdos, inclusive inativos (gestão da secretaria).</summary>
    public async Task<IReadOnlyList<ConteudoDto>> ListarTodosAsync(Usuario solicitante)
    {
        Permissoes.ExigirSecretaria(solicitante);
        var lista = await _conteudos.ListarAsync(somenteAtivos: false);
        return lista.Select(ParaDto).ToList();
    }

    /// <summary>UC07 - Adicionar conteúdo.</summary>
    public async Task<ConteudoDto> CriarAsync(NovoConteudoDto dto, Usuario solicitante)
    {
        var secretaria = Permissoes.ExigirSecretaria(solicitante);

        var conteudo = new Conteudo { IdSecretaria = secretaria.IdSecretaria };
        PreencherDados(conteudo, dto);
        conteudo.Validar();

        var salvo = await _conteudos.AdicionarAsync(conteudo);
        return ParaDto(salvo);
    }

    /// <summary>UC08 - Editar conteúdo.</summary>
    public async Task<ConteudoDto> EditarAsync(int idConteudo, NovoConteudoDto dto, Usuario solicitante)
    {
        Permissoes.ExigirSecretaria(solicitante);

        var conteudo = await _conteudos.ObterPorIdAsync(idConteudo)
            ?? throw new DominioException("Conteúdo não encontrado.");

        PreencherDados(conteudo, dto);
        conteudo.Validar();

        await _conteudos.AtualizarAsync(conteudo);
        return ParaDto(conteudo);
    }

    /// <summary>UC09 - Desativar (ou reativar) conteúdo.</summary>
    public async Task AlterarAtivoAsync(int idConteudo, bool ativo, Usuario solicitante)
    {
        Permissoes.ExigirSecretaria(solicitante);

        var conteudo = await _conteudos.ObterPorIdAsync(idConteudo)
            ?? throw new DominioException("Conteúdo não encontrado.");

        conteudo.Ativo = ativo;
        await _conteudos.AtualizarAsync(conteudo);
    }

    private static void PreencherDados(Conteudo conteudo, NovoConteudoDto dto)
    {
        if (dto is null)
            throw new DominioException("Informe os dados do conteúdo.");

        conteudo.Titulo = (dto.Titulo ?? string.Empty).Trim();
        conteudo.Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim();
        conteudo.Categoria = (dto.Categoria ?? string.Empty).Trim();
        conteudo.Nivel = string.IsNullOrWhiteSpace(dto.Nivel) ? "Iniciante" : dto.Nivel.Trim();
        conteudo.Duracao = string.IsNullOrWhiteSpace(dto.Duracao) ? "0h" : dto.Duracao.Trim();
        conteudo.Aulas = dto.Aulas;
    }

    private static ConteudoDto ParaDto(Conteudo c) =>
        new(c.Id, c.Titulo, c.Descricao, c.Categoria, c.Nivel, c.Duracao, c.Aulas, c.Ativo);
}
