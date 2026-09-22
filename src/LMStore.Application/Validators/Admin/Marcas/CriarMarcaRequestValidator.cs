using FluentValidation;
using LMStore.Application.DTOs.Admin.Marcas;

namespace LMStore.Application.Validators.Admin.Marcas;

public class CriarMarcaRequestValidator : AbstractValidator<CriarMarcaRequest>
{
    public CriarMarcaRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(100);
    }
}
