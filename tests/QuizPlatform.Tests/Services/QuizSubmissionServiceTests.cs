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
    

    public class QuizSubmissionServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly QuizSubmissionService _submissionService;
        private readonly User _testUser;
        private readonly Quiz _testQuiz;
        private readonly string _databaseName;

        public QuizSubmissionServiceTests()
        {

            // Уникальная БД для каждого теста
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

            _submissionService = new QuizSubmissionService(_context, _mapper);

            // Создаём тестового пользователя
            _testUser = new User
            {
                Id = 1,
                Username = "quiztaker",
                Email = "taker@example.com",
                PasswordHash = "hashedpassword",
                Role = UserRole.User
            };
            _context.Users.Add(_testUser);

            // Создаём тестовый квиз с вопросами
            _testQuiz = new Quiz
            {
                Id = 1,
                Title = "Test Quiz",
                CreatedById = _testUser.Id,
                CreatedBy = _testUser,
                Puzzles = new List<Puzzle>
            {
                new Puzzle
                {
                    Id = 1,
                    Text = "Question 1?",
                    OrderIndex = 0,
                    AnswerOptions = new List<AnswerOption>
                    {
                        new AnswerOption { Id = 1, Text = "Correct Answer", IsCorrect = true },
                        new AnswerOption { Id = 2, Text = "Wrong Answer", IsCorrect = false }
                    }
                },
                new Puzzle
                {
                    Id = 2,
                    Text = "Question 2?",
                    OrderIndex = 1,
                    AnswerOptions = new List<AnswerOption>
                    {
                        new AnswerOption { Id = 3, Text = "Wrong Answer", IsCorrect = false },
                        new AnswerOption { Id = 4, Text = "Correct Answer", IsCorrect = true }
                    }
                }
            }
            };
            _context.Quizzes.Add(_testQuiz);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Прохождение квиза

        [Fact]
        public async Task SubmitQuizAsync_AllCorrectAnswers_ReturnsFullScore()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>
            {
                new AnswerSubmissionRequest { PuzzleId = 1, AnswerOptionId = 1 }, // Правильно
                new AnswerSubmissionRequest { PuzzleId = 2, AnswerOptionId = 4 }  // Правильно
            }
            };

            // Act
            var result = await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Assert
            result.Should().NotBeNull();
            result.Score.Should().Be(2);
            result.TotalQuestions.Should().Be(2);
            result.Percentage.Should().Be(100.0);
        }

        [Fact]
        public async Task SubmitQuizAsync_AllWrongAnswers_ReturnsZeroScore()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>
            {
                new AnswerSubmissionRequest { PuzzleId = 1, AnswerOptionId = 2 }, // Неправильно
                new AnswerSubmissionRequest { PuzzleId = 2, AnswerOptionId = 3 }  // Неправильно
            }
            };

            // Act
            var result = await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Assert
            result.Score.Should().Be(0);
            result.Percentage.Should().Be(0.0);
        }

        [Fact]
        public async Task SubmitQuizAsync_PartialCorrectAnswers_ReturnsPartialScore()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>
            {
                new AnswerSubmissionRequest { PuzzleId = 1, AnswerOptionId = 1 }, // Правильно
                new AnswerSubmissionRequest { PuzzleId = 2, AnswerOptionId = 3 }  // Неправильно
            }
            };

            // Act
            var result = await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Assert
            result.Score.Should().Be(1);
            result.Percentage.Should().Be(50.0);
        }

        [Fact]
        public async Task SubmitQuizAsync_NonExistingQuiz_ThrowsException()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _submissionService.SubmitQuizAsync(999, request, _testUser.Id));
        }

        [Fact]
        public async Task SubmitQuizAsync_SavesResultToDatabase()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>
            {
                new AnswerSubmissionRequest { PuzzleId = 1, AnswerOptionId = 1 }
            }
            };

            // Act
            await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Assert
            var resultInDb = await _context.UserResults.FirstOrDefaultAsync(r => r.UserId == _testUser.Id);
            resultInDb.Should().NotBeNull();
            resultInDb!.QuizId.Should().Be(_testQuiz.Id);
            resultInDb.Score.Should().Be(1);
        }

        #endregion

        #region Мои результаты

        [Fact]
        public async Task GetUserResultsAsync_ReturnsUserResults()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>
            {
                new AnswerSubmissionRequest { PuzzleId = 1, AnswerOptionId = 1 }
            }
            };
            await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Act
            var results = await _submissionService.GetUserResultsAsync(_testUser.Id);

            // Assert
            results.Should().HaveCount(1);
            results[0].QuizTitle.Should().Be("Test Quiz");
            results[0].Score.Should().Be(1);
        }

        #endregion

        #region Статистика квиза

        [Fact]
        public async Task GetQuizStatisticsAsync_Owner_CanViewStatistics()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>
            {
                new AnswerSubmissionRequest { PuzzleId = 1, AnswerOptionId = 1 }
            }
            };
            await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Act
            var stats = await _submissionService.GetQuizStatisticsAsync(_testQuiz.Id, _testUser.Id);

            // Assert
            stats.Should().NotBeNull();
            stats!.TotalAttempts.Should().Be(1);
            stats.AverageScore.Should().Be(1.0);
            stats.HighScore.Should().Be(1);
        }

        [Fact]
        public async Task GetQuizStatisticsAsync_NotOwner_ThrowsException()
        {
            // Arrange
            var request = new SubmitQuizRequest
            {
                Answers = new List<AnswerSubmissionRequest>()
            };
            await _submissionService.SubmitQuizAsync(_testQuiz.Id, request, _testUser.Id);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _submissionService.GetQuizStatisticsAsync(_testQuiz.Id, 999)); // Другой пользователь
        }

        #endregion
    }
}
