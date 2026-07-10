namespace Manga.Service.Feedback;

public interface IService
{
    Task<bool> SendFeedback(Request.SendFeedbackRequest request);
    Task<List<Response.GetFeedBackDetailResponse>> GetFeedBackDetail(Request.GetFeedBackRequest request);
    Task<List<Response.GetFeedBackDetailResponse>> GetFeedbackList();
    Task<Response.GetFeedBackDetailResponse> GetFeedbackAnnotation(Request.GetFeedBackRequest request);
    Task<bool> MarkAsRead(Guid feedbackId);
}