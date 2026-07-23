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
            var userID = GetUserCurrentId();
            var user = await _dbContext.Readers.AnyAsync(x => x.Id == userID);
            if (user == null) throw new UnauthorizedAccessException("You must log in");
            var chapterExists =
                await _dbContext.Chapters.AnyAsync(x => x.Id == request.ChapterId && x.Status == ChapterStatus.Publishing);
            if (!chapterExists) throw new KeyNotFoundException("Chapter not found or this is an unpublished chapter");

            if (request.Rate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request.Rate));
            }

            var voting = await _dbContext.ChapterVotings
                .FirstOrDefaultAsync(x => x.ChapterId == request.ChapterId && x.ReaderId == userID);

            if (voting == null)
            {
                voting = new Repository.Entity.ChapterVoting()
                {
                    Id = Guid.NewGuid(),
                    Rate = request.Rate,
                    VoteAt = DateTimeOffset.UtcNow,
                    ChapterId = request.ChapterId,
                    ReaderId = userID,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _dbContext.ChapterVotings.Add(voting);
            }
            else
            {
                if (voting.UpdatedAt == null)
                {
                    voting.Rate = request.Rate;
                    voting.VoteAt = DateTimeOffset.UtcNow;
                    voting.UpdatedAt = DateTimeOffset.UtcNow;
                }
                else
                {
                    throw new InvalidDataException("You only change vote once");
                }
                
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

        public async Task<Response.GetReaderVoteResponse> GetReaderVote( Guid chapterId)
        {
            var user = _httpContextAccessor.HttpContext!.User.Claims
                .FirstOrDefault(u => u.Type == "userId" || u.Type == "UserId")?.Value;
            
            if(String.IsNullOrEmpty(user))
                throw new UnauthorizedAccessException("User not login");
            
            var userIdGuid = Guid.Parse(user!);
            
            var reader = await _dbContext.Readers.FirstOrDefaultAsync(u => u.Id == userIdGuid);
            
            if (reader == null)
                throw new KeyNotFoundException("User not found");
            
            var vote = await _dbContext.ChapterVotings
                .FirstOrDefaultAsync(x =>
                    x.ReaderId == userIdGuid &&
                    x.ChapterId == chapterId &&
                    !x.IsDeleted);

            if (vote == null)
                throw new KeyNotFoundException("Vote not found.");

            return new Response.GetReaderVoteResponse
            {
                ReaderId = vote.ReaderId,
                ChapterId = vote.ChapterId,
                Rating = vote.Rate
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
