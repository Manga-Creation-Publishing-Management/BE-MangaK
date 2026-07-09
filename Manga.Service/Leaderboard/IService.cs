namespace Manga.Service.Leaderboard;

public interface IService
{
    Task<IEnumerable<Response.LeaderboardResponse>> GetWeeklyLeaderboard();
    Task<IEnumerable<Response.LeaderboardResponse>> GetMonthlyLeaderboard();
}