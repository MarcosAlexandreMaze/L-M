using FluentValidation;
using LMStore.Api.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LMStore.Api.Filters;

// Roda antes de toda action: para cada argumento (DTO) que tenha um IValidator<T>
// registrado no DI (ver LMStore.Application/DependencyInjection.cs), valida
// automaticamente. Evita repetir "validator.ValidateAsync(...)" em cada Controller —
// registrado uma vez, globalmente, em Program.cs.
public class ValidacaoAutomaticaFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argumento in context.ActionArguments.Values)
        {
            if (argumento is null)
                continue;

            var tipoDoValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());

            if (context.HttpContext.RequestServices.GetService(tipoDoValidador) is not IValidator validador)
                continue;

            var resultado = await validador.ValidateAsync(new ValidationContext<object>(argumento));

            if (!resultado.IsValid)
            {
                var erros = resultado.Errors.Select(e => e.ErrorMessage).ToArray();
                context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                    new ErroPadraoResponse(StatusCodes.Status400BadRequest, "Um ou mais campos são inválidos.", erros));
                return;
            }
        }

        await next();
    }
}
