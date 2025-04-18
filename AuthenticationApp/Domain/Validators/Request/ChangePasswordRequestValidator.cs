using AuthenticationApp.Domain.Request;
using AuthenticationApp.Domain.Validators.Commons;
using FluentValidation;

namespace AuthenticationApp.Domain.Validators.Request
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty()
                .WithMessage("A senha antiga não pode estar vazia");

            RuleFor(x => x.NewPassword)
                .ValidatePassword("A senha nova")
                .NotEqual(x => x.OldPassword).WithMessage("A nova senha não pode ser igual à senha atual");
        }
    }
}
