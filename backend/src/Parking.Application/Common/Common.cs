using FluentValidation;

namespace Parking.Application.Common;

public enum SortDirection
{
    Asc,
    Desc,
}

public static class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(MinLength).WithMessage($"A senha deve ter pelo menos {MinLength} caracteres.")
            .MaximumLength(MaxLength).WithMessage($"A senha não pode exceder {MaxLength} caracteres.")
            .Must(p => p is not null && p.Any(char.IsLetter) && p.Any(char.IsDigit))
            .WithMessage("A senha deve conter letras e números.");
}
