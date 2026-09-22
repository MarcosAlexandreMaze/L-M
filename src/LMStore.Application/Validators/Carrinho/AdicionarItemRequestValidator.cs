using FluentValidation;
using LMStore.Application.DTOs.Carrinho;

namespace LMStore.Application.Validators.Carrinho;

public class AdicionarItemRequestValidator : AbstractValidator<AdicionarItemRequest>
{
    public AdicionarItemRequestValidator()
    {
        RuleFor(r => r.VariacaoProdutoId).NotEmpty();
        RuleFor(r => r.Quantidade).GreaterThan(0);
    }
}
