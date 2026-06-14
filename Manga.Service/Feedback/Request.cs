namespace Manga.Service.Feedback;

public class Request
{
    public class SendFeedbackRequest
    {
        public required Guid ReceiverId { get; set; }
        public Guid? ChapterId { get; set; }
        public Guid? SeriesId { get; set; }
        public Guid? MangaId { get; set; }
        public required string Content { get; set; }
        
    }
    public class GetFeedBackRequest
    {
        public Guid? ChapterId { get; set; }
        public Guid? SeriesId { get; set; }
        public Guid? MangaId { get; set; }
    }
}