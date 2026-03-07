using System.Text.Json.Serialization;

namespace ServerOfGame.Server.Models
{
    public class PlayerSession
    {
        public string Username        { get; set; } = "Unknown";
        public string UserId          { get; set; } = string.Empty;
        public int    Level           { get; set; }
        public int    ExperiencePoints{ get; set; }
        public int    CurrentScore    { get; set; }
        public string CurrentRoom     { get; set; } = "Lobby";
        public bool   IsReady         { get; set; }

        [JsonIgnore]
        public System.Net.WebSockets.WebSocket? MySocket { get; set; }
    }
}
