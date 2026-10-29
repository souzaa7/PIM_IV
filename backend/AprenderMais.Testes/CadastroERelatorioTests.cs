using AprenderMais.Aplicacao.Dtos;
using AprenderMais.Dominio.Entidades;
using AprenderMais.Dominio.Excecoes;
using Xunit;

namespace AprenderMais.Testes;

public class CadastroERelatorioTests
{
    [Fact]
    public async Task Visitante_se_cadastra_como_aluno_e_consegue_entrar()
    {
        var ambiente = new Ambiente();
        var dto = new NovoUsuarioDto("123.987.654-00", "Lucas Teste", "Lucas@Teste.com", "senhaforte1");

        var criado = await ambiente.CriarUsuarioService().CadastrarAlunoAsync(dto);
        var logado = await ambiente.CriarAutenticacao().EntrarAsync("lucas@teste.com", "senhaforte1");

        Assert.Equal("Aluno", criado.Perfil);
        Assert.IsType<Aluno>(logado);
    }

    [Fact]
    public async Task Cadastro_com_email_repetido_e_rejeitado()
    {
        var ambiente = new Ambiente();
        var dto = new NovoUsuarioDto("12398765400", "Outra Marina", "aluno@aprendermais.com", "senhaforte1");

        await Assert.ThrowsAsync<DominioException>(() => ambiente.CriarUsuarioService().CadastrarAlunoAsync(dto));
    }

    [Fact]
    public async Task Secretaria_edita_conteudo()
    {
        var ambiente = new Ambiente();
        var secretaria = await ambiente.ObterAsync<Secretaria>("secretaria@aprendermais.com");
        var dto = new NovoConteudoDto("Microsoft 365 Avançado", "Nova descrição", "Produtividade", "Avançado", "12h", 20);

        var editado = await ambiente.CriarConteudoService().EditarAsync(3, dto, secretaria);

        Assert.Equal("Microsoft 365 Avançado", editado.Titulo);
        Assert.Equal("Avançado", editado.Nivel);
    }

    [Fact]
    public async Task Desativar_conteudo_desativa_as_atividades()
    {
        var ambiente = new Ambiente();
        var secretaria = await ambiente.ObterAsync<Secretaria>("secretaria@aprendermais.com");

        await ambiente.CriarConteudoService().AlterarAtivoAsync(3, false, secretaria);

        var atividades = await ambiente.CriarAtividadeService().ListarPorConteudoAsync(3);
        Assert.Empty(atividades);
    }

    [Fact]
    public async Task Resumo_do_administrador_conta_os_dados_de_demonstracao()
    {
        var ambiente = new Ambiente();
        var admin = await ambiente.ObterAsync<Administrador>("admin@aprendermais.com");

        var resumo = await ambiente.CriarRelatorioService().ResumoAsync(admin);

        Assert.Equal(3, resumo.TotalAlunos);
        Assert.Equal(1, resumo.RespostasPendentes);
        Assert.Equal(1, resumo.CertificadosEmitidos);
    }

    [Fact]
    public async Task Aluno_nao_acessa_relatorios()
    {
        var ambiente = new Ambiente();
        var marina = await ambiente.ObterAsync<Aluno>("aluno@aprendermais.com");

        await Assert.ThrowsAsync<DominioException>(() => ambiente.CriarRelatorioService().ResumoAsync(marina));
    }
}
