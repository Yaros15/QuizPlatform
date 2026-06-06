using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.API.Data;
using QuizPlatform.Domain.Entities;
using QuizPlatform.DTO.Requests;
using QuizPlatform.DTO.Responses;

namespace QuizPlatform.API.Services
{
    public interface IQuizService
    {
        // Чтение
        Task<List<QuizResponse>> GetAllQuizzesAsync();
        Task<QuizResponse?> GetQuizByIdAsync(long id);

        // Создание
        Task<QuizResponse> CreateQuizAsync(CreateQuizRequest request, long createdById);

        // Обновление (только автор)
        Task<QuizResponse?> UpdateQuizAsync(long id, CreateQuizRequest request, long userId);

        // Удаление (только автор)
        Task<bool> DeleteQuizAsync(long id, long userId);
    }

    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuizService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<QuizResponse>> GetAllQuizzesAsync()
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.CreatedBy)
                .Include(q => q.Puzzles)
                    .ThenInclude(p => p.AnswerOptions)
                .ToListAsync();

            return _mapper.Map<List<QuizResponse>>(quizzes);
        }

        public async Task<QuizResponse?> GetQuizByIdAsync(long id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.CreatedBy)
                .Include(q => q.Puzzles)
                    .ThenInclude(p => p.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return null;

            return _mapper.Map<QuizResponse>(quiz);
        }

        public async Task<QuizResponse> CreateQuizAsync(CreateQuizRequest request, long createdById)
        {
            var quiz = _mapper.Map<Quiz>(request);
            quiz.CreatedById = createdById;
            quiz.CreatedAt = DateTime.UtcNow;

            // Маппинг вопросов и ответов
            foreach (var puzzleRequest in request.Puzzles)
            {
                var puzzle = _mapper.Map<Puzzle>(puzzleRequest);
                puzzle.Quiz = quiz;

                foreach (var answerRequest in puzzleRequest.AnswerOptions)
                {
                    var answer = _mapper.Map<AnswerOption>(answerRequest);
                    answer.Puzzle = puzzle;
                    puzzle.AnswerOptions.Add(answer);
                }

                quiz.Puzzles.Add(puzzle);
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return await GetQuizByIdAsync(quiz.Id) ?? throw new Exception("Failed to create quiz");
        }

        public async Task<QuizResponse?> UpdateQuizAsync(long id, CreateQuizRequest request, long userId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Puzzles)
                    .ThenInclude(p => p.AnswerOptions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return null;

            // Проверка: только автор может редактировать
            if (quiz.CreatedById != userId)
                throw new UnauthorizedAccessException("Только автор может редактировать этот квиз");

            // Обновляем простые поля
            quiz.Title = request.Title;
            quiz.Description = request.Description;

            // Полная замена вопросов (для простоты)
            _context.Puzzles.RemoveRange(quiz.Puzzles);
            quiz.Puzzles.Clear();

            foreach (var puzzleRequest in request.Puzzles)
            {
                var puzzle = _mapper.Map<Puzzle>(puzzleRequest);
                puzzle.Quiz = quiz;

                foreach (var answerRequest in puzzleRequest.AnswerOptions)
                {
                    var answer = _mapper.Map<AnswerOption>(answerRequest);
                    answer.Puzzle = puzzle;
                    puzzle.AnswerOptions.Add(answer);
                }

                quiz.Puzzles.Add(puzzle);
            }

            await _context.SaveChangesAsync();

            return await GetQuizByIdAsync(quiz.Id);
        }

        public async Task<bool> DeleteQuizAsync(long id, long userId)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null) return false;

            // Проверка: только автор может удалять
            if (quiz.CreatedById != userId)
                throw new UnauthorizedAccessException("Только автор может удалять этот квиз");

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
