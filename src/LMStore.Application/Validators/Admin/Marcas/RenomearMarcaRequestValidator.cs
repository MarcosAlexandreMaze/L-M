using FluentValidation;
using LMStore.Application.DTOs.Admin.Marcas;

namespace LMStore.Application.Validators.Admin.Marcas;

public class RenomearMarcaRequestValidator : AbstractValidator<RenomearMarcaRequest>
{
    public RenomearMarcaRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(100);
    }
}
