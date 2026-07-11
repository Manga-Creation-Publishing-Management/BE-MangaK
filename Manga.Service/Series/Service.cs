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
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User is not login");

        var userIdGuid = Guid.Parse(userId);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userIdGuid)
                   ?? throw new KeyNotFoundException("User not found");

        var query = _dbContext.Series
            .Where(s => !s.IsDeleted)
            .Include(s => s.CreatedBy)
            .Include(s => s.Chapters)
            .Include(s => s.CategorySeries)
                .ThenInclude(cs => cs.Category) 
            .OrderByDescending(s => s.CreatedAt)
            .AsQueryable();

        if (user.Role == UserRole.Mangaka)
            query = query.Where(s => s.CreatedById == userIdGuid);
        
        if (user.Role == UserRole.Reader)
            query = query.Where(s => s.Status == SeriesStatus.Publishing);
        
        var seriesList = await query.ToListAsync();
        
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
            .Include(s => s.PublishingSchedule)
            .Include(s => s.CategorySeries)
                .ThenInclude(cs => cs.Category) 
            .Include(s => s.Chapters.Where(c => !c.IsDeleted))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (series == null)
            throw new KeyNotFoundException("Series not found");

        var chapters = series.Chapters.OrderBy(c => c.ChapterNumber)
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
            Status   = series.Status,
            MangakaName = series.CreatedBy.AuthorName ?? series.CreatedBy.FirstName + " " + series.CreatedBy.LastName,
            PublishDate = series.PublishingSchedule?.PublishDate,
            PublishPeriod = series.PublishingSchedule?.PublishPeriod,
            CreateAt = series.CreatedAt,
            Chapters =  chapters
        };
        
    }

    public async Task<List<Response.GetAllSeriesResponse>> GetSeriesByTitle(string title)
    {
        var seriesList = await _dbContext.Series
            .Where(s => !s.IsDeleted && s.Title.ToLower().Contains(title.ToLower()))
            .Include(s => s.CreatedBy)
            .Include(s => s.Chapters)
            .Include(s => s.CategorySeries)
            .ThenInclude(s => s.Category)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        
        if (!seriesList.Any())
            throw new KeyNotFoundException($"No found series");

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

    public async Task<Response.ReviewSeriesResponse> ReviewSeriesByTantouEditor(Guid seriesId, Request.ReviewSeriesRequest request)
    {
        var user = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(u => u.Type == "userId" || u.Type == "UserId")?.Value;
        
        if(String.IsNullOrEmpty(user))
            throw new UnauthorizedAccessException("User not login");
        
        var userIdGuid = Guid.Parse(user!);
        
        var editor = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
        
        if(editor == null)
            throw new UnauthorizedAccessException("User not found");
        
        if(editor.Role != UserRole.Tantou)
            throw new UnauthorizedAccessException("Only TantouEditor can review review series");
        
        var series = await _dbContext.Series
            .Include(s => s.CreatedBy)
            .FirstOrDefaultAsync(s =>
                s.Id == seriesId &&
                !s.IsDeleted);

        if (series == null)
            throw new KeyNotFoundException("Series not found");

        if (series.CreatedBy.SupervisorId != userIdGuid)
            throw new UnauthorizedAccessException(
                "You are not assigned to review this series.");
        
        if(series.Status != SeriesStatus.Processing)
            throw new UnauthorizedAccessException($"Series must be in processing status. Current status is: {series.Status}");
        
        if (series.ReviewedById != null && series.ReviewedById != userIdGuid)
            throw new UnauthorizedAccessException("This series is already being handled by another Tantou Editor.");
        
        if (request.IsApproved)
        {
            series.Status = SeriesStatus.Pending;
            series.ReviewedById = userIdGuid;
        }else
        {
            series.Status = SeriesStatus.Rejected;
        }
        series.UpdatedAt = DateTimeOffset.UtcNow;
        
        //
        var feedbackCreated = false;
        
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            var feedback = new Repository.Entity.Feedback
            {
                Id        = Guid.NewGuid(),
                SenderId  = editor.Id,
                Content   = request.Note,
                SeriesId  = series.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await _dbContext.Feedbacks.AddAsync(feedback);
            feedbackCreated = true;
        }
        //
        
        await _dbContext.SaveChangesAsync();

        return new Response.ReviewSeriesResponse()
        {
            SeriesId = series.Id,
            Title = series.Title,
            Status = series.Status,
            Note = request.Note,
            FeedbackCreated = feedbackCreated,
            ReviewerName = $"{editor.FirstName} {editor.LastName}",
            ReviewerRole = editor.Role.ToString(),
            UpdatedAt = series.UpdatedAt.Value
        };
    }

    public async Task<Response.ReviewSeriesResponse> ApprovedSeriesByEditorialBoard(Guid seriesId, Request.ReviewSeriesRequest request)
    {
        var user = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(u => u.Type == "userId" || u.Type == "UserId")?.Value;
        
        if(String.IsNullOrEmpty(user))
            throw new UnauthorizedAccessException("User not login");
        
        var userIdGuid = Guid.Parse(user!);
        
        var board = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
        
        if(board == null)
            throw new UnauthorizedAccessException("User not found");
        
        if(board.Role != UserRole.Editorial)
            throw new UnauthorizedAccessException("Only EditorialBoard can review review series");
        
        var series = await _dbContext.Series.FirstOrDefaultAsync(s => s.Id == seriesId && !s.IsDeleted);
        
        if(series == null)
            throw new KeyNotFoundException("Series not found");
        
        if(series.Status != SeriesStatus.Pending)
            throw new UnauthorizedAccessException($"Series must be reviewed by TantouEditor first. Current status: {series.Status}");

        if (request.IsApproved)
        {
            series.Status = SeriesStatus.Approved;
            series.ApprovedById = userIdGuid;
        }
        else
        {
            series.Status = SeriesStatus.Rejected;
        }
        series.UpdatedAt = DateTimeOffset.UtcNow;
        
        //
        var feedbackCreated = false;
        
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            var feedback = new Repository.Entity.Feedback
            {
                Id        = Guid.NewGuid(),
                SenderId  = board.Id,
                Content   = request.Note,
                SeriesId  = series.Id,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await _dbContext.Feedbacks.AddAsync(feedback);
            feedbackCreated = true;
        }
        //
        
        await _dbContext.SaveChangesAsync();
        
        return new Response.ReviewSeriesResponse
        {
            SeriesId     = series.Id,
            Title        = series.Title,
            Status       = series.Status,
            Note         = request.Note,
            FeedbackCreated = feedbackCreated,
            ReviewerName = $"{board.FirstName} {board.LastName}",
            ReviewerRole = nameof(UserRole.Editorial),
            UpdatedAt    = series.UpdatedAt.Value
        };
    }

    public async Task<List<Response.GetAllSeriesResponse>> FilterSeriesByStatus(SeriesStatus status)
    {
        var user = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(u => u.Type == "userId" || u.Type == "UserId")?.Value;
        
        if(String.IsNullOrEmpty(user))
            throw new UnauthorizedAccessException("User not login");
        
        var userIdGuid = Guid.Parse(user!);
        
        var users = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
        
        if(users == null)
            throw new UnauthorizedAccessException("User not found");
        
        if(users.Role == UserRole.Reader)
            throw new UnauthorizedAccessException("Reader is not allowed to filter series by status ");
        var seriesList = await _dbContext.Series
            .Where(s => s.Status == status && !s.IsDeleted)
            .Include(s => s.CreatedBy)
            .Include(s => s.Chapters)
            .Include(s => s.CategorySeries)
            .ThenInclude(cs => cs.Category)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        if (!seriesList.Any())
            throw new KeyNotFoundException($"No Series found with {status}");

        return seriesList.Select(s => new Response.GetAllSeriesResponse()
        {
            SeriesId = s.Id,
            Title = s.Title,
            MangakaName = s.CreatedBy.AuthorName ?? $"{s.CreatedBy.FirstName}{s.CreatedBy.LastName}",
            Categories = s.CategorySeries.Select(cs => cs.Category.Name).ToList(),
            CoverFile = s.CoverFile,
            TotalChapters = s.Chapters.Count(c => !c.IsDeleted),
            Status = s.Status,
            CreateAt = s.CreatedAt
        }).ToList();
    }
    
    public async Task<List<Response.GetAllSeriesResponse>> GetAllSeriesByCategory(Request.GetSeriesByCategoryRequest request)
    {
        if(request.CategoryIds == null || request.CategoryIds.Count == 0)
            throw new ArgumentNullException("At least one category id is required.");
        
        var categoryCount = await _dbContext.Categories
            .CountAsync(x => request.CategoryIds.Contains(x.Id));
        
        if(categoryCount != request.CategoryIds.Count)
            throw new KeyNotFoundException("One or more category is not found.");
            
        var seriesList = await _dbContext.Series
            .Where(s => s.CategorySeries.Any(cs => request.CategoryIds.Contains(cs.CategoryId)))
            .Include(s => s.CreatedBy)
            .Include(s => s.Chapters)
            .Include(s => s.CategorySeries)
            .ThenInclude(cs => cs.Category)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        
        if(!seriesList.Any())
            throw new KeyNotFoundException("No series found");

        return seriesList.Select(s => new Response.GetAllSeriesResponse()
        {
            SeriesId = s.Id,
            Title = s.Title,
            Categories = s.CategorySeries.Select(cs => cs.Category.Name).ToList(),
            CoverFile = s.CoverFile,
            Status = s.Status,
            MangakaName = s.CreatedBy.AuthorName ?? $"{s.CreatedBy.FirstName}{s.CreatedBy.LastName}",
            TotalChapters = s.Chapters.Count(c => !c.IsDeleted),
            CreateAt = s.CreatedAt
        }).ToList();
    }

    public async Task<Response.CancelSeriesResponse> CancelSeries(Guid seriesId, Request.CancelSeriesRequest request)
    {
        var userId = _httpContextAccessor.HttpContext!.User.Claims
            .FirstOrDefault(c => c.Type == "userId" || c.Type == "UserId")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not login");

        var userIdGuid = Guid.Parse(userId);
        
        var board = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);

        if (board == null)
            throw new UnauthorizedAccessException("User not found");
        
        if (board.Role != UserRole.Editorial)
            throw new UnauthorizedAccessException("Only EditorialBoard can cancel series");
        
        var series = await _dbContext.Series
            .Include(s => s.PublishingSchedule)
            .FirstOrDefaultAsync(s => s.Id == seriesId && !s.IsDeleted);

        if (series == null)
            throw new KeyNotFoundException("Series not found");
        
        if (series.Status == SeriesStatus.Cancelled)
            throw new InvalidOperationException("Series is already cancelled");

        if (series.Status == SeriesStatus.Rejected)
            throw new InvalidOperationException("Cannot cancel a rejected series");
        
        if (series.PublishingSchedule != null && !series.PublishingSchedule.IsDeleted)
        {
            series.PublishingSchedule.IsDeleted = true;
            series.PublishingSchedule.UpdatedAt = DateTimeOffset.UtcNow;
        }
        
        series.Status = SeriesStatus.Cancelled;
        series.UpdatedAt = DateTimeOffset.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            var feedback = new Repository.Entity.Feedback
            {
                Id        = Guid.NewGuid(),
                SenderId  = userIdGuid,
                Content   = request.Reason,
                SeriesId  = series.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                Type      = FeedbackType.StatusChange,
                IsRead    = false,
            };
            await _dbContext.Feedbacks.AddAsync(feedback);
        }

        await _dbContext.SaveChangesAsync();

        return new Response.CancelSeriesResponse
        {
            SeriesId        = series.Id,
            Title           = series.Title,
            Status          = series.Status,
            Reason          = request.Reason,
            CancelledByName = board.AuthorName ?? $"{board.FirstName} {board.LastName}",
            CancelledAt     = series.UpdatedAt!.Value
        };
    }

    public async Task<object> SearchSeriesByVoting(Request.SearchSeriesByVotingRequest request)
    {
        if (request.MinRate < 0 || request.MaxRate > 5 || request.MinRate > request.MaxRate)
            throw new ArgumentException(
                "MinRate must be 0-5, MaxRate must be 0-5 and MinRate cannot be greater than MaxRate");
        
        var now = DateTimeOffset.UtcNow;
        
        var seriesList = await _dbContext.Series
            .Where(s => s.Status == SeriesStatus.Publishing && !s.IsDeleted)
            .Include(s => s.PublishingSchedule)
            .Include(s => s.Chapters.Where(c => !c.IsDeleted))
            .ThenInclude(c => c.ChapterVotes)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        
        if (request.RankingType == "Weekly")
            return GetWeeklyRanking(seriesList, now, request.MinRate, request.MaxRate);

        if (request.RankingType == "Monthly")
            return GetMonthlyRanking(seriesList, now, request.MinRate, request.MaxRate);

        // Both là cả weekly và monthly
        return new Response.SearchSeriesByVotingResponse
        {
            WeeklyRanking  = GetWeeklyRanking(seriesList, now, request.MinRate, request.MaxRate),
            MonthlyRanking = GetMonthlyRanking(seriesList, now, request.MinRate, request.MaxRate),
        };
    }
    
    private List<ChapterVoting.Response.WeeklyRankingResponse> GetWeeklyRanking(
        List<Repository.Entity.Series> seriesList, DateTimeOffset now, double minRate, double maxRate)
    {
        return seriesList
            .Where(s => s.PublishingSchedule?.PublishPeriod == "Weekly")
            .Select(series =>
            {
                var publishDate       = series.PublishingSchedule!.PublishDate;
                var daysPassed        = (now - publishDate).TotalDays;
                var weeklyPeriodStart = publishDate.AddDays((int)(daysPassed / 7) * 7);
                var weeklyPeriodEnd   = weeklyPeriodStart.AddDays(7);

                var weeklyVotes = series.Chapters
                    .SelectMany(c => c.ChapterVotes)
                    .Where(v => v.VoteAt >= weeklyPeriodStart && v.VoteAt < weeklyPeriodEnd)
                    .ToList();

                return new ChapterVoting.Response.WeeklyRankingResponse
                {
                    SeriesId          = series.Id,
                    Title             = series.Title,
                    CoverFile         = series.CoverFile,
                    WeeklyAverageRate = weeklyVotes.Count > 0 ? Math.Round(weeklyVotes.Average(v => v.Rate), 2) : 0,
                    WeeklyTotalVotes  = weeklyVotes.Count,
                    WeeklyPeriodStart = weeklyPeriodStart,
                    WeeklyPeriodEnd   = weeklyPeriodEnd,
                };
            }).Where(r => r.WeeklyAverageRate >= minRate && r.WeeklyAverageRate <= maxRate)
            .OrderByDescending(r => r.WeeklyAverageRate)
            .ThenByDescending(r => r.WeeklyTotalVotes)
            .ToList();
    }

    private List<ChapterVoting.Response.MonthlyRankingResponse> GetMonthlyRanking(
         List<Repository.Entity.Series> seriesList, DateTimeOffset now, double minRate, double maxRate)
    {
        return seriesList
            .Where(s => s.PublishingSchedule?.PublishPeriod == "Monthly")
            .Select(series =>
            {
                var publishDate        = series.PublishingSchedule!.PublishDate;
                var daysPassed         = (now - publishDate).TotalDays;
                var monthlyPeriodStart = publishDate.AddDays((int)(daysPassed / 30) * 30);
                var monthlyPeriodEnd   = monthlyPeriodStart.AddDays(30);

                var monthlyVotes = series.Chapters
                    .SelectMany(c => c.ChapterVotes)
                    .Where(v => v.VoteAt >= monthlyPeriodStart && v.VoteAt < monthlyPeriodEnd)
                    .ToList();

                return new ChapterVoting.Response.MonthlyRankingResponse
                {
                    SeriesId           = series.Id,
                    Title              = series.Title,
                    CoverFile          = series.CoverFile,
                    MonthlyAverageRate = monthlyVotes.Count > 0 ? Math.Round(monthlyVotes.Average(v => v.Rate), 2) : 0,
                    MonthlyTotalVotes  = monthlyVotes.Count,
                    MonthlyPeriodStart = monthlyPeriodStart,
                    MonthlyPeriodEnd   = monthlyPeriodEnd,
                };
            })
            .Where(r => r.MonthlyAverageRate >= minRate && r.MonthlyAverageRate <= maxRate)
            .OrderByDescending(r => r.MonthlyAverageRate)
            .ThenByDescending(r => r.MonthlyTotalVotes)
            .ToList();
    }
}