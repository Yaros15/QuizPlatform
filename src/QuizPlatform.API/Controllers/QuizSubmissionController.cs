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
    [Authorize]  // Все эндпоинты требуют авторизации
    public class QuizSubmissionController : ControllerBase
    {
        private readonly IQuizSubmissionService _submissionService;

        public QuizSubmissionController(IQuizSubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        // POST /api/quizsubmission/{quizId}/submit - Пройти квиз
        [HttpPost("{quizId}/submit")]
        public async Task<ActionResult<QuizResultResponse>> SubmitQuiz(long quizId, [FromBody] SubmitQuizRequest request)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException());

            try
            {
                var result = await _submissionService.SubmitQuizAsync(quizId, request, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/quizsubmission/me/results - Мои результаты
        [HttpGet("me/results")]
        public async Task<ActionResult<List<UserQuizResultResponse>>> GetMyResults()
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException());

            var results = await _submissionService.GetUserResultsAsync(userId);
            return Ok(results);
        }

        // GET /api/quizsubmission/{quizId}/stats - Статистика квиза (для автора)
        [HttpGet("{quizId}/stats")]
        public async Task<ActionResult<QuizStatisticsResponse>> GetQuizStatistics(long quizId)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException());

            try
            {
                var stats = await _submissionService.GetQuizStatisticsAsync(quizId, userId);

                if (stats == null)
                    return NotFound(new { message = $"Квиз с ID {quizId} не найден" });

                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });  // ✅ Правильно
            }
        }
    }

}
