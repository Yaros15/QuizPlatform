
namespace QuizPlatform.Domain.Entities
{
    public class UserResult
    {

        public long Id { get; set; }
        public int Score { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        // аналог Many-to-One -->
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public long QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
        // аналог Many-to-One <--
    }
}
