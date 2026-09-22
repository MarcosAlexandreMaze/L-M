using FluentValidation;
using LMStore.Application.DTOs.Admin.Produtos;

namespace LMStore.Application.Validators.Admin.Produtos;

public class AplicarPromocaoRequestValidator : AbstractValidator<AplicarPromocaoRequest>
{
    public AplicarPromocaoRequestValidator()
    {
        RuleFor(r => r.PrecoPromocional).GreaterThan(0);
    }
}
