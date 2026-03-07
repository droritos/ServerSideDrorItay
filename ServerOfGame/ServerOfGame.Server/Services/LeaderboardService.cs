using ServerOfGame.Server.Models;

namespace ServerOfGame.Server.Services
{
    /// <summary>
    /// Thin facade that produces leaderboard views from UserService data.
    /// </summary>
    public class LeaderboardService
    {
        private readonly UserService _users;
        public LeaderboardService(UserService users) => _users = users;

        public List<LeaderboardEntry> GetTopByWins(int count = 10)
            => _users.GetAll()
                .OrderByDescending(u => u.Wins)
                .Take(count)
                .Select(u => new LeaderboardEntry
                {
                    Username    = u.Username,
                    Wins        = u.Wins,
                    GamesPlayed = u.GamesPlayed
                })
                .ToList();
    }

    public class LeaderboardEntry
    {
        public string Username    { get; set; } = string.Empty;
        public int    Wins        { get; set; }
        public int    GamesPlayed { get; set; }
    }
}
