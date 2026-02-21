using System.Text.Json.Serialization;

namespace ServerOfGame.Server.Models
{
    public class PlayerSession
    {
        public string Username { get; set; }
        public int Level { get; set; }
        public int ExperiencePoints { get; set; }
        public string CurrentRoom { get; set; } // e.g., "Lobby", "Room1", "Room2"

        [JsonIgnore] // We don't want to send the actual socket over the internet!
        public System.Net.WebSockets.WebSocket MySocket { get; set; }
    }
}