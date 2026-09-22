using FluentValidation;
using LMStore.Application.DTOs.Admin.Administradores;

namespace LMStore.Application.Validators.Admin.Administradores;

public class CriarAdministradorRequestValidator : AbstractValidator<CriarAdministradorRequest>
{
    public CriarAdministradorRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(150);
        RuleFor(r => r.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(r => r.Senha)
            .NotEmpty()
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");

        RuleFor(r => r.Cargo).MaximumLength(100);
    }
}
