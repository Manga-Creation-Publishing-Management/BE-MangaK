namespace Manga.Service.ChapterVoting;

public interface IService
{
    Task<Response.VoteChapterResponse> VoteChapter(Request.VoteChapterRequest request);
    Task<Response.RankingResponse> CalculateChapterVote();
}