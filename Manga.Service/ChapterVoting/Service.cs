using Manga.Repository.Data;
using Manga.Repository.Entity.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Manga.Service.ChapterVoting;

public class Service : IService
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Service(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Response.VoteChapterResponse> VoteChapter(Request.VoteChapterRequest request)
    {
        var user = await _dbContext.Readers.FirstOrDefaultAsync(x => x.Id == GetUserCurrentId());
        if (user == null) throw new UnauthorizedAccessException("You must log in");
        var chapterExists =
            await _dbContext.Chapters.AnyAsync(x => x.Id == request.ChapterId || x.Status != ChapterStatus.Publishing);
        if (!chapterExists) throw new KeyNotFoundException("Chapter not found or already published");

        if (request.Rate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Rate));
        }

        var voting = await _dbContext.ChapterVotings
            .FirstOrDefaultAsync(x => x.ChapterId == request.ChapterId && x.ReaderId == user.Id);

        if (voting == null)
        {
            voting = new Repository.Entity.ChapterVoting()
            {
                Id = Guid.NewGuid(),
                Rate = request.Rate,
                VoteAt = DateTimeOffset.UtcNow,
                ChapterId = request.ChapterId,
                ReaderId = user.Id,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.ChapterVotings.Add(voting);
        }
        else
        {
            voting.Rate = request.Rate;
            voting.VoteAt = DateTimeOffset.UtcNow;
            voting.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return new Response.VoteChapterResponse()
        {
            Id = voting.Id,
            ChapterId = voting.ChapterId,
            Rate = voting.Rate,
            VoteAt = voting.VoteAt
        };
    }

    private Guid GetUserCurrentId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == "UserId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("You must log in");
        }

        return Guid.Parse(userId);
    }
}