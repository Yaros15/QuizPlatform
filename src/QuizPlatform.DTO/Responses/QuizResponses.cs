

namespace QuizPlatform.DTO.Responses
{
    public class QuizResponse
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<PuzzleResponse> Puzzles { get; set; } = new();
    }
    public class PuzzleResponse
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public List<AnswerOptionResponse> AnswerOptions { get; set; } = new();
    }

    public class AnswerOptionResponse
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
