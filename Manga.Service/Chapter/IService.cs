namespace Manga.Service.Chapter;

public interface IService
{
    Task<Response.CreateChapterResponse> CreateChapter(Guid seriesId, Request.CreateChapterRequest request);
}