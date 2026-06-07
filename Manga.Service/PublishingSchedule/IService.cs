namespace Manga.Service.PublishingSchedule;

public interface IService
{
    Task<Response.CreatePublishingScheduleResponse>  CreatePublishingSchedule(Guid seriesId, Request.CreatePublishingScheduleRequest request);
    Task<List<Response.GetPublishingScheduleResponse>> GetAllPublishingSchedules();
    Task<Response.GetPublishingScheduleResponse> UpdatePublishingSchedule(Guid scheduleId, Request.UpdatePublishingScheduleRequest request);
    Task DeletePublishingSchedule(Guid scheduleId);
}