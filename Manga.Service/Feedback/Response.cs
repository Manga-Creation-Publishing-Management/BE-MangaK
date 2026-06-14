namespace Manga.Service.Feedback;

public class Response
{
    public class GetFeedBackResponse
    {
        public required Guid ReceiverId { get; set; }
        public required Guid SenderId { get; set; }
        public Guid? ChapterId { get; set; }
        public Guid? SeriesId { get; set; }
        public Guid? MangaId { get; set; }
        public required string Content { get; set; }
    }
}