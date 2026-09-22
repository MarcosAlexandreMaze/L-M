using FluentValidation;
using LMStore.Application.DTOs.Admin.Produtos;

namespace LMStore.Application.Validators.Admin.Produtos;

public class AdicionarVariacaoRequestValidator : AbstractValidator<AdicionarVariacaoRequest>
{
    public AdicionarVariacaoRequestValidator()
    {
        RuleFor(r => r.Sku).NotEmpty().MaximumLength(50);
        RuleFor(r => r.Tamanho).NotEmpty().MaximumLength(20);
        RuleFor(r => r.Cor).NotEmpty().MaximumLength(40);
        RuleFor(r => r.PrecoAdicional).GreaterThanOrEqualTo(0).When(r => r.PrecoAdicional is not null);
        RuleFor(r => r.QuantidadeInicial).GreaterThanOrEqualTo(0);
    }
}
