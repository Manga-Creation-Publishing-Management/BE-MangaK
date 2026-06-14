namespace Manga.Service.Feedback;

public interface IService
{
    Task<bool> SendFeedback(Request.SendFeedbackRequest request);
    Task<Response.GetFeedBackResponse> GetFeedBack(Request.GetFeedBackRequest request);
    
}