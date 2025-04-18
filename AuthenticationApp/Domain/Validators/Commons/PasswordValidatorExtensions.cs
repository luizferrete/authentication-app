using FluentValidation;

namespace AuthenticationApp.Domain.Validators.Commons
{
    public static class PasswordValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidatePassword<T>(this IRuleBuilder<T, string> ruleBuilder, string displayName)
        {
            return ruleBuilder
                .NotEmpty().WithMessage($"{displayName} é obrigatória")
                .MinimumLength(6).WithMessage($"{displayName} deve ter pelo menos 6 caracteres")
                .MaximumLength(20).WithMessage($"{displayName} deve ter no máximo 20 caracteres")
                .Matches(@"[A-Z]").WithMessage($"{displayName} deve conter pelo menos uma letra maiúscula")
                .Matches(@"[a-z]").WithMessage($"{displayName} deve conter pelo menos uma letra minúscula")
                .Matches(@"[0-9]").WithMessage($"{displayName} deve conter pelo menos um número")
                .Matches(@"[\W]").WithMessage($"{displayName} deve conter pelo menos um caractere especial");
        }
    }
}
