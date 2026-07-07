namespace Manga.Service.Leaderboard;

public class Response
{
    public class LeaderboardResponse
    {
        public int Rank { get; set; }
        public string Series { get; set; }
        public string Author { get; set; }
        public int Votes { get; set; }
        public double AverageRate { get; set; }
        public string Change { get; set; }
    }
}