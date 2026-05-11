using QuizPlatform.Domain.Enums;

namespace QuizPlatform.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // аналог @OneToMany -->
        public ICollection<Quiz> CreatedQuizzes { get; set; } = new List<Quiz>();

        public ICollection<UserResult> Results { get; set; } = new List<UserResult>();
        // аналог @OneToMany <--

        // аналог helper methods в Java -->
        public void AddQuiz(Quiz quiz)
        {
            CreatedQuizzes.Add(quiz);
            quiz.CreatedBy = this;
        }

        public void RemoveQuiz(Quiz quiz)
        {
            CreatedQuizzes.Remove(quiz);
            quiz.CreatedBy = null;
        }
        // аналог helper methods в Java <--
    }
}
