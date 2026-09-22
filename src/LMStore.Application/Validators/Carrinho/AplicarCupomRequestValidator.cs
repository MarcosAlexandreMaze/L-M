using FluentValidation;
using LMStore.Application.DTOs.Carrinho;

namespace LMStore.Application.Validators.Carrinho;

public class AplicarCupomRequestValidator : AbstractValidator<AplicarCupomRequest>
{
    public AplicarCupomRequestValidator()
    {
        RuleFor(r => r.Codigo).NotEmpty().MaximumLength(30);
    }
}
