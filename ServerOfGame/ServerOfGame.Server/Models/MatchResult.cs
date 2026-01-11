namespace ServerOfGame.Server.Models
{
    public struct MatchResult
    {
        public int score { get; set; }
        public float durationSeconds { get; set; }
        public string levelName { get; set; }
    }
}