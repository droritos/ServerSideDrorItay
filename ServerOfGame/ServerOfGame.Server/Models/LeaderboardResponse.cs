namespace ServerOfGame.Server.Models
{
    public struct LeaderboardResponse
    {
        // Unity needs the list to be inside a variable, not the root
        public List<MatchResult> list {  get; set; }
    }
}