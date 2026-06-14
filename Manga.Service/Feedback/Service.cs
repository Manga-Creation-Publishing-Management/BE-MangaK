using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.Feedback;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> SendFeedback(Request.SendFeedbackRequest request)
    {
        var userId = GetUserIdCurrent();
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) throw new UnauthorizedAccessException("Unauthorized");
        
        if(user.Role == UserRole.Assistant) throw new UnauthorizedAccessException("Assistant can't send feedback");
        
        var checkSeries = await _dbContext.Series.AnyAsync(x => x.Id == request.SeriesId && x.IsDeleted == false);
        if (!checkSeries) throw new KeyNotFoundException("Series not found or deleted");

        var feedback = new Repository.Entity.Feedback()
        {
            Id = Guid.NewGuid(),
            SenderId = user.Id,
            ReceiverId = request.ReceiverId,
            Content = request.Content,
            SeriesId = request.SeriesId,
            ChapterId = request.ChapterId,
            MangaTaskId = request.MangaId,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        _dbContext.Feedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Response.GetFeedBackResponse> GetFeedBack(Request.GetFeedBackRequest request)
    {
        var userId = GetUserIdCurrent();
        var  user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) throw new UnauthorizedAccessException("Unauthorized");
        throw new NotImplementedException("Not implemented yet");
    }

    private Guid GetUserIdCurrent()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "userId" || x.Type == "UserId")?.Value;
            
        if (string.IsNullOrEmpty(userId)) 
            throw new UnauthorizedAccessException("You must log in");

        return Guid.Parse(userId);
    }
}