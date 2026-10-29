using AprenderMais.Dominio.Excecoes;

namespace AprenderMais.Api.Configuracao;

/// <summary>
/// Middleware que transforma exceções em respostas HTTP padronizadas: { "mensagem": "..." }.
///   DominioException            -> 400 (regra de negócio violada, ex.: "nota maior que a máxima")
///   UnauthorizedAccessException -> 401 (sessão inválida)
///   qualquer outra              -> 500 (erro inesperado, registrado no log)
/// </summary>
public class TratamentoDeErros
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeErros> _logger;

    public TratamentoDeErros(RequestDelegate proximo, ILogger<TratamentoDeErros> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (DominioException ex)
        {
            await ResponderAsync(contexto, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await ResponderAsync(contexto, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado em {Caminho}", contexto.Request.Path);
            await ResponderAsync(contexto, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static Task ResponderAsync(HttpContext contexto, int status, string mensagem)
    {
        contexto.Response.StatusCode = status;
        return contexto.Response.WriteAsJsonAsync(new { mensagem });
    }
}
