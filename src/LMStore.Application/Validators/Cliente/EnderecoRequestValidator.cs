using FluentValidation;
using LMStore.Application.DTOs.Cliente;

namespace LMStore.Application.Validators.Cliente;

public class EnderecoRequestValidator : AbstractValidator<EnderecoRequest>
{
    public EnderecoRequestValidator()
    {
        RuleFor(r => r.Cep).NotEmpty();
        RuleFor(r => r.Logradouro).NotEmpty().MaximumLength(200);
        RuleFor(r => r.Numero).NotEmpty().MaximumLength(20);
        RuleFor(r => r.Bairro).NotEmpty().MaximumLength(100);
        RuleFor(r => r.Cidade).NotEmpty().MaximumLength(100);
        RuleFor(r => r.Estado).NotEmpty().Length(2);
    }
}
