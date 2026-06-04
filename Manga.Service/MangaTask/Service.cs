using Manga.Repository.Data;
using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.MangaTask;

public class Service : IService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MediaService.IService _mediaService;
    private readonly AppDbContext _dbContext;

    public Service(IHttpContextAccessor httpContextAccessor, MediaService.IService mediaService, AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _mediaService = mediaService;
        _dbContext = dbContext;
    }

    public async Task<Response.CreateNewTaskResponse> CreateNewTask(Request.CreateNewTaskRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (userId == null) throw new UnauthorizedAccessException("Unauthorized");
        
        var userIdGuid = Guid.Parse(userId);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null) throw new UnauthorizedAccessException("Unauthorized");
        if(user.Role != UserRole.Mangaka) throw new UnauthorizedAccessException("Only Mangaka is allowed");
        
        var chapter = await _dbContext.Chapters
            .Include(x => x.Series)
            .FirstOrDefaultAsync(x => x.Id == request.ChapterId);
        if (chapter == null) throw new KeyNotFoundException("You cannot create a chapter. Chapter can not be found");
        if (chapter.Series == null || chapter.Series.Status != SeriesStatus.Approved)
        {
            throw new InvalidDataException("You cannot create a chapter. Series must be approved");
        }
        if(chapter.Status != ChapterStatus.Processing) throw new InvalidOperationException("You cannot create a chapter. Status must be Processing status");
        
        var assignedAssistant = await _dbContext.Users.
            FirstOrDefaultAsync(x => x.Id == request.AssignedToId);
        if(assignedAssistant == null) throw new KeyNotFoundException("Assigned assistant not found");
        if(assignedAssistant.Role != UserRole.Assistant) throw new UnauthorizedAccessException("Task can only be assigned to Assistant");
        var mangaTask = new Repository.Entity.MangaTask()
        {
            Id = Guid.NewGuid(),
            TaskTitle = request.TaskTitle,
            TaskDescription = request.TaskDescription,
            Deadline = request.Deadline,
            AssignedAt = DateTimeOffset.Now,
            ChapterId = request.ChapterId,
            AssignedToId = request.AssignedToId,
            CreatedById = userIdGuid,
            Income = new Income()
            {
                Id = Guid.NewGuid(),
                Amount = request.AmountIncome,
                CreatedAt = DateTimeOffset.Now,
            },
        };
        _dbContext.Add(mangaTask);
        await _dbContext.SaveChangesAsync();
        return new Response.CreateNewTaskResponse()
        {
            Id = mangaTask.Id,
            TaskTitle = mangaTask.TaskTitle,
            TaskDescription = mangaTask.TaskDescription,
            Deadline = mangaTask.Deadline,
            AssignedToId = mangaTask.AssignedToId,
            AssignedAt = mangaTask.AssignedAt,
            ChapterId = mangaTask.ChapterId,
            Income = mangaTask.Income,
            CreatedAt = mangaTask.CreatedAt
        };
    }

    public async Task<Response.GetTaskDetailsResponse> GetTaskDetails(Request.GetTaskDetailsRequest request)
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (userId == null) throw new UnauthorizedAccessException("Unauthorized");
        var userIdGuid = Guid.Parse(userId);
        // var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        
        
        var taskChapter  =await _dbContext.MangaTasks
            .Include(x => x.Chapter)
            .Include(x => x.CreatedBy)
            .Include(x => x.Income)
            .Include(x => x.AssignedTo)
            .Include(x => x.Feedbacks)
            .ThenInclude(x => x.Sender)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (taskChapter == null) throw new KeyNotFoundException("Task not found");
        
        if(userIdGuid != taskChapter.AssignedToId && userIdGuid != taskChapter.CreatedById) throw new UnauthorizedAccessException("You don't have permission to access this task");
        
        
        var feedbackList = taskChapter.Feedbacks.OrderBy(x => x.CreatedAt)
            .Select(x => new Response.FeedbackSummaryResponse()
            {
                FeedbackId = x.Id,
                SenderId = x.SenderId,
                ReceiverId = x.ReceiverId,
                Content = x.Content,
                CreatedAt = x.CreatedAt,
            }).ToList();

        return new Response.GetTaskDetailsResponse()
        {
            Id = taskChapter.Id,
            TaskTitle = taskChapter.TaskTitle,
            TaskDescription = taskChapter.TaskDescription,
            SubmittedFileUrl = taskChapter.submittedFileUrl,
            Status = taskChapter.Status,
            Deadline = taskChapter.Deadline,
            AssignedAt = taskChapter.AssignedAt,
            SubmittedAt = taskChapter.SubmittedAt,
            ChapterId = taskChapter.ChapterId,
            CreatedById = taskChapter.CreatedById,
            AssignedToId = taskChapter.AssignedToId,
            Income = taskChapter.Income,
            Feedback = feedbackList
        };
    }

    public Task<Response.GetTaskListResponse> GetTaskList(Request.GetTaskListRequest request)
    {
        throw new NotImplementedException();
    }
}