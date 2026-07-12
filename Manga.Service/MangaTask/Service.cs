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
        if (series == null) throw new KeyNotFoundException("Series not found");
        if (series.Status == SeriesStatus.Rejected) throw new KeyNotFoundException("Series was rejected");

        var chapter = await _dbContext.Chapters
            .Include(x => x.Series)
            .FirstOrDefaultAsync(x => x.Id == request.ChapterId);
        if (chapter == null) throw new KeyNotFoundException("You cannot create a task. Chapter can not be found");
        if (chapter.SeriesId != series.Id)
            throw new InvalidDataException("Chapter does not belong to the specified series.");
        if (chapter.Series == null || (chapter.Series.Status != SeriesStatus.Approved &&
                                       chapter.Series.Status != SeriesStatus.Publishing &&
                                       chapter.Series.Status != SeriesStatus.Scheduled))
        {
            throw new InvalidDataException("You cannot create a task. Series must be approved or publishing");
        }

        if (chapter.Status != ChapterStatus.Processing && chapter.Status != ChapterStatus.Created)
            throw new InvalidOperationException(
                "You cannot create a task. Chapter status must be Processing status or Created status");
        
        var assignedAssistant = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.AssignedToId);
        if (assignedAssistant == null) throw new KeyNotFoundException("Assigned assistant not found");
        if (assignedAssistant.Role != UserRole.Assistant)
            throw new UnauthorizedAccessException("Task can only be assigned to Assistant");

        var checkAssistant =
            await _dbContext.MangaTasks.AnyAsync(x =>
                x.AssignedToId == request.AssignedToId && x.ChapterId == chapter.Id && x.IsDeleted == false);
        if (checkAssistant)
            throw new InvalidOperationException("This assistant has already been assigned a task in this chapter.");

        if (request.From <= 0)
            throw new InvalidDataException("From page must be greater than 0.");
        
        if (request.To < request.From)
            throw new InvalidDataException("To page must be greater than or equal to From page.");

        if (chapter.TotalPage.HasValue && request.To > chapter.TotalPage.Value)
            throw new InvalidDataException("To page cannot exceed the total pages of the chapter.");

        var existingTasks = await _dbContext.MangaTasks
            .Where(x => x.ChapterId == chapter.Id && x.IsDeleted == false)
            .ToListAsync();
            
        foreach (var t in existingTasks)
        {
            var parts = t.TaskDescription?.Split('-');
            if (parts != null && parts.Length == 2 && int.TryParse(parts[0], out int existingFrom) && int.TryParse(parts[1], out int existingTo))
            {
                if (request.From <= existingTo && request.To >= existingFrom)
                {
                    throw new InvalidOperationException($"The requested page range ({request.From}-{request.To}) overlaps with an existing task ({existingFrom}-{existingTo}).");
                }
            }
        }
        if (request.Deadline <= DateTimeOffset.UtcNow)
        {
            throw new InvalidDataException("Deadline must be a future date.");
        }

        if (request.Deadline >= chapter.Deadline)
        {
            throw new InvalidDataException("Deadline task must be before Deadline Chapter.");
        }

        if (request.AmountIncome <= 0)
        {
            throw new InvalidDataException("Income amount must be greater than zero.");
        }

        var mangaTask = new Repository.Entity.MangaTask()
        {
            Id = Guid.NewGuid(),
            TaskTitle = request.TaskTitle,
            TaskDescription = request.From.ToString() + "-" + request.To.ToString(),
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
                Status = IncomeStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };
        chapter.Status = ChapterStatus.Processing;
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
            .AsNoTracking()
            .Where(x => x.Id == request.TaskId)
            .Select(x => new Response.GetTaskDetailsResponse
            {
                Id = x.Id,
                SeriesTitle = x.Chapter.Series.Title,
                ChapterId = x.ChapterId,
                ChapterTitle = x.Chapter.Title,
                ChapterNumber = x.Chapter.ChapterNumber,
                ManuscriptFileUrl = x.Chapter.ManuscriptFileUrl,
                TotalPages = x.Chapter.TotalPage,

                TaskTitle = x.TaskTitle,
                TaskDescription = x.TaskDescription,
                SubmittedFileUrl = x.submittedFileUrl,
                Status = x.Status,
                Deadline = x.Deadline,
                AssignedAt = x.AssignedAt,
                SubmittedAt = x.SubmittedAt,

                CreatedById = x.CreatedById,
                AssignedToId = x.AssignedToId,
                AssistantName = x.AssignedTo.FirstName + " " + x.AssignedTo.LastName,
                MangakaAuthorName = x.CreatedBy.FirstName + " " + x.CreatedBy.LastName,
                IncomeAmount = x.Income.Amount,

                Feedback = x.Feedbacks
                    .OrderBy(f => f.CreatedAt)
                    .Select(f => new Response.FeedbackSummaryResponse
                    {
                        FeedbackId = f.Id,
                        SenderId = f.SenderId,
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
    public async Task<List<Response.GetTaskDetailsResponse>> GetTaskList(Request.GetTaskListRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null) throw new KeyNotFoundException("User not found");
        if (user.Role != UserRole.Mangaka && user.Role != UserRole.Assistant)
            throw new UnauthorizedAccessException("You don't have permission to access this action");

        IQueryable<Repository.Entity.MangaTask> query = _dbContext.MangaTasks.AsNoTracking().AsQueryable();
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
            .Select(x => new Response.GetTaskDetailsResponse()
            {
                Id = x.Id,
                SeriesTitle = x.Chapter.Series.Title,
                ChapterId = x.ChapterId,
                ChapterTitle = x.Chapter.Title,
                ChapterNumber = x.Chapter.ChapterNumber,
                ManuscriptFileUrl = x.Chapter.ManuscriptFileUrl,
                TotalPages = x.Chapter.TotalPage,

                TaskTitle = x.TaskTitle,
                TaskDescription = x.TaskDescription,
                SubmittedFileUrl = x.submittedFileUrl,
                Status = x.Status,
                Deadline = x.Deadline,
                AssignedAt = x.AssignedAt,
                SubmittedAt = x.SubmittedAt,

                CreatedById = x.CreatedById,
                AssignedToId = x.AssignedToId,
                AssistantName = x.AssignedTo.FirstName + " " + x.AssignedTo.LastName,
                MangakaAuthorName = x.CreatedBy.FirstName + " " + x.CreatedBy.LastName,
                IncomeAmount = x.Income.Amount,
            }).ToListAsync();
        return taskList;
    }

    public async Task<bool> UpdateTaskStatus(Request.UpdateTaskStatusRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var task = await _dbContext.MangaTasks.FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.AssignedToId != userIdGuid) throw new UnauthorizedAccessException("You are not assigned to this task");
        if (task.Status != MangaTaskStatus.Available)
            throw new InvalidOperationException("Task is not available to be accepted or updated");

        if (request.Status != MangaTaskStatus.Processing && request.Status != MangaTaskStatus.Rejected)
        {
            throw new InvalidOperationException(
                "You can only Accept (Processing) or Decline (Rejected) an Available task.");
        }

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
        if (task.Status != MangaTaskStatus.Processing && task.Status != MangaTaskStatus.Revising &&
            task.Status != MangaTaskStatus.Pending)
            throw new InvalidOperationException("Task can't be submitted.");

        if (request.SubmittedFileUrl == null || request.SubmittedFileUrl.Length <= 0)
        {
            throw new InvalidOperationException("You must supply a submitted file ");
        }

        var currentDate = DateTimeOffset.UtcNow;
        if (currentDate > task.Deadline) throw new InvalidOperationException("The deadline for this task has passed.");

        var submittedFile = await _mediaService.UploadFileAsync(request.SubmittedFileUrl);
        task.submittedFileUrl = submittedFile.FileUrl;
        task.SubmittedAt = currentDate;
        //task.Status = MangaTaskStatus.Pending;
        if (task.Status == MangaTaskStatus.Revising || currentDate >= task.Deadline)
        {
            task.Status = MangaTaskStatus.Pending;
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReviewTask(Request.ReviewTaskRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var task = await _dbContext.MangaTasks
            .Include(x => x.Chapter)
            .ThenInclude(c => c.Series)
            .ThenInclude(s => s.PublishingSchedule)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.CreatedById != userIdGuid)
            throw new UnauthorizedAccessException("Only the creator can review this task");
        if (task.Status != MangaTaskStatus.Pending)
            throw new InvalidOperationException("Task must be in Pending status to review");
       
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var currentDate = DateTimeOffset.UtcNow;
            if (request.IsApproved)
            {
                task.Status = MangaTaskStatus.Completed;
                var income = await _dbContext.Incomes.FirstOrDefaultAsync(x => x.MangaTaskId == task.Id);
                if (income != null)
                {
                    income.Date = currentDate;
                    income.Status = IncomeStatus.Paid;
                }
            }
            else
            {
                var isOverdue = currentDate >= task.Deadline;
                if (isOverdue)
                {
                    var extensionCount = await _dbContext.Feedbacks.CountAsync(f => 
                        f.MangaTaskId == task.Id && 
                        f.Content == "Deadline Extended due to rejection" &&
                        f.SenderId == userIdGuid);

                    if (extensionCount >= 2)
                    {
                        task.Status = MangaTaskStatus.Unsatisfactory;
                        task.UpdatedAt = currentDate;
                        
                        var income = await _dbContext.Incomes.FirstOrDefaultAsync(x => x.MangaTaskId == task.Id);
                        if (income != null)
                        {
                            income.Amount = income.Amount * 0.7m;
                            income.Date = currentDate;
                            income.Status = IncomeStatus.Paid;
                        }
                    }
                    else
                    {
                        task.Status = MangaTaskStatus.Revising;
                        
                        var publishPeriod = task.Chapter?.Series?.PublishingSchedule?.PublishPeriod;
                        if (!string.IsNullOrEmpty(publishPeriod))
                        {
                            var isWeekly = publishPeriod.Equals("Weekly", StringComparison.OrdinalIgnoreCase);
                            var maxDeadline = task.Chapter!.Deadline.AddDays(isWeekly ? -1 : -3);
                            var extensionDays = isWeekly ? 1 : 3;
                            var newDeadline = task.Deadline.AddDays(extensionDays);
                            
                            if (newDeadline < currentDate.AddHours(24)) newDeadline = currentDate.AddHours(24);
                            if (newDeadline > maxDeadline) newDeadline = maxDeadline;

                            task.Deadline = newDeadline;
                            task.UpdatedAt = currentDate;
                            
                            var feedback = new Repository.Entity.Feedback
                            {
                                Id = Guid.NewGuid(),
                                SenderId = userIdGuid,
                                Content = "Deadline Extended due to rejection",
                                CreatedAt = currentDate,
                                MangaTaskId = task.Id,
                                ChapterId = task.ChapterId,
                                SeriesId = task.Chapter?.SeriesId,
                                Type = FeedbackType.StatusChange,
                                IsRead = false
                            };
                            _dbContext.Feedbacks.Add(feedback);
                        }

                    }
                }
                else
                {
                    task.Status = MangaTaskStatus.Revising;
                }
            }

            if (!string.IsNullOrEmpty(request.FeedbackContent))
            {
                var feedback = new Repository.Entity.Feedback
                {
                    Id = Guid.NewGuid(),
                    SenderId = userIdGuid,
                    Content = request.FeedbackContent,
                    CreatedAt = currentDate,
                    MangaTaskId = task.Id,
                    ChapterId = task.ChapterId,
                    SeriesId = task.Chapter?.SeriesId,
                    Type = request.IsApproved ? FeedbackType.StatusChange : FeedbackType.Manual,
                    IsRead = false
                };
                _dbContext.Feedbacks.Add(feedback);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }
        return false;
    }

    public async Task<Response.GetTotalTaskResponse> GetTotalTask(Request.GetTaskListRequest request)
    {
        var chapterExists = await _dbContext.Chapters.AnyAsync(x => x.Id == request.ChapterId);
        if (!chapterExists) throw new KeyNotFoundException("Chapter not found");

        var total = await _dbContext.MangaTasks.Where(x => x.ChapterId == request.ChapterId && x.IsDeleted == false)
            .CountAsync();
        var checkStatus = await _dbContext.MangaTasks
            .Where(x => x.ChapterId == request.ChapterId && x.IsDeleted == false && x.Status == request.Status)
            .CountAsync();

        return new Response.GetTotalTaskResponse()
        {
            Total = total,
            NumberOfStatus = checkStatus
        };
    }

    public async Task<bool> UpdateMangaTask(Request.UpdateMangaTaskRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null) throw new UnauthorizedAccessException("Unauthorized");
        if (user.Role != UserRole.Mangaka) throw new UnauthorizedAccessException("Only Mangaka is allowed");

        var task = await _dbContext.MangaTasks
            .Include(x => x.Chapter)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.CreatedById != userIdGuid) throw new UnauthorizedAccessException("Only the creator can update this task");

        if (task.Status == MangaTaskStatus.Completed)
            throw new InvalidOperationException("Cannot update deadline for a completed task.");
        if (task.Status == MangaTaskStatus.Rejected)
            throw new InvalidOperationException("Cannot update deadline for a rejected task.");

        if (request.Deadline <= DateTimeOffset.UtcNow)
        {
            throw new InvalidDataException("Deadline must be a future date.");
        }

        if (task.Chapter == null)
        {
            throw new KeyNotFoundException("Chapter not found for this task.");
        }

        if (request.Deadline >= task.Chapter.Deadline)
        {
            throw new InvalidDataException("Deadline task must be before Deadline Chapter.");
        }

        task.Deadline = request.Deadline;
        task.UpdatedAt = DateTimeOffset.UtcNow;

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

    public async Task<bool> ReassignTaskAsync(Request.ReassignTaskRequest request)
    {
        var userIdGuid = GetCurrentUserId();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        if (user == null || user.Role != UserRole.Mangaka) throw new UnauthorizedAccessException("Only Mangaka can reassign tasks.");

        var task = await _dbContext.MangaTasks.FirstOrDefaultAsync(x => x.Id == request.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");
        if (task.CreatedById != userIdGuid) throw new UnauthorizedAccessException("Only the creator can reassign this task");
        
        if (task.Status != MangaTaskStatus.Available && task.Status != MangaTaskStatus.Rejected)
            throw new InvalidOperationException("Task must be in Available or Rejected status to be reassigned.");

        var newAssistant = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == request.NewAssistantId);
        if (newAssistant == null || newAssistant.Role != UserRole.Assistant) 
            throw new InvalidOperationException("New assigned user must be an Assistant.");

        task.AssignedToId = request.NewAssistantId;
        task.Status = MangaTaskStatus.Available;
        task.AssignedAt = DateTimeOffset.UtcNow;
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<List<Response.GetPageRangeResponse>> GetPageRange(Request.GetPageRangeRequest request)
    {
        var chapterExists = await _dbContext.Chapters.AnyAsync(x => x.Id == request.ChapterId);
        if (!chapterExists) throw new KeyNotFoundException("Chapter not found");

        var tasks = await _dbContext.MangaTasks
            .Include(t => t.AssignedTo)
            .Where(x => x.ChapterId == request.ChapterId && x.IsDeleted == false)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<Response.GetPageRangeResponse>();
        foreach (var task in tasks)
        {
            var parts = task.TaskDescription?.Split('-');
            if (parts != null && parts.Length == 2 && int.TryParse(parts[0], out int from) && int.TryParse(parts[1], out int to))
            {
                result.Add(new Response.GetPageRangeResponse
                {
                    TaskId = task.Id,
                    TaskTitle = task.TaskTitle,
                    From = from,
                    To = to,
                    Status = task.Status,
                    AssignedToId = task.AssignedToId,
                    AssistantName = ((task.AssignedTo.FirstName ?? "") + " " + (task.AssignedTo.LastName ?? "")).Trim()
                });
            }
        }
        return result.OrderBy(x => x.From).ToList();
    }
}