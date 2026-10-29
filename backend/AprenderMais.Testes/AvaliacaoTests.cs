using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using Xunit;

namespace AprenderMais.Testes;

/// <summary>Fluxo completo: aluno envia resposta (UC04) -> secretaria corrige (UC06) -> progresso e certificado.</summary>
public class AvaliacaoTests
{
    private const string Marina = "aluno@aprendermais.com";
    private const string Bruno = "bruno@aprendermais.com";
    private const string Secretaria = "secretaria@aprendermais.com";

    [Fact]
    public async Task Aluno_envia_resposta_e_ela_aparece_como_pendente()
    {
        var ambiente = new Ambiente();
        var marina = await ambiente.ObterAsync<Aluno>(Marina);
        var secretaria = await ambiente.ObterAsync<Secretaria>(Secretaria);

        // Atividade 4 = "Protótipo de tela acessível" (UX/UI, prazo 04/12)
        await ambiente.CriarRespostaService().EnviarAsync(new EnviarRespostaDto(4, "Minha tela acessível.", null), marina);

        var pendentes = await ambiente.CriarRespostaService().ListarPendentesAsync(secretaria);
        Assert.Contains(pendentes, p => p.Atividade == "Protótipo de tela acessível" && p.Aluno == "Marina Costa");
    }

    [Fact]
    public async Task Resposta_fora_do_prazo_e_rejeitada()
    {
        var ambiente = new Ambiente();
        var bruno = await ambiente.ObterAsync<Aluno>(Bruno);

        // Atividade 2 tem prazo 20/10 e o relógio dos testes está em 29/10.
        await Assert.ThrowsAsync<DominioException>(() =>
            ambiente.CriarRespostaService().EnviarAsync(new EnviarRespostaDto(2, "Atrasada", null), bruno));
    }

    [Fact]
    public async Task Aluno_sem_matricula_nao_envia_resposta()
    {
        var ambiente = new Ambiente();
        var bruno = await ambiente.ObterAsync<Aluno>(Bruno);

        // Bruno não está matriculado em UX/UI (atividade 4).
        await Assert.ThrowsAsync<DominioException>(() =>
            ambiente.CriarRespostaService().EnviarAsync(new EnviarRespostaDto(4, "Oi", null), bruno));
    }

    [Fact]
    public async Task Nota_maior_que_a_maxima_e_rejeitada()
    {
        var ambiente = new Ambiente();
        var secretaria = await ambiente.ObterAsync<Secretaria>(Secretaria);

        // Resposta 4 (Bruno) está pendente.
        await Assert.ThrowsAsync<DominioException>(() =>
            ambiente.CriarRespostaService().AvaliarAsync(4, new AvaliarRespostaDto(11m, null), secretaria));
    }

    [Fact]
    public async Task Correcao_atualiza_o_progresso_do_aluno()
    {
        var ambiente = new Ambiente();
        var secretaria = await ambiente.ObterAsync<Secretaria>(Secretaria);

        // Bruno: Engenharia de Software tem 2 atividades; ele respondeu 1 -> 50%.
        var resultado = await ambiente.CriarRespostaService().AvaliarAsync(4, new AvaliarRespostaDto(8m, "Bom"), secretaria);

        Assert.Equal(50m, resultado.ProgressoDoAluno);
        Assert.False(resultado.ConteudoConcluido);
        Assert.Null(resultado.CodigoCertificado);
    }

    [Fact]
    public async Task Concluir_todas_as_atividades_emite_certificado()
    {
        var ambiente = new Ambiente();
        var marina = await ambiente.ObterAsync<Aluno>(Marina);
        var secretaria = await ambiente.ObterAsync<Secretaria>(Secretaria);
        var respostas = ambiente.CriarRespostaService();

        // UX/UI: atividade 3 já avaliada; falta a 4.
        await respostas.EnviarAsync(new EnviarRespostaDto(4, "Tela com contraste 4.5:1.", null), marina);
        var pendente = (await respostas.ListarPendentesAsync(secretaria)).Single(p => p.Aluno == "Marina Costa");

        var resultado = await respostas.AvaliarAsync(pendente.IdResposta, new AvaliarRespostaDto(10m, "Excelente"), secretaria);

        Assert.True(resultado.ConteudoConcluido);
        Assert.Equal(100m, resultado.ProgressoDoAluno);
        Assert.NotNull(resultado.CodigoCertificado);

        var certificados = await ambiente.CriarCertificadoService().ListarDoAlunoAsync(marina);
        Assert.Contains(certificados, c => c.Conteudo == "UX/UI Design");
    }

    [Fact]
    public async Task Resposta_ja_avaliada_nao_recebe_segunda_nota()
    {
        var ambiente = new Ambiente();
        var secretaria = await ambiente.ObterAsync<Secretaria>(Secretaria);

        await Assert.ThrowsAsync<DominioException>(() =>
            ambiente.CriarRespostaService().AvaliarAsync(1, new AvaliarRespostaDto(5m, null), secretaria));
    }

    [Fact]
    public async Task Boletim_mostra_somente_respostas_corrigidas()
    {
        var ambiente = new Ambiente();
        var marina = await ambiente.ObterAsync<Aluno>(Marina);

        var boletim = await ambiente.CriarNotaService().ListarDoAlunoAsync(marina);

        Assert.Equal(3, boletim.Count);
        Assert.Contains(boletim, n => n.Atividade == "Modelagem de requisitos" && n.Valor == 9.5m);
    }

    [Fact]
    public async Task Atividades_do_aluno_mostram_a_situacao_de_cada_entrega()
    {
        var ambiente = new Ambiente();
        var marina = await ambiente.ObterAsync<Aluno>(Marina);

        var atividades = await ambiente.CriarAtividadeService().ListarDoAlunoAsync(marina);

        Assert.Contains(atividades, a => a.Titulo == "Modelagem de requisitos" && a.Situacao == "Avaliada");
        Assert.Contains(atividades, a => a.Titulo == "Protótipo de tela acessível" && a.Situacao == "Pendente" && a.PodeEnviar);
    }

    [Fact]
    public async Task Certificado_e_validado_pelo_codigo()
    {
        var ambiente = new Ambiente();

        var certificado = await ambiente.CriarCertificadoService().ValidarAsync("am-2026-7f3a9c21");

        Assert.Equal("Marina Costa", certificado.Aluno);
        Assert.Equal("Engenharia de Software", certificado.Conteudo);
    }
}
