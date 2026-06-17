namespace Manga.Service.MangaTask;

public interface IService
{
    public Task<Response.CreateNewTaskResponse> CreateNewTask(Request.CreateNewTaskRequest request);
    public Task<Response.GetTaskDetailsResponse> GetTaskDetails(Request.GetTaskDetailsRequest request);
    public Task<List<Response.GetTaskDetailsResponse>> GetTaskList(Request.GetTaskListRequest request);
    public Task<bool> UpdateTaskStatus(Request.UpdateTaskStatusRequest request);
    public Task<bool> SubmitTask(Request.SubmitTaskRequest request);
    public Task<bool> ReviewTask(Request.ReviewTaskRequest request);
    public Task<Response.GetTotalTaskResponse> GetTotalTask(Request.GetTaskListRequest request);
    
}