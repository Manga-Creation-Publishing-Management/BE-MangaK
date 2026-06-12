namespace Manga.Service.ChapterVoting;

public class Response
{
    public class VoteChapterResponse
    {
        public Guid Id { get; set; }
        public Guid ChapterId { get; set; }
        public int Rate { get; set; }
        public DateTimeOffset VoteAt { get; set; }
    }
}