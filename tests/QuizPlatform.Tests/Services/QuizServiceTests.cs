using AutoMapper;
using global::QuizPlatform.API.Mappings;
using FluentAssertions;
using global::QuizPlatform.API.Data;
using global::QuizPlatform.API.Services;
using global::QuizPlatform.Domain.Entities;
using global::QuizPlatform.Domain.Enums;
using global::QuizPlatform.DTO.Requests;
using Microsoft.EntityFrameworkCore;

namespace QuizPlatform.Tests.Services
{

    public class QuizServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly QuizService _quizService;
        private readonly User _testUser;
        private readonly string _databaseName;

        public QuizServiceTests()
        {

            // Уникальное имя БД для каждого экземпляра теста
            _databaseName = Guid.NewGuid().ToString();

            // In-Memory БД
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: _databaseName)
                .Options;

            _context = new AppDbContext(options);

            // AutoMapper
            var mappingProfile = new MappingProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(mappingProfile));
            _mapper = new Mapper(configuration);

            _quizService = new QuizService(_context, _mapper);

            // Создаём тестового пользователя
            _testUser = new User
            {
                Id = 1,
                Username = "quizcreator",
                Email = "creator@example.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.User
            };
            _context.Users.Add(_testUser);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Создание квиза

        [Fact]
        public async Task CreateQuizAsync_ValidData_ReturnsQuizResponse()
        {
            // Arrange
            var request = new CreateQuizRequest
            {
                Title = "Test Quiz",
                Description = "Test Description",
                Puzzles = new List<CreatePuzzleRequest>
            {
                new CreatePuzzleRequest
                {
                    Text = "Question 1?",
                    OrderIndex = 0,
                    AnswerOptions = new List<CreateAnswerOptionRequest>
                    {
                        new CreateAnswerOptionRequest { Text = "Answer 1", IsCorrect = true },
                        new CreateAnswerOptionRequest { Text = "Answer 2", IsCorrect = false }
                    }
                }
            }
            };

            // Act
            var result = await _quizService.CreateQuizAsync(request, _testUser.Id);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Quiz");
            result.AuthorUsername.Should().Be("quizcreator");
            result.Puzzles.Should().HaveCount(1);
            result.Puzzles[0].AnswerOptions.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateQuizAsync_WithMultipleQuestions_CreatesAllQuestions()
        {
            // Arrange
            var request = new CreateQuizRequest
            {
                Title = "Multi-Question Quiz",
                Puzzles = new List<CreatePuzzleRequest>
            {
                new CreatePuzzleRequest { Text = "Q1", OrderIndex = 0, AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A1", IsCorrect = true } } },
                new CreatePuzzleRequest { Text = "Q2", OrderIndex = 1, AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A2", IsCorrect = true } } },
                new CreatePuzzleRequest { Text = "Q3", OrderIndex = 2, AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A3", IsCorrect = true } } }
            }
            };

            // Act
            var result = await _quizService.CreateQuizAsync(request, _testUser.Id);

            // Assert
            result.Puzzles.Should().HaveCount(3);
        }

        #endregion

        #region Получение квизов

        [Fact]
        public async Task GetAllQuizzesAsync_ReturnsAllQuizzes()
        {
            // Arrange
            await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "Quiz 1",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "Quiz 2",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            // Act
            var result = await _quizService.GetAllQuizzesAsync();

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetQuizByIdAsync_ExistingId_ReturnsQuiz()
        {
            // Arrange
            var created = await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "Find Me",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            // Act
            var result = await _quizService.GetQuizByIdAsync(created.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Find Me");
        }

        [Fact]
        public async Task GetQuizByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _quizService.GetQuizByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Обновление квиза

        [Fact]
        public async Task UpdateQuizAsync_Owner_CanUpdate()
        {
            // Arrange
            var created = await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "Original Title",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            var updateRequest = new CreateQuizRequest
            {
                Title = "Updated Title",
                Description = "New Description",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "New Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "New A", IsCorrect = true } } } }
            };

            // Act
            var result = await _quizService.UpdateQuizAsync(created.Id, updateRequest, _testUser.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Updated Title");
            result.Description.Should().Be("New Description");
        }

        [Fact]
        public async Task UpdateQuizAsync_NotOwner_ThrowsException()
        {
            // Arrange
            var created = await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "Owner Quiz",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            var updateRequest = new CreateQuizRequest
            {
                Title = "Hacked Title",
                Puzzles = new List<CreatePuzzleRequest>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _quizService.UpdateQuizAsync(created.Id, updateRequest, 999)); // Другой пользователь
        }

        #endregion

        #region Удаление квиза

        [Fact]
        public async Task DeleteQuizAsync_Owner_CanDelete()
        {
            // Arrange
            var created = await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "To Delete",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            // Act
            var result = await _quizService.DeleteQuizAsync(created.Id, _testUser.Id);

            // Assert
            result.Should().BeTrue();

            var deleted = await _quizService.GetQuizByIdAsync(created.Id);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task DeleteQuizAsync_NotOwner_ThrowsException()
        {
            // Arrange
            var created = await _quizService.CreateQuizAsync(new CreateQuizRequest
            {
                Title = "Protected Quiz",
                Puzzles = new List<CreatePuzzleRequest> { new() { Text = "Q", AnswerOptions = new List<CreateAnswerOptionRequest> { new() { Text = "A", IsCorrect = true } } } }
            }, _testUser.Id);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _quizService.DeleteQuizAsync(created.Id, 999));
        }

        #endregion
    }

}
