using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>Atividades (avaliações) dos conteúdos - parte dos UC03, UC04 e UC07.</summary>
public class AtividadeService
{
    private readonly IAtividadeRepositorio _atividades;
    private readonly IConteudoRepositorio _conteudos;
    private readonly IMatriculaRepositorio _matriculas;
    private readonly IRespostaRepositorio _respostas;
    private readonly INotaRepositorio _notas;
    private readonly IRelogio _relogio;

    public AtividadeService(
        IAtividadeRepositorio atividades,
        IConteudoRepositorio conteudos,
        IMatriculaRepositorio matriculas,
        IRespostaRepositorio respostas,
        INotaRepositorio notas,
        IRelogio relogio)
    {
        _atividades = atividades;
        _conteudos = conteudos;
        _matriculas = matriculas;
        _respostas = respostas;
        _notas = notas;
        _relogio = relogio;
    }

    /// <summary>Atividades ativas de um conteúdo.</summary>
    public async Task<IReadOnlyList<AtividadeDto>> ListarPorConteudoAsync(int idConteudo)
    {
        var lista = await _atividades.ListarPorConteudoAsync(idConteudo, somenteAtivas: true);
        return lista.Select(ParaDto).ToList();
    }

    /// <summary>Secretaria cria uma atividade para um conteúdo.</summary>
    public async Task<AtividadeDto> CriarAsync(NovaAtividadeDto dto, Usuario solicitante)
    {
        Permissoes.ExigirSecretaria(solicitante);

        if (dto is null)
            throw new DominioException("Informe os dados da atividade.");

        var conteudo = await _conteudos.ObterPorIdAsync(dto.IdConteudo);
        if (conteudo is null || !conteudo.Ativo)
            throw new DominioException("Conteúdo inexistente ou inativo.");

        if (dto.DataEntrega <= _relogio.AgoraBrasilia)
            throw new DominioException("A data de entrega deve ser no futuro.");

        var atividade = new Atividade
        {
            IdConteudo = dto.IdConteudo,
            Titulo = (dto.Titulo ?? string.Empty).Trim(),
            Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim(),
            DataEntrega = dto.DataEntrega,
            NotaMaxima = dto.NotaMaxima <= 0 ? 10m : dto.NotaMaxima,
            DataCriacao = _relogio.AgoraUtc
        };
        atividade.Validar();

        var salva = await _atividades.AdicionarAsync(atividade);
        return ParaDto(salva);
    }

    /// <summary>
    /// Todas as atividades dos conteúdos em que o aluno está matriculado,
    /// com a situação da entrega dele (pendente, enviada, avaliada...).
    /// </summary>
    public async Task<IReadOnlyList<AtividadeDoAlunoDto>> ListarDoAlunoAsync(Usuario solicitante)
    {
        var aluno = Permissoes.ExigirAluno(solicitante);
        var resultado = new List<AtividadeDoAlunoDto>();

        var matriculas = await _matriculas.ListarPorAlunoAsync(aluno.IdAluno);

        foreach (var matricula in matriculas.Where(m => m.Status != StatusMatricula.Cancelado))
        {
            var conteudo = await _conteudos.ObterPorIdAsync(matricula.IdConteudo);
            if (conteudo is null || !conteudo.Ativo)
                continue;

            var atividades = await _atividades.ListarPorConteudoAsync(conteudo.Id, somenteAtivas: true);

            foreach (var atividade in atividades)
            {
                var resposta = await _respostas.ObterPorAlunoEAtividadeAsync(aluno.IdAluno, atividade.Id);
                var nota = resposta is null ? null : await _notas.ObterPorRespostaAsync(resposta.Id);
                bool prazoEncerrado = atividade.PrazoEncerrado(_relogio.AgoraBrasilia);

                string situacao =
                    nota is not null ? "Avaliada" :
                    resposta is not null ? "Aguardando correção" :
                    prazoEncerrado ? "Prazo encerrado" :
                    "Pendente";

                bool podeEnviar = resposta is null
                                  && !prazoEncerrado
                                  && matricula.Status == StatusMatricula.EmAndamento;

                resultado.Add(new AtividadeDoAlunoDto(
                    atividade.Id,
                    conteudo.Id,
                    conteudo.Titulo,
                    atividade.Titulo,
                    atividade.Descricao,
                    atividade.DataEntrega.ToString("dd/MM/yyyy HH:mm"),
                    atividade.NotaMaxima,
                    situacao,
                    podeEnviar,
                    nota?.Valor,
                    nota?.Feedback));
            }
        }

        return resultado.OrderBy(a => a.Situacao != "Pendente").ThenBy(a => a.Conteudo).ToList();
    }

    private AtividadeDto ParaDto(Atividade a) =>
        new(a.Id,
            a.IdConteudo,
            a.Titulo,
            a.Descricao,
            a.DataEntrega.ToString("dd/MM/yyyy HH:mm"),
            a.NotaMaxima,
            a.PrazoEncerrado(_relogio.AgoraBrasilia));
}
