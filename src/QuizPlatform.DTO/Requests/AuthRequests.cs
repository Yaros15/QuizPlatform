using System.ComponentModel.DataAnnotations;

namespace QuizPlatform.DTO.Requests
{
    
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [MinLength(3, ErrorMessage = "Минимум 3 символа")]
        [MaxLength(50, ErrorMessage = "Максимум 50 символов")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Минимум 6 символов")]
        public string Password { get; set; } = string.Empty;
    }
    public class LoginRequest
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }
}
