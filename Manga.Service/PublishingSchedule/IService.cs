namespace Manga.Service.PublishingSchedule;

public interface IService
{
    Task<Response.CreatePublishingScheduleResponse>  CreatePublishingSchedule(Guid seriesId, Request.CreatePublishingScheduleRequest request);
    Task<List<Response.GetPublishingScheduleResponse>> GetAllPublishingSchedules();
}