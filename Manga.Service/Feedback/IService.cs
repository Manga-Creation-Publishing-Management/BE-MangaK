namespace Manga.Service.Feedback;

public interface IService
{
    Task<bool> SendFeedback(Request.SendFeedbackRequest request);
    Task<List<Response.GetFeedBackDetailResponse>> GetFeedBackDetail(Request.GetFeedBackRequest request);
    Task<List<Response.GetFeedBackDetailResponse>> GetFeedbackList();
    Task<bool> MarkAsRead(Guid feedbackId);
}