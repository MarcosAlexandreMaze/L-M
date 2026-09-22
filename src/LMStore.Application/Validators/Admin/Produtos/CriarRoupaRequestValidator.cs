using FluentValidation;
using LMStore.Application.DTOs.Admin.Produtos;

namespace LMStore.Application.Validators.Admin.Produtos;

public class CriarRoupaRequestValidator : AbstractValidator<CriarRoupaRequest>
{
    public CriarRoupaRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(150);
        RuleFor(r => r.Descricao).NotEmpty().MaximumLength(2000);
        RuleFor(r => r.MarcaId).NotEmpty();
        RuleFor(r => r.CategoriaId).NotEmpty();
        RuleFor(r => r.Preco).GreaterThan(0);
        RuleFor(r => r.Genero).IsInEnum();
        RuleFor(r => r.TipoRoupa).IsInEnum();
        RuleFor(r => r.Material).NotEmpty().MaximumLength(100);
    }
}
