using Manga.Repository.Data;
using Manga.Repository.Entity;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Series;

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

    public async Task<Response.CreateSeriesResponse> CreateSeries(Request.CreateSeriesRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId"|| x.Type == "UserId")?.Value;
        
        if(string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User is not login");
        
        var userIdGuid = Guid.Parse(userId!);
        
        var managkUser = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid);
        
        if(managkUser == null)
            throw new KeyNotFoundException("User not found");
        
        if (managkUser.Role != UserRole.Mangaka)
            throw new UnauthorizedAccessException("Only mangaka is create series");
        
        if (request.CategoryIds == null || request.CategoryIds.Count == 0)
            throw new ArgumentException("At least one category is required.");
        
        var categories = await _dbContext.Categories
            .Where(c => request.CategoryIds.Contains(c.Id))
            .ToListAsync();

        if (categories.Count != request.CategoryIds.Count)
            throw new KeyNotFoundException("One or more categories not found.");
        
        string? coverFileUrl = null;
        string? nameFileUrl = null;
        string? nameFilePublicId = null;

        if (request.CoverFile != null && request.CoverFile.Length > 0)
            coverFileUrl  = await _mediaService.UploadImageAsync(request.CoverFile);

        if (request.NameFile != null && request.NameFile.Length > 0)
        {
            var uploadResult  = await _mediaService.UploadFileAsync(request.NameFile);
            nameFileUrl = uploadResult.FileUrl;
            nameFilePublicId = uploadResult.PublicId;
        }
        
        var series = new Repository.Entity.Series
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CoverFile =  coverFileUrl,
            NameFile =  nameFileUrl,
            NameFilePublicId   = nameFilePublicId,
            Status = SeriesStatus.Processing,
            CreatedById =  userIdGuid,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        await _dbContext.Series.AddAsync(series);
        
        var categorySeriesList = categories.Select(c => new CategorySeries()
        {
            CategoryId = c.Id,
            SeriesId   = series.Id,
        }).ToList();

        await _dbContext.CategorySeries.AddRangeAsync(categorySeriesList);
        await _dbContext.SaveChangesAsync();

        return new Response.CreateSeriesResponse
        {
            SeriesId = series.Id,
            Title = series.Title,
            Description = series.Description,
            Categories  = categories.Select(c => c.Name).ToList(),
            CoverFile = series.CoverFile,
            NameFile = series.NameFile,
            NameFilePublicId   = series.NameFilePublicId,
            Status = series.Status,
            CreateAt = series.CreatedAt
        };
    }

    public async Task<List<Response.GetAllSeriesResponse>> GetAllSeries()
    {
        var seriesList = await _dbContext.Series
            .Where(s => !s.IsDeleted)
            .Include(s => s.CreatedBy)
            .Include(s => s.Chapters)
            .Include(s => s.CategorySeries)
                .ThenInclude(cs => cs.Category) 
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        
        return seriesList.Select(s => new Response.GetAllSeriesResponse()
        {
            SeriesId = s.Id,
            Title = s.Title,
            Categories = s.CategorySeries.Select(cs => cs.Category.Name).ToList(),
            CoverFile = s.CoverFile,
            Status = s.Status,
            MangakaName = s.CreatedBy.AuthorName ?? $"{s.CreatedBy.FirstName} {s.CreatedBy.LastName}",
            TotalChapters = s.Chapters.Count(c => !c.IsDeleted),
            CreateAt = s.CreatedAt
        }).ToList();
    }

    public async Task<Response.GetSeriesDetailsResponse> GetSeriesDetails(Guid seriesId)
    {
        var series = await _dbContext.Series
            .Where(s => s.Id == seriesId && !s.IsDeleted)
            .Include(s => s.CreatedBy)
            .Include(s => s.CategorySeries)
                .ThenInclude(cs => cs.Category) 
            .Include(s => s.Chapters.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync();

        if (series == null)
            throw new KeyNotFoundException("Series not found");

        var chapters = series.Chapters.OrderByDescending(c => c.ChapterNumber)
            .Select(c => new Response.ChapterSummary()
            {
                ChapterId = c.Id,
                ChapterNumber = c.ChapterNumber,
                Title = c.Title,
                Summary = c.Summary,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            }).ToList();
        
        return new Response.GetSeriesDetailsResponse
        {
            SeriesId = series.Id,
            Title = series.Title,
            Description = series.Description,
            Categories   = series.CategorySeries.Select(cs => cs.Category.Name).ToList(),
            CoverFile = series.CoverFile,
            NameFile = series.NameFile,
            MangakaName = series.CreatedBy.AuthorName ?? series.CreatedBy.FirstName + " " + series.CreatedBy.LastName,
            CreateAt = series.CreatedAt,
            Chapters =  chapters
        };
        
    }
}