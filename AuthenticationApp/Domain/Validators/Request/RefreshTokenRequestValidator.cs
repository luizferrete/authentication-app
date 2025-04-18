using AuthenticationApp.Domain.Request;
using FluentValidation;

namespace AuthenticationApp.Domain.Validators.Request
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator() { 
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("O token de atualização não pode ser vazio.")
                .NotNull()
                .WithMessage("O token de atualização não pode ser nulo.")
                .Length(10, 50)
                .WithMessage("Tamanho de token inválido.");
        }
    }
}
