namespace Manga.Service.MangaTask;

public interface IService
{
    public Task<Response.CreateNewTaskResponse> CreateNewTask(Request.CreateNewTaskRequest request);
    public Task<Response.GetTaskDetailsResponse> GetTaskDetails(Request.GetTaskDetailsRequest request);
    public Task<List<Response.GetTaskListResponse>> GetTaskList(Request.GetTaskListRequest request);
    public Task<bool> AcceptTask(Request.AcceptTaskRequest request);
    public Task<bool> RejectTask(Request.RejectTaskRequest request);
    public Task<bool> SubmitTask(Request.SubmitTaskRequest request);
    public Task<bool> ReviewTask(Request.ReviewTaskRequest request);
}