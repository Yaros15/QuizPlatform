using FluentValidation;
using QuizPlatform.DTO.Requests;

namespace QuizPlatform.API.Validators
{

    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Имя пользователя обязательно")
                .MinimumLength(3).WithMessage("Минимум 3 символа")
                .MaximumLength(50).WithMessage("Максимум 50 символов");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email обязателен")
                .EmailAddress().WithMessage("Некорректный Email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль обязателен")
                .MinimumLength(6).WithMessage("Минимум 6 символов")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Пароль должен содержать заглавные, строчные буквы и цифры");
        }
    }
}
