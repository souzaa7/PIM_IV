using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Aplicacao.Interfaces;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Repositorios;

namespace AprenderMais.Aplicacao.Servicos;

/// <summary>UC05 - Acessar notas (boletim do aluno).</summary>
public class NotaService
{
    private readonly IRespostaRepositorio _respostas;
    private readonly INotaRepositorio _notas;
    private readonly IAtividadeRepositorio _atividades;
    private readonly IConteudoRepositorio _conteudos;
    private readonly IRelogio _relogio;

    public NotaService(
        IRespostaRepositorio respostas,
        INotaRepositorio notas,
        IAtividadeRepositorio atividades,
        IConteudoRepositorio conteudos,
        IRelogio relogio)
    {
        _respostas = respostas;
        _notas = notas;
        _atividades = atividades;
        _conteudos = conteudos;
        _relogio = relogio;
    }

    public async Task<IReadOnlyList<NotaDto>> ListarDoAlunoAsync(Usuario solicitante)
    {
        var aluno = Permissoes.ExigirAluno(solicitante);
        var boletim = new List<NotaDto>();

        foreach (var resposta in await _respostas.ListarPorAlunoAsync(aluno.IdAluno))
        {
            var nota = await _notas.ObterPorRespostaAsync(resposta.Id);
            if (nota is null)
                continue; // ainda não corrigida

            var atividade = await _atividades.ObterPorIdAsync(resposta.IdAtividade);
            var conteudo = atividade is null ? null : await _conteudos.ObterPorIdAsync(atividade.IdConteudo);

            boletim.Add(new NotaDto(
                conteudo?.Titulo ?? "-",
                atividade?.Titulo ?? "-",
                nota.Valor,
                atividade?.NotaMaxima ?? 10m,
                nota.Feedback,
                _relogio.ParaBrasilia(nota.DataAvaliacao).ToString("dd/MM/yyyy")));
        }

        return boletim.OrderBy(n => n.Conteudo).ThenBy(n => n.Atividade).ToList();
    }
}
