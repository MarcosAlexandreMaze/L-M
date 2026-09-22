using LMStore.Api.Common;
using LMStore.Domain.Exceptions;

namespace LMStore.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception excecao)
        {
            await TratarExcecaoAsync(context, excecao);
        }
    }

    private async Task TratarExcecaoAsync(HttpContext context, Exception excecao)
    {
        var (status, mensagem) = excecao switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, excecao.Message),
            ConflictException => (StatusCodes.Status409Conflict, excecao.Message),
            DomainException => (StatusCodes.Status400BadRequest, excecao.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Não autenticado."),
            // Qualquer outra exceção é inesperada — a mensagem exposta ao cliente é
            // sempre genérica, nunca a mensagem real (que pode conter detalhe interno).
            // O stack trace completo só vai para o log, nunca para a resposta HTTP.
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(excecao, "Erro não tratado em {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("{Excecao} em {Metodo} {Caminho}: {Mensagem}",
                excecao.GetType().Name, context.Request.Method, context.Request.Path, excecao.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        await context.Response.WriteAsJsonAsync(new ErroPadraoResponse(status, mensagem, [mensagem]));
    }
}
