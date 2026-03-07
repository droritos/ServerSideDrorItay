using ServerOfGame.Server.Controllers;
using ServerOfGame.Server.Models;

namespace ServerOfGame.Server.Services
{
    public class MatchmakingService
    {
        private static readonly MatchmakingService _instance = new();
        public  static MatchmakingService Instance => _instance;

        private readonly List<PlayerSession> _queue = new();
        private readonly object _lock = new();

        private MatchmakingService() { }

        public void AddToQueue(PlayerSession session)
        {
            lock (_lock)
            {
                if (_queue.Contains(session)) return;
                _queue.Add(session);
                Console.WriteLine($"[Matchmaking] {session.Username} joined queue. Size: {_queue.Count}");
                CheckForMatch();
            }
        }

        public void RemoveFromQueue(PlayerSession session)
        {
            lock (_lock)
            {
                if (_queue.Remove(session))
                    Console.WriteLine($"[Matchmaking] {session.Username} removed from queue.");
            }
        }

        private void CheckForMatch()
        {
            if (_queue.Count < 2) return;

            var p1 = _queue[0];
            var p2 = _queue[1];
            _queue.RemoveRange(0, 2);

            string roomId = $"Game_{Guid.NewGuid().ToString()[..8]}";
            p1.CurrentRoom = roomId;
            p2.CurrentRoom = roomId;

            Console.WriteLine($"[Matchmaking] {p1.Username} vs {p2.Username} in {roomId}");

            _ = NotifyMatch(p1, p2, roomId);
        }

        private async Task NotifyMatch(PlayerSession p1, PlayerSession p2, string roomId)
        {
            await LobbyService.Instance.SendMatchNotification(p1, p2.Username, roomId);
            await LobbyService.Instance.SendMatchNotification(p2, p1.Username, roomId);
            await LobbyService.Instance.BroadcastPlayerList(WebSocketController._connectedClients);
        }
    }
}
