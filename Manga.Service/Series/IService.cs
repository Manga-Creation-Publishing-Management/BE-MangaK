namespace Manga.Service.Series;

public interface IService
{
    Task<Response.CreateSeriesResponse> CreateSeries(Request.CreateSeriesRequest request);
    Task<List<Response.GetAllSeriesResponse>> GetAllSeries();
    Task<Response.GetSeriesDetailsResponse> GetSeriesDetails(Guid seriesId);
    Task<List<Response.GetAllSeriesResponse>> GetSeriesByTitle(string title);
}