using FluentValidation;
using LMStore.Application.DTOs.Admin.Categorias;

namespace LMStore.Application.Validators.Admin.Categorias;

public class RenomearCategoriaRequestValidator : AbstractValidator<RenomearCategoriaRequest>
{
    public RenomearCategoriaRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(100);
    }
}
