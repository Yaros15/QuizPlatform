
namespace QuizPlatform.Domain.Entities
{
    public class AnswerOption
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        // аналог Many-to-One -->
        public long PuzzleId { get; set; }
        public Puzzle Puzzle { get; set; } = null!;
        // аналог Many-to-One <--
    }
}
