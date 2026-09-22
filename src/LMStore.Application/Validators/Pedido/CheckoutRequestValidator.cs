using FluentValidation;
using LMStore.Application.DTOs.Pedido;

namespace LMStore.Application.Validators.Pedido;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(r => r.EnderecoEntregaId).NotEmpty();
        RuleFor(r => r.TokenPagamento).NotEmpty();
        RuleFor(r => r.FormaPagamento).IsInEnum();
    }
}
