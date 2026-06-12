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
        var userIdGuid = GetCurrentUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null) throw new UnauthorizedAccessException("Unauthorized");
        if (user.Role != UserRole.Mangaka) throw new UnauthorizedAccessException("Only Mangaka is allowed");
        
        var series = await _dbContext.Series.FirstOrDefaultAsync(x => x.Id == request.SeriesId);
        if(series == null) throw new KeyNotFoundException("Series not found");
        if(series.Status == SeriesStatus.Rejected) throw new KeyNotFoundException("Series was rejected");
        
        var chapter = await _dbContext.Chapters
            .Include(x => x.Series)
            .FirstOrDefaultAsync(x => x.Id == request.ChapterId);
        if (chapter == null) throw new KeyNotFoundException("You cannot create a task. Chapter can not be found");
        if (chapter.Series == null || chapter.Series.Status != SeriesStatus.Approved)
        {
            throw new InvalidDataException("You cannot create a task. Series must be approved");
        }

        if (chapter.Status != ChapterStatus.Processing)
            throw new InvalidOperationException("You cannot create a task. Chapter status must be Processing status");

        var assignedAssistant = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.AssignedToId);
        if (assignedAssistant == null) throw new KeyNotFoundException("Assigned assistant not found");
        if (assignedAssistant.Role != UserRole.Assistant)
            throw new UnauthorizedAccessException("Task can only be assigned to Assistant");
        
        var checkAssistant = await _dbContext.MangaTasks.AnyAsync(x => x.AssignedToId == request.AssignedToId && x.ChapterId == chapter.Id);
        if (checkAssistant) throw new InvalidOperationException("This assistant has already been assigned a task in this chapter.");
        
        if (request.Deadline <= DateTimeOffset.UtcNow)
        {
            throw new InvalidDataException("Deadline must be a future date.");
        }

        // if (request.AmountIncome <= 0)
        // {
        //     throw new InvalidDataException("Income amount must be greater than zero.");
        // }
        var mangaTask = new Repository.Entity.MangaTask()
        {
            Id = Guid.NewGuid(),
            TaskTitle = request.TaskTitle,
            TaskDescription = request.Page_range,
            Status = MangaTaskStatus.Available,
            Deadline = request.Deadline,
            AssignedAt = DateTimeOffset.UtcNow,
            ChapterId = request.ChapterId,
            AssignedToId = request.AssignedToId,
            CreatedById = userIdGuid,
            CreatedAt = DateTimeOffset.UtcNow,
            Income = new Income()
            {
                Id = Guid.NewGuid(),
                Amount = request.AmountIncome,
                CreatedAt = DateTimeOffset.UtcNow,
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
            Income = mangaTask.Income.Amount,
            CreatedAt = mangaTask.CreatedAt,
            SeriesId = series.Id,
        };
    }

    public async Task<Response.GetTaskDetailsResponse> GetTaskDetails(Request.GetTaskDetailsRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var taskDetail = await _dbContext.MangaTasks
            .Where(x => x.Id == request.TaskId)
            .Select(x => new Response.GetTaskDetailsResponse
            {
                Id = x.Id,
                TaskTitle = x.TaskTitle,
                TaskDescription = x.TaskDescription,
                SubmittedFileUrl = x.submittedFileUrl,
                Status = x.Status,
                Deadline = x.Deadline,
                AssignedAt = x.AssignedAt,
                SubmittedAt = x.SubmittedAt,
                ChapterId = x.ChapterId,
                CreatedById = x.CreatedById,
                AssignedToId = x.AssignedToId,
                IncomeAmount = x.Income.Amount,

                Feedback = x.Feedbacks
                    .OrderBy(f => f.CreatedAt)
                    .Select(f => new Response.FeedbackSummaryResponse
                    {
                        FeedbackId = f.Id,
                        SenderId = f.SenderId,
                        ReceiverId = f.ReceiverId,
                        Content = f.Content,
                        CreatedAt = f.CreatedAt
                    }).ToList(),
            })
            .FirstOrDefaultAsync();

        if (taskDetail == null) throw new KeyNotFoundException("Task not found");

        if (userIdGuid != taskDetail.CreatedById && userIdGuid != taskDetail.AssignedToId)
            throw new UnauthorizedAccessException("You don't have permission to access this task");

        return taskDetail;
    }

//Cái này e làm luôn chức năng filter theo status luôn nha
    public async Task<List<Response.GetTaskListResponse>> GetTaskList(Request.GetTaskListRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null) throw new KeyNotFoundException("User not found");
        if (user.Role != UserRole.Mangaka && user.Role != UserRole.Assistant)
            throw new UnauthorizedAccessException("You don't have permission to access this action");

        IQueryable<Repository.Entity.MangaTask> query = _dbContext.MangaTasks.AsQueryable();
        if (user.Role == UserRole.Assistant)
        {
            query = query.Where(t => t.AssignedToId == userIdGuid);
        }
        else if (user.Role == UserRole.Mangaka)
        {
            query = query.Where(t => t.CreatedById == userIdGuid);
        }

        if (request.ChapterId.HasValue)
        {
            query = query.Where(t => t.ChapterId == request.ChapterId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status);
        }

        var taskList = await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new Response.GetTaskListResponse()
            {
                TaskId = t.Id,
                TaskTitle = t.TaskTitle,
                TaskDescription = t.TaskDescription,
                Status = t.Status,
                Deadline = t.Deadline,
                SubmittedFileUrl = t.submittedFileUrl,
                CreatedAt = t.CreatedAt,
                SubmittedAt = t.SubmittedAt,

                ChapterId = t.ChapterId,
                ChapterNumber = t.Chapter.ChapterNumber,
                ChapterTitle = t.Chapter.Title,

                AssistantId = t.AssignedToId,
                AssistantName = t.AssignedTo.FirstName + " " + t.AssignedTo.LastName,
                MangakaId = t.CreatedById,
                MangakaAuthorName = t.CreatedBy.FirstName + " " + t.CreatedBy.LastName,
            }).ToListAsync();
        return taskList;
    }
    
    public async Task<bool> UpdateTaskStatus(Request.UpdateTaskStatusRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var task = await _dbContext.MangaTasks.FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.AssignedToId != userIdGuid) throw new UnauthorizedAccessException("You are not assigned to this task");
        if (task.Status != MangaTaskStatus.Available) throw new InvalidOperationException("Task is not available to be accepted");
        task.Status = request.Status;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SubmitTask(Request.SubmitTaskRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var task = await _dbContext.MangaTasks.FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.AssignedToId != userIdGuid) throw new UnauthorizedAccessException("You are not assigned to this task");
        if (task.Status != MangaTaskStatus.Processing && task.Status != MangaTaskStatus.Revising) 
            throw new InvalidOperationException("Task must be in Processing or Revising status to submit");

        task.submittedFileUrl = request.SubmittedFileUrl;
        task.SubmittedAt = DateTimeOffset.UtcNow;
        task.Status = MangaTaskStatus.Pending;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReviewTask(Request.ReviewTaskRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var task = await _dbContext.MangaTasks.FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.CreatedById != userIdGuid) throw new UnauthorizedAccessException("Only the creator can review this task");
        if (task.Status != MangaTaskStatus.Pending) throw new InvalidOperationException("Task must be in Pending status to review");

        if (request.IsApproved)
        {
            task.Status = MangaTaskStatus.Completed;
        }
        else
        {
            task.Status = MangaTaskStatus.Revising;
        }

        if (!string.IsNullOrEmpty(request.FeedbackContent))
        {
            var feedback = new Repository.Entity.Feedback
            {
                Id = Guid.NewGuid(),
                SenderId = userIdGuid,
                ReceiverId = task.AssignedToId,
                Content = request.FeedbackContent,
                CreatedAt = DateTimeOffset.UtcNow,
                MangaTaskId = task.Id
            };
            _dbContext.Feedbacks.Add(feedback);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
            
        if (string.IsNullOrEmpty(userId)) 
            throw new UnauthorizedAccessException("You must log in");

        return Guid.Parse(userId);

    }
}