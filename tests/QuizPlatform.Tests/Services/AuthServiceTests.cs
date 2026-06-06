using AutoMapper;
using global::QuizPlatform.API.Mappings;
using FluentAssertions;
using global::QuizPlatform.API.Data;
using global::QuizPlatform.API.Services;
using global::QuizPlatform.API.Services.Jwt;
using global::QuizPlatform.Domain.Entities;
using global::QuizPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using QuizPlatform.DTO.Requests;

namespace QuizPlatform.Tests.Services
{

    public class AuthServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;
        private readonly AuthService _authService;
        private readonly string _databaseName;

        public AuthServiceTests()
        {

            // Уникальная БД для каждого теста
            _databaseName = Guid.NewGuid().ToString();

            // Используем In-Memory БД для тестов
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            _context = new AppDbContext(options);

            // Настраиваем AutoMapper
            var mappingProfile = new MappingProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(mappingProfile));
            _mapper = new Mapper(configuration);

            // Мокаем JWT сервис
            var mockJwtService = new Mock<IJwtService>();
            mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token-for-testing");
            _jwtService = mockJwtService.Object;

            _authService = new AuthService(_context, _mapper, _jwtService);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Регистрация

        [Fact]
        public async Task RegisterAsync_ValidData_ReturnsUserResponse()
        {
            // Arrange (Подготовка)
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123"
            };

            // Act (Действие)
            var result = await _authService.RegisterAsync(request);

            // Assert (Проверка)
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
            result.Role.Should().Be("User");

            // Проверяем, что пользователь сохранён в БД
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            userInDb.Should().NotBeNull();
            userInDb!.PasswordHash.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ThrowsException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "existinguser",
                Email = "test@example.com",
                Password = "Password123"
            };

            // Создаём пользователя с таким же именем
            _context.Users.Add(new User
            {
                Username = "existinguser",
                Email = "different@example.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.User
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "newuser",
                Email = "existing@example.com",
                Password = "Password123"
            };

            // Создаём пользователя с таким же email
            _context.Users.Add(new User
            {
                Username = "differentuser",
                Email = "existing@example.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.User
            });
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(request));
        }

        [Fact]
        public async Task RegisterAsync_PasswordIsHashed()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Password123"
            };

            // Act
            await _authService.RegisterAsync(request);

            // Assert
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            userInDb.Should().NotBeNull();

            // Пароль должен быть захеширован (не равен исходному)
            userInDb!.PasswordHash.Should().NotBe("Password123");
            userInDb.PasswordHash.Should().StartWith("$2"); // BCrypt формат
        }

        #endregion

        #region Вход

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "loginuser",
                Email = "login@example.com",
                Password = "Password123"
            };
            await _authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Username = "loginuser",
                Password = "Password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("fake-jwt-token-for-testing");
            result.Username.Should().Be("loginuser");
            result.Role.Should().Be("User");
        }

        [Fact]
        public async Task LoginAsync_InvalidUsername_ThrowsException()
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "nonexistent",
                Password = "Password123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(request));
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsException()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Username = "passworduser",
                Email = "password@example.com",
                Password = "Password123"
            };
            await _authService.RegisterAsync(registerRequest);

            var loginRequest = new LoginRequest
            {
                Username = "passworduser",
                Password = "WrongPassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(loginRequest));
        }

        #endregion
    }

}
