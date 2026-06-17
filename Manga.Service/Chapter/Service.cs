using Manga.Repository.Data;
using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Chapter;

public class Service: IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MediaService.IService _mediaService;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor, MediaService.IService mediaService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _mediaService = mediaService;
    }

    public async Task<Response.CreateChapterResponse> CreateChapter(Guid seriesId, Request.CreateChapterRequest request)
    {
        // check user
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
        
        if(userId == null)
            throw new UnauthorizedAccessException("User is not login");
        
        var userIdGuid = Guid.Parse(userId!);

        var mangakaUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);

        if (mangakaUser == null)
            throw new KeyNotFoundException("User not found");

        if (mangakaUser.Role != UserRole.Mangaka)
            throw new UnauthorizedAccessException("Only Mangaka can create chapter");

        var series = await _dbContext.Series.FirstOrDefaultAsync(x => x.Id == seriesId);

        if (series == null)
            throw new KeyNotFoundException("Series not found");
        
        if (series.CreatedById != userIdGuid)
            throw new UnauthorizedAccessException("You are not the creator for series");
        
        if(series.Status != SeriesStatus.Approved && series.Status != SeriesStatus.Publishing)
            throw new Exception("Series need approved before create chapter");
        
        if(request.Deadline <= DateTimeOffset.UtcNow)
            throw new ArgumentException("Deadline must be in the future");
        
        var lastChapterNumber = await  _dbContext.Chapters.Where(c => c.SeriesId == seriesId && !c.IsDeleted)
            .MaxAsync(c => (int?)c.ChapterNumber) ?? 0;
        
        var nextChapterNumber = lastChapterNumber + 1;
        
        string? manuscriptFileUrl = null;
        string? chapterFileUrl = null;
        
        if (request.ManuscriptFileUrl != null && request.ManuscriptFileUrl.Length > 0)
        {
            var result = await _mediaService.UploadFileAsync(request.ManuscriptFileUrl);
            manuscriptFileUrl = result.FileUrl;
        }
        
        if (request.ChapterFileUrl != null && request.ChapterFileUrl.Length > 0)
        {
            var result = await _mediaService.UploadFileAsync(request.ChapterFileUrl);
            chapterFileUrl = result.FileUrl;
        }

        var chapter = new Repository.Entity.Chapter()
        {
            Id = Guid.NewGuid(),
            ChapterNumber = nextChapterNumber,
            Title = request.Title,
            Summary = request.Summary,
            ManuscriptFileUrl = manuscriptFileUrl,
            ChapterFileUrl = chapterFileUrl,
            Status = ChapterStatus.Created,
            SeriesId = seriesId,
            Deadline = request.Deadline,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        await _dbContext.Chapters.AddAsync(chapter);
        await _dbContext.SaveChangesAsync();

        return new Response.CreateChapterResponse()
        {
            ChapterId = chapter.Id,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Summary = chapter.Summary,
            ManuscriptFileUrl = manuscriptFileUrl,
            ChapterFileUrl = chapterFileUrl,
            Status = chapter.Status,
            SeriesId = seriesId,
            SeriesTitle = series.Title,
            Deadline = chapter.Deadline,
            CreateAt = chapter.CreatedAt
        };

    }

    public async Task<List<Response.GetAllChaptersResponse>> GetAllChapters(Guid seriesId)
    {
        var seriesExist = await _dbContext.Series.AnyAsync(s => s.Id == seriesId && !s.IsDeleted);
        
        if (!seriesExist)
            throw new KeyNotFoundException("Series not found");
        
        var chapter = await _dbContext.Chapters
            .Where(c => c.SeriesId == seriesId && !c.IsDeleted)
            .Include(c => c.MangaTasks)
            .OrderBy(c => c.ChapterNumber)
            .ToListAsync();

        var result = chapter.Select(c => new Response.GetAllChaptersResponse()
        {
            ChapterId = c.Id,
            ChapterNumber = c.ChapterNumber,
            Title = c.Title,
            Summary = c.Summary,
            Status = c.Status,
            TotalTask = c.MangaTasks.Count(t => !t.IsDeleted),
            CreatedAt = c.CreatedAt
        }).ToList();

        return result;
    }

    public async Task<Response.GetChapterDetailsResponse> GetChapterDetails(Guid seriesId, Guid chapterId)
    {
        var chapter = await _dbContext.Chapters
            .Where(c => c.Id == chapterId && c.SeriesId == seriesId && !c.IsDeleted)
            .Include(c => c.Series)
            .Include(c => c.MangaTasks.Where(t => !t.IsDeleted))
            .ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync();

        if (chapter == null)
            throw new KeyNotFoundException("Chapter not found");
        
        var tasks = chapter.MangaTasks
            .OrderBy(t => t.CreatedAt)
            .Select(t => new Response.TaskSummary()
            {
                MangaTaskId = t.Id,
                TaskTitle = t.TaskTitle,
                TaskDescription = t.TaskDescription,
                Status = t.Status,
                Deadline = t.Deadline,
                AssignedTo = t.AssignedTo.FirstName + " " + t.AssignedTo.LastName,
            }).ToList();

        return new Response.GetChapterDetailsResponse()
        {
            ChapterId = chapter.Id,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Summary = chapter.Summary,
            ManuscriptFileUrl = chapter.ManuscriptFileUrl,
            ChapterFileUrl = chapter.ChapterFileUrl,
            Status = chapter.Status,
            SeriesId = chapter.SeriesId,
            SeriesTitle = chapter.Series.Title,
            CreatedAt = chapter.CreatedAt,
            Deadline = chapter.Deadline,
            UpdatedAt = chapter.UpdatedAt,
            Tasks = tasks
        };
    }


    public async Task<Response.UpdateChapterResponse> UpdateChapter(Guid seriesId, Guid chapterId, Request.UpdateChapterRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
        
        if(userId == null)
            throw new UnauthorizedAccessException("User not login");
        
        var userIdGuid = Guid.Parse(userId);
        
        var tantou = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        
        if(tantou == null)
            throw new UnauthorizedAccessException("User not found");

        if (tantou.Role != UserRole.Tantou)
            throw new UnauthorizedAccessException("Only Tantou can update chapter.");

        var chapter = await _dbContext.Chapters.Where(x => x.Id == chapterId && !x.IsDeleted)
            .Include(s => s.Series)
            .Include(s => s.MangaTasks.Where(t => !t.IsDeleted))
            .ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync();
        
        if (chapter == null)
            throw new KeyNotFoundException("Chapter not found");

        if (request.ChapterFileUrl != null && request.ChapterFileUrl.Length > 0)
        {
            var uploadResult = await _mediaService.UploadFileAsync(request.ChapterFileUrl);
            chapter.ChapterFileUrl = uploadResult.FileUrl;
        }
        
        if(request.Status.HasValue)
            chapter.Status = request.Status.Value;
        
        chapter.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

        var tasks = chapter.MangaTasks
            .OrderBy(t => t.CreatedAt)
            .Select(t => new Response.TaskSummary()
            {
                MangaTaskId = t.Id,
                TaskTitle = t.TaskTitle,
                TaskDescription = t.TaskDescription,
                Status = t.Status,
                Deadline = t.Deadline,
                AssignedTo = $"{t.AssignedTo.FirstName}{t.AssignedTo.LastName}",
            }).ToList();

        return new Response.UpdateChapterResponse()
        {
            ChapterId = chapter.Id,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Summary = chapter.Summary,
            ManuscriptFileUrl = chapter.ManuscriptFileUrl,
            ChapterFileUrl = chapter.ChapterFileUrl,
            Status = chapter.Status,
            SeriesId = chapter.SeriesId,
            SeriesTitle = chapter.Series.Title,
            UpdatedByName     = $"{tantou.FirstName} {tantou.LastName}",
            CreatedAt = chapter.CreatedAt,
            UpdatedAt = chapter.UpdatedAt,
            Tasks = tasks
        };
    }
}