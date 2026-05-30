namespace Manga.Service.Chapter;

public interface IService
{
    Task<Response.CreateChapterResponse> CreateChapter(Guid seriesId, Request.CreateChapterRequest request);
    Task<List<Response.GetAllChaptersResponse>> GetAllChapters(Guid seriesId);
    Task<Response.GetChapterDetailsResponse>  GetChapterDetails(Guid seriesId, Guid chapterId);
}