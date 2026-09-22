using FluentValidation;
using LMStore.Application.DTOs.Admin.Produtos;

namespace LMStore.Application.Validators.Admin.Produtos;

public class AtualizarProdutoRequestValidator : AbstractValidator<AtualizarProdutoRequest>
{
    public AtualizarProdutoRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(150);
        RuleFor(r => r.Descricao).NotEmpty().MaximumLength(2000);
    }
}
