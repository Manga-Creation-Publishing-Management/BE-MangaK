namespace Manga.Service.Leaderboard;

public interface IService
{
    Task<IEnumerable<Response.LeaderboardResponse>> GetWeeklyLeaderboardAsync();
    Task<IEnumerable<Response.LeaderboardResponse>> GetMonthlyLeaderboardAsync();
}