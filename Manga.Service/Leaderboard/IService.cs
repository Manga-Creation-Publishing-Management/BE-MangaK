namespace Manga.Service.Leaderboard;

public interface IService
{
    Task GenerateWeeklyLeaderboard();

    Task GenerateMonthlyLeaderboard();
}