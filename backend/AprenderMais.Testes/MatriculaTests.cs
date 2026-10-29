using AprenderMais.Dominio.Entidades;
using Xunit;

namespace AprenderMais.Testes;

/// <summary>
/// A tela habilita o botão "Cancelar matrícula" a partir de MatriculaDto.PodeCancelar,
/// que precisa seguir a mesma regra de Matricula.Cancelar().
/// </summary>
public class MatriculaDtoTests
{
    [Fact]
    public async Task Somente_matricula_em_andamento_pode_ser_cancelada()
    {
        var ambiente = new Ambiente();
        var marina = await ambiente.ObterAsync<Aluno>("aluno@aprendermais.com");

        var lista = await ambiente.CriarMatriculaService().ListarDoAlunoAsync(marina);

        Assert.Contains(lista, m => m.Status == "Concluída" && !m.PodeCancelar);
        Assert.Contains(lista, m => m.Status == "Em andamento" && m.PodeCancelar);
    }

    [Fact]
    public async Task Matricula_cancelada_nao_pode_ser_cancelada_de_novo()
    {
        var ambiente = new Ambiente();
        var servico = ambiente.CriarMatriculaService();
        var bruno = await ambiente.ObterAsync<Aluno>("bruno@aprendermais.com");

        var criada = await servico.MatricularAsync(2, bruno);
        Assert.True(criada.PodeCancelar);

        await servico.CancelarAsync(criada.IdMatricula, bruno);
        var lista = await servico.ListarDoAlunoAsync(bruno);

        var cancelada = Assert.Single(lista, m => m.IdMatricula == criada.IdMatricula);
        Assert.Equal("Cancelada", cancelada.Status);
        Assert.False(cancelada.PodeCancelar);
    }
}
