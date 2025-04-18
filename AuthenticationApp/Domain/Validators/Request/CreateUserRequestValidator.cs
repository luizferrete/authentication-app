using AuthenticationApp.Domain.Request;
using AuthenticationApp.Domain.Validators.Commons;
using FluentValidation;

namespace AuthenticationApp.Domain.Validators.Request
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        public CreateUserRequestValidator() { 
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório")
                .EmailAddress().WithMessage("O e-mail deve ser válido");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório")
                .MinimumLength(3).WithMessage("O nome de usuário deve ter pelo menos 3 caracteres")
                .MaximumLength(20).WithMessage("O nome de usuário deve ter no máximo 20 caracteres");

            RuleFor(x => x.Password)
                .ValidatePassword("A senha");
        }
    }
}
