using FluentValidation;
using LMStore.Application.DTOs.Pedido;

namespace LMStore.Application.Validators.Pedido;

public class PagarNovamenteRequestValidator : AbstractValidator<PagarNovamenteRequest>
{
    public PagarNovamenteRequestValidator()
    {
        RuleFor(r => r.TokenPagamento).NotEmpty();
    }
}
