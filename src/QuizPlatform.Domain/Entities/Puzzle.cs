
namespace QuizPlatform.Domain.Entities
{
    public class Puzzle
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int OrderIndex { get; set; }

        // аналог Many-to-One -->
        public long QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        // аналог Many-to-One <--

        // аналог @OneToMany -->
        public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
        // аналог @OneToMany <--

        // аналог helper methods в Java -->
        public void AddAnswerOption(AnswerOption option)
        {
            AnswerOptions.Add(option);
            option.Puzzle = this;
        }
        // аналог helper methods в Java <--
    }
}
