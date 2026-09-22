using FluentValidation;
using LMStore.Application.DTOs.Admin.Produtos;

namespace LMStore.Application.Validators.Admin.Produtos;

public class CriarTenisRequestValidator : AbstractValidator<CriarTenisRequest>
{
    public CriarTenisRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(150);
        RuleFor(r => r.Descricao).NotEmpty().MaximumLength(2000);
        RuleFor(r => r.MarcaId).NotEmpty();
        RuleFor(r => r.CategoriaId).NotEmpty();
        RuleFor(r => r.Preco).GreaterThan(0);
        RuleFor(r => r.Genero).IsInEnum();
        RuleFor(r => r.IndicacaoUso).IsInEnum();
        RuleFor(r => r.TipoPisada).IsInEnum().When(r => r.TipoPisada is not null);
    }
}
