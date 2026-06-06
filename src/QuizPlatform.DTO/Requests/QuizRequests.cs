using System.ComponentModel.DataAnnotations;

namespace QuizPlatform.DTO.Requests
{
    
    public class CreateQuizRequest
    {
        [Required(ErrorMessage = "Название обязательно")]
        [MaxLength(200, ErrorMessage = "Максимум 200 символов")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Максимум 1000 символов")]
        public string? Description { get; set; }

        [MinLength(1, ErrorMessage = "Квиз должен содержать хотя бы 1 вопрос")]
        public List<CreatePuzzleRequest> Puzzles { get; set; } = new();
    }

    public class CreatePuzzleRequest
    {
        [Required(ErrorMessage = "Текст вопроса обязателен")]
        public string Text { get; set; } = string.Empty;

        public int OrderIndex { get; set; }

        [MinLength(2, ErrorMessage = "Минимум 2 варианта ответа")]
        public List<CreateAnswerOptionRequest> AnswerOptions { get; set; } = new();
    }

    public class CreateAnswerOptionRequest
    {
        [Required(ErrorMessage = "Текст ответа обязателен")]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}
