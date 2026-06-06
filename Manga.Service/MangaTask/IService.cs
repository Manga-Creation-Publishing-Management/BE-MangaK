namespace Manga.Service.MangaTask;

public interface IService
{
    public Task<Response.CreateNewTaskResponse> CreateNewTask(Request.CreateNewTaskRequest request);
    public Task<Response.GetTaskDetailsResponse> GetTaskDetails(Request.GetTaskDetailsRequest request);
    public Task<List<Response.GetTaskListResponse>> GetTaskList(Request.GetTaskListRequest request);
}