using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizPlatform.API.Services;
using QuizPlatform.DTO.Requests;
using QuizPlatform.DTO.Responses;
using System.Security.Claims;

namespace QuizPlatform.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET /api/quizzes - публичный эндпоинт
        [HttpGet]
        public async Task<ActionResult<List<QuizResponse>>> GetAllQuizzes()
        {
            var quizzes = await _quizService.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        // GET /api/quizzes/{id} - публичный эндпоинт
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizResponse>> GetQuizById(long id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);

            if (quiz == null)
                return NotFound(new { message = $"Квиз с ID {id} не найден" });

            return Ok(quiz);
        }

        // POST /api/quizzes - только для авторизованных
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<QuizResponse>> CreateQuiz([FromBody] CreateQuizRequest request)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException());

            var quiz = await _quizService.CreateQuizAsync(request, userId);
            return CreatedAtAction(nameof(GetQuizById), new { id = quiz.Id }, quiz);
        }

        // PUT /api/quizzes/{id} - только автор
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<QuizResponse>> UpdateQuiz(long id, [FromBody] CreateQuizRequest request)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException());

            var quiz = await _quizService.UpdateQuizAsync(id, request, userId);

            if (quiz == null)
                return NotFound(new { message = $"Квиз с ID {id} не найден" });

            return Ok(quiz);
        }

        // DELETE /api/quizzes/{id} - только автор
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteQuiz(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException());

            var deleted = await _quizService.DeleteQuizAsync(id, userId);

            if (!deleted)
                return NotFound(new { message = $"Квиз с ID {id} не найден" });

            return NoContent();
        }
    }
}
