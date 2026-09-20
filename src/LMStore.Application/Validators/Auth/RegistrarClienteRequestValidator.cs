using FluentValidation;
using LMStore.Application.DTOs.Auth;

namespace LMStore.Application.Validators.Auth;

public class RegistrarClienteRequestValidator : AbstractValidator<RegistrarClienteRequest>
{
    public RegistrarClienteRequestValidator()
    {
        RuleFor(r => r.Nome).NotEmpty().MaximumLength(150);

        RuleFor(r => r.Email).NotEmpty().EmailAddress().MaximumLength(256);

        // Regra de forma (tamanho, presença) fica aqui; a regra de negócio de fato
        // (dígito verificador, e-mail/CPF duplicado) já é responsabilidade do VO/Domain —
        // não duplicamos a lógica, só barramos entradas obviamente malformadas mais cedo.
        RuleFor(r => r.Senha)
            .NotEmpty()
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter ao menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter ao menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.");

        RuleFor(r => r.Cpf).NotEmpty();
        RuleFor(r => r.Telefone).NotEmpty().MaximumLength(20);

        RuleFor(r => r.DataNascimento)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("A data de nascimento não pode estar no futuro.");
    }
}
