using FluentValidation;
using LMStore.Application.DTOs.Auth;

namespace LMStore.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().EmailAddress();
        RuleFor(r => r.Senha).NotEmpty();
    }
}
