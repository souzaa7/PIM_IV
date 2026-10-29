using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Enums;
using AprenderMais.Dominio.Excecoes;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>UC04 (aluno realiza a avaliação) e UC06 (secretaria avalia a atividade).</summary>
public class RespostaService
{
    private readonly IRespostaRepositorio _respostas;
    private readonly IAtividadeRepositorio _atividades;
    private readonly IMatriculaRepositorio _matriculas;
    private readonly INotaRepositorio _notas;
    private readonly ICertificadoRepositorio _certificados;
    private readonly IRelogio _relogio;

    public RespostaService(
        IRespostaRepositorio respostas,
        IAtividadeRepositorio atividades,
        IMatriculaRepositorio matriculas,
        INotaRepositorio notas,
        ICertificadoRepositorio certificados,
        IRelogio relogio)
    {
        _respostas = respostas;
        _atividades = atividades;
        _matriculas = matriculas;
        _notas = notas;
        _certificados = certificados;
        _relogio = relogio;
    }

    /// <summary>UC04 - O aluno envia a resposta de uma atividade.</summary>
    public async Task EnviarAsync(EnviarRespostaDto dto, Usuario solicitante)
    {
        var aluno = Permissoes.ExigirAluno(solicitante);

        if (dto is null || string.IsNullOrWhiteSpace(dto.Texto))
            throw new DominioException("Escreva a sua resposta antes de enviar.");

        var atividade = await _atividades.ObterPorIdAsync(dto.IdAtividade);
        if (atividade is null || !atividade.Ativo)
            throw new DominioException("Atividade não encontrada.");

        var matricula = await _matriculas.ObterPorAlunoEConteudoAsync(aluno.IdAluno, atividade.IdConteudo);
        if (matricula is null || matricula.Status != StatusMatricula.EmAndamento)
            throw new DominioException("Você precisa estar matriculado no conteúdo para enviar a resposta.");

        if (atividade.PrazoEncerrado(_relogio.AgoraBrasilia))
            throw new DominioException("O prazo de entrega desta atividade já terminou.");

        if (await _respostas.ObterPorAlunoEAtividadeAsync(aluno.IdAluno, atividade.Id) is not null)
            throw new DominioException("Você já enviou a resposta desta atividade.");

        await _respostas.AdicionarAsync(new Resposta
        {
            IdAluno = aluno.IdAluno,
            IdAtividade = atividade.Id,
            TextoResposta = dto.Texto.Trim(),
            ArquivoUrl = string.IsNullOrWhiteSpace(dto.ArquivoUrl) ? null : dto.ArquivoUrl.Trim(),
            DataEnvio = _relogio.AgoraUtc
        });
    }

    /// <summary>UC06 - Lista as respostas que aguardam correção.</summary>
    public async Task<IReadOnlyList<RespostaPendenteDto>> ListarPendentesAsync(Usuario solicitante)
    {
        Permissoes.ExigirSecretaria(solicitante);

        var pendentes = await _respostas.ListarPendentesAsync();
        return pendentes
            .OrderBy(p => p.DataEnvio)
            .Select(p => new RespostaPendenteDto(
                p.IdResposta,
                p.NomeAluno,
                p.TituloAtividade,
                p.TituloConteudo,
                p.NotaMaxima,
                p.TextoResposta,
                p.ArquivoUrl,
                _relogio.ParaBrasilia(p.DataEnvio).ToString("dd/MM/yyyy HH:mm")))
            .ToList();
    }

    /// <summary>
    /// UC06 - A secretaria dá a nota. Depois disso o progresso da matrícula é recalculado
    /// e, se o aluno concluiu todas as atividades, o certificado é emitido.
    /// </summary>
    public async Task<AvaliacaoResultadoDto> AvaliarAsync(int idResposta, AvaliarRespostaDto dto, Usuario solicitante)
    {
        var secretaria = Permissoes.ExigirSecretaria(solicitante);

        if (dto is null)
            throw new DominioException("Informe a nota.");

        var resposta = await _respostas.ObterPorIdAsync(idResposta)
            ?? throw new DominioException("Resposta não encontrada.");

        if (await _notas.ObterPorRespostaAsync(idResposta) is not null)
            throw new DominioException("Esta resposta já foi avaliada.");

        var atividade = await _atividades.ObterPorIdAsync(resposta.IdAtividade)
            ?? throw new DominioException("Atividade não encontrada.");

        // 1) grava a nota
        var nota = new Nota
        {
            IdResposta = resposta.Id,
            IdSecretaria = secretaria.IdSecretaria,
            Valor = dto.Valor,
            Feedback = string.IsNullOrWhiteSpace(dto.Feedback) ? null : dto.Feedback.Trim(),
            DataAvaliacao = _relogio.AgoraUtc
        };
        nota.Validar(atividade.NotaMaxima);
        await _notas.AdicionarAsync(nota);

        // 2) atualiza o progresso da matrícula
        var matricula = await _matriculas.ObterPorAlunoEConteudoAsync(resposta.IdAluno, atividade.IdConteudo);
        if (matricula is null)
            return new AvaliacaoResultadoDto(0, false, null);

        var (avaliadas, total) = await _matriculas.ContarAtividadesAsync(resposta.IdAluno, atividade.IdConteudo);
        matricula.AtualizarProgresso(avaliadas, total, _relogio.AgoraUtc);
        await _matriculas.AtualizarAsync(matricula);

        // 3) emite o certificado se concluiu
        string? codigo = null;
        bool concluido = matricula.Status == StatusMatricula.Concluido;
        if (concluido)
        {
            var certificado = await _certificados.EmitirAsync(resposta.IdAluno, atividade.IdConteudo);
            codigo = certificado.CodigoValidacao;
        }

        return new AvaliacaoResultadoDto(matricula.Progresso, concluido, codigo);
    }
}
