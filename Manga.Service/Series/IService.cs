using Manga.Repository.Entity.Enums;

namespace Manga.Service.Series;

public interface IService
{
    Task<Response.CreateSeriesResponse> CreateSeries(Request.CreateSeriesRequest request);
    Task<List<Response.GetAllSeriesResponse>> GetAllSeries();
    Task<Response.GetSeriesDetailsResponse> GetSeriesDetails(Guid seriesId);
    Task<List<Response.GetAllSeriesResponse>> GetSeriesByTitle(string title);
    Task<Response.ReviewSeriesResponse> ReviewSeriesByTantouEditor(Guid seriesId, Request.ReviewSeriesRequest request);
    Task<Response.ReviewSeriesResponse> ApprovedSeriesByEditorialBoard(Guid seriesId, Request.ReviewSeriesRequest request);
    Task<List<Response.GetAllSeriesResponse>> FilterSeriesByStatus(SeriesStatus status);
    Task<List<Response.GetAllSeriesResponse>> GetAllSeriesByCategory(Request.GetSeriesByCategoryRequest request);
    Task<Response.CancelSeriesResponse> CancelSeries(Guid seriesId, Request.CancelSeriesRequest request);
}