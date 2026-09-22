using FluentValidation;
using LMStore.Application.DTOs.Admin.Estoque;

namespace LMStore.Application.Validators.Admin.Estoque;

public class ReporEstoqueRequestValidator : AbstractValidator<ReporEstoqueRequest>
{
    public ReporEstoqueRequestValidator()
    {
        RuleFor(r => r.Quantidade).GreaterThan(0);
        RuleFor(r => r.Motivo).NotEmpty().MaximumLength(300);
    }
}
