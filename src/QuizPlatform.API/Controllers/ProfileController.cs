using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuizPlatform.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Требуется аутентификация
    public class ProfileController : ControllerBase
    {
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            // Получаем данные пользователя из токена
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                id = userId,
                username,
                email,
                role
            });
        }

        [Authorize(Roles = "Admin")]  // Только для админов
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "Доступ разрешён только администраторам!" });
        }
    }
}
