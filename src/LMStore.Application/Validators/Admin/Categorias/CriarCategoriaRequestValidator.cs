using FluentValidation;
using LMStore.Application.DTOs.Admin.Categorias;

namespace LMStore.Application.Validators.Admin.Categorias;

public class CriarCategoriaRequestValidator : AbstractValidator<CriarCategoriaRequest>
{
    public CriarCategoriaRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(100);
    }
}
