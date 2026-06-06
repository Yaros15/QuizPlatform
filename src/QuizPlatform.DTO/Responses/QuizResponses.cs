

namespace QuizPlatform.DTO.Responses
{
    public class QuizResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<PuzzleResponse> Puzzles { get; set; } = new();
    }
    public class PuzzleResponse
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public List<AnswerOptionResponse> AnswerOptions { get; set; } = new();
    }

    public class AnswerOptionResponse
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    // Ответ с результатом прохождения
    public class QuizResultResponse
    {
        public long Id { get; set; }
        public long QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }              // Количество правильных ответов
        public int TotalQuestions { get; set; }     // Всего вопросов
        public double Percentage { get; set; }      // Процент правильных
        public DateTime CompletedAt { get; set; }
    }

    // Статистика квиза (для автора)
    public class QuizStatisticsResponse
    {
        public long QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }      // Сколько раз проходили
        public double AverageScore { get; set; }    // Средний балл
        public int HighScore { get; set; }          // Лучший результат
        public int TotalQuestions { get; set; }
    }

    // Мои результаты (список)
    public class UserQuizResultResponse
    {
        public long ResultId { get; set; }
        public long QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public double Percentage { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
