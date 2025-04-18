using AuthenticationApp.Domain.Request;
using FluentValidation;

namespace AuthenticationApp.Domain.Validators.Request
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("O nome de usuário é obrigatório.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("A senha é obrigatória.");
        }
    }
}
