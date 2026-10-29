namespace AprenderMais.Api.Controllers;

// Corpos (JSON) recebidos pela API que não existem na camada de Aplicação.

public record LoginRequisicao(string Email, string Senha);

public record AlterarAtivoRequisicao(bool Ativo);

public record MatricularRequisicao(int IdConteudo);
