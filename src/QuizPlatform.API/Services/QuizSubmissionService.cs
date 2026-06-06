using AutoMapper;
using global::QuizPlatform.API.Data;
using global::QuizPlatform.Domain.Entities;
using global::QuizPlatform.DTO.Requests;
using global::QuizPlatform.DTO.Responses;
using Microsoft.EntityFrameworkCore;

namespace QuizPlatform.API.Services
{

    public interface IQuizSubmissionService
    {
        // Пройти квиз
        Task<QuizResultResponse> SubmitQuizAsync(long quizId, SubmitQuizRequest request, long userId);

        // Получить мои результаты
        Task<List<UserQuizResultResponse>> GetUserResultsAsync(long userId);

        // Получить статистику квиза (для автора)
        Task<QuizStatisticsResponse?> GetQuizStatisticsAsync(long quizId, long userId);
    }

    public class QuizSubmissionService : IQuizSubmissionService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuizSubmissionService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<QuizResultResponse> SubmitQuizAsync(long quizId, SubmitQuizRequest request, long userId)
        {
            // Загружаем квиз с вопросами и ответами
            var quiz = await _context.Quizzes
                .Include(q => q.Puzzles)
                    .ThenInclude(p => p.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
                throw new Exception($"Квиз с ID {quizId} не найден");

            // Подсчёт правильных ответов
            int correctCount = 0;
            int totalQuestions = quiz.Puzzles.Count;

            foreach (var submission in request.Answers)
            {
                // Находим вопрос по ID
                var puzzle = quiz.Puzzles.FirstOrDefault(p => p.Id == submission.PuzzleId);
                if (puzzle == null) continue;

                // Находим выбранный ответ
                var selectedAnswer = puzzle.AnswerOptions
                    .FirstOrDefault(a => a.Id == submission.AnswerOptionId);

                // Проверяем, правильный ли ответ
                if (selectedAnswer != null && selectedAnswer.IsCorrect)
                {
                    correctCount++;
                }
            }

            // Сохраняем результат в БД
            var result = new UserResult
            {
                UserId = userId,
                QuizId = quizId,
                Score = correctCount,
                CompletedAt = DateTime.UtcNow
            };

            _context.UserResults.Add(result);
            await _context.SaveChangesAsync();

            // Возвращаем результат
            return new QuizResultResponse
            {
                Id = result.Id,
                QuizId = quizId,
                QuizTitle = quiz.Title,
                Score = correctCount,
                TotalQuestions = totalQuestions,
                Percentage = totalQuestions > 0 ? Math.Round((double)correctCount / totalQuestions * 100, 2) : 0,
                CompletedAt = result.CompletedAt
            };
        }

        public async Task<List<UserQuizResultResponse>> GetUserResultsAsync(long userId)
        {
            var results = await _context.UserResults
                .Include(r => r.Quiz)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            return results.Select(r => new UserQuizResultResponse
            {
                ResultId = r.Id,
                QuizId = r.QuizId,
                QuizTitle = r.Quiz.Title,
                Score = r.Score,
                TotalQuestions = r.Quiz.Puzzles.Count,
                Percentage = r.Quiz.Puzzles.Count > 0
                    ? Math.Round((double)r.Score / r.Quiz.Puzzles.Count * 100, 2)
                    : 0,
                CompletedAt = r.CompletedAt
            }).ToList();
        }

        public async Task<QuizStatisticsResponse?> GetQuizStatisticsAsync(long quizId, long userId)
        {
            // Проверка: только автор может видеть статистику
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
            if (quiz == null) return null;

            if (quiz.CreatedById != userId)
                throw new UnauthorizedAccessException("Только автор может просматривать статистику квиза");

            // Получаем все результаты для этого квиза
            var results = await _context.UserResults
                .Where(r => r.QuizId == quizId)
                .ToListAsync();

            int totalAttempts = results.Count;
            double averageScore = totalAttempts > 0
                ? results.Average(r => r.Score)
                : 0;
            int highScore = totalAttempts > 0
                ? results.Max(r => r.Score)
                : 0;

            return new QuizStatisticsResponse
            {
                QuizId = quizId,
                Title = quiz.Title,
                TotalAttempts = totalAttempts,
                AverageScore = Math.Round(averageScore, 2),
                HighScore = highScore,
                TotalQuestions = quiz.Puzzles.Count
            };
        }
    }
}
