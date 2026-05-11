namespace QuizPlatform.Domain.Entities
{
    public class Quiz
    {

        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } // может быть null
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // аналог Many-to-One -->
        public long CreatedById { get; set; } // Foreign Key (аналог @JoinColumn)
        public User? CreatedBy { get; set; }
        // аналог Many-to-One <--

        // аналог @OneToMany -->
        public ICollection<Puzzle> Puzzles { get; set; } = new List<Puzzle>();
        public ICollection<UserResult> Results { get; set; } = new List<UserResult>();
        // аналог @OneToMany <--

        // аналог helper methods в Java -->
        public void AddPuzzle(Puzzle puzzle)
        {
            Puzzles.Add(puzzle);
            puzzle.Quiz = this;
        }

        public void RemovePuzzle(Puzzle puzzle)
        {
            Puzzles.Remove(puzzle);
            puzzle.Quiz = null;
        }
        // аналог helper methods в Java <--

    }
}
