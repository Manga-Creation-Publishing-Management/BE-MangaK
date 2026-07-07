namespace Manga.Service.ChapterVoting;

public class Request
{
    public class VoteChapterRequest
    {
        public Guid ChapterId { get; set; }
        public int Rate { get; set; }
    }
}