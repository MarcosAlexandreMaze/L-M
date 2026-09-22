using FluentValidation;
using LMStore.Application.DTOs.Admin.Cupons;

namespace LMStore.Application.Validators.Admin.Cupons;

public class CriarCupomRequestValidator : AbstractValidator<CriarCupomRequest>
{
    public CriarCupomRequestValidator()
    {
        RuleFor(r => r.Codigo).NotEmpty().MaximumLength(30);
        RuleFor(r => r.TipoDesconto).IsInEnum();
        RuleFor(r => r.Valor).GreaterThan(0);
        RuleFor(r => r.DataFim).GreaterThan(r => r.DataInicio);
        RuleFor(r => r.ValorMinimoCompra).GreaterThanOrEqualTo(0);
        RuleFor(r => r.LimiteUso).GreaterThan(0);
    }
}
