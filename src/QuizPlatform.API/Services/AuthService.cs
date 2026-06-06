using AutoMapper;
using Microsoft.EntityFrameworkCore;
using QuizPlatform.API.Data;
using QuizPlatform.API.Services.Jwt;
using QuizPlatform.Domain.Entities;
using QuizPlatform.Domain.Enums;
using QuizPlatform.DTO.Requests;
using QuizPlatform.DTO.Responses;

namespace QuizPlatform.API.Services
{

    public interface IAuthService
    {
        Task<UserResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public AuthService(AppDbContext context, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            // Проверка на существование
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                throw new Exception("Пользователь с таким именем уже существует");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email уже занят");

            // Создание пользователя
            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.Role = Domain.Enums.UserRole.User;
            user.CreatedAt = DateTime.UtcNow;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserResponse>(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Поиск пользователя
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Неверное имя пользователя или пароль");

            // Генерация JWT токена
            var token = _jwtService.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role.ToString(),
                ExpiresIn = 86400
            };
        }

    }
}
