namespace Manga.Service.Leaderboard;

public interface IService
{
    Task<bool> GenerateWeeklyLeaderboard();

    Task<bool> GenerateMonthlyLeaderboard();

    Task<List<string>> GetAvailablePeriods(string type);

    Task<List<Response.LeaderboardResponse>> GetWeeklyLeaderboard(string? period);

    Task<List<Response.LeaderboardResponse>> GetMonthlyLeaderboard(string? period);
}