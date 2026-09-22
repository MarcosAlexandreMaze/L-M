using FluentValidation;
using LMStore.Application.DTOs.Carrinho;

namespace LMStore.Application.Validators.Carrinho;

public class AlterarQuantidadeRequestValidator : AbstractValidator<AlterarQuantidadeRequest>
{
    public AlterarQuantidadeRequestValidator()
    {
        RuleFor(r => r.Quantidade).GreaterThan(0);
    }
}
