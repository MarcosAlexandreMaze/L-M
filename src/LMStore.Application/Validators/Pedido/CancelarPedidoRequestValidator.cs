using FluentValidation;
using LMStore.Application.DTOs.Pedido;

namespace LMStore.Application.Validators.Pedido;

public class CancelarPedidoRequestValidator : AbstractValidator<CancelarPedidoRequest>
{
    public CancelarPedidoRequestValidator()
    {
        RuleFor(r => r.Motivo).NotEmpty().MaximumLength(500);
    }
}
