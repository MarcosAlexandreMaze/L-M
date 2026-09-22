using FluentValidation;
using LMStore.Application.DTOs.Cupom;

namespace LMStore.Application.Validators.Cupom;

public class ValidarCupomRequestValidator : AbstractValidator<ValidarCupomRequest>
{
    public ValidarCupomRequestValidator()
    {
        RuleFor(r => r.Codigo).NotEmpty().MaximumLength(30);
        RuleFor(r => r.Subtotal).GreaterThanOrEqualTo(0);
    }
}
