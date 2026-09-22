using FluentValidation;
using LMStore.Application.DTOs.Admin.Pedidos;

namespace LMStore.Application.Validators.Admin.Pedidos;

public class MarcarComoEnviadoRequestValidator : AbstractValidator<MarcarComoEnviadoRequest>
{
    public MarcarComoEnviadoRequestValidator()
    {
        RuleFor(r => r.CodigoRastreio).NotEmpty().MaximumLength(50);
    }
}
