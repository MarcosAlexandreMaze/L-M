using FluentValidation;
using LMStore.Application.DTOs.Admin.Estoque;

namespace LMStore.Application.Validators.Admin.Estoque;

public class AjustarEstoqueRequestValidator : AbstractValidator<AjustarEstoqueRequest>
{
    public AjustarEstoqueRequestValidator()
    {
        RuleFor(r => r.NovaQuantidade).GreaterThanOrEqualTo(0);
        RuleFor(r => r.Motivo).NotEmpty().MaximumLength(300);
    }
}
