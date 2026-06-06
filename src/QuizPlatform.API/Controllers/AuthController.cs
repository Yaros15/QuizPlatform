using Microsoft.AspNetCore.Mvc;
using QuizPlatform.API.Services;
using QuizPlatform.DTO.Requests;
using QuizPlatform.DTO.Responses;

namespace QuizPlatform.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterRequest request)
        {
            var user = await _authService.RegisterAsync(request);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
    }
}
