using ServerOfGame.Server.Controllers;
using ServerOfGame.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerOfGame.Server.Services
{
    public class MatchmakingService
    {
        public event Action<PlayerSession, PlayerSession, string> OnMatchCreated;

        // 1. The Singleton Instance
        private static readonly MatchmakingService _instance = new MatchmakingService();
        public static MatchmakingService Instance => _instance;

        // 2. The Private Queue
        private readonly List<PlayerSession> _queue = new List<PlayerSession>();

        private MatchmakingService()
        {
            this.OnMatchCreated += HandleMatchCreated;
        }

        // 3. The Public Entry Point
        public void AddToQueue(PlayerSession session)
        {
            if (!_queue.Contains(session))
            {
                _queue.Add(session);
                Console.WriteLine($"[Matchmaking] {session.Username} joined. Queue size: {_queue.Count}");

                CheckForMatch();
            }
        }
        private async void HandleMatchCreated(PlayerSession p1, PlayerSession p2, string roomId)
        {
            // Call the logic from the LobbyService instead!
            await LobbyService.Instance.SendMatchNotification(p1, p2, roomId);
            await LobbyService.Instance.SendMatchNotification(p2, p1, roomId);

            await LobbyService.Instance.BroadcastPlayerList(WebSocketController._connectedClients);
        }
        private void CheckForMatch()
        {
            if(_queue.Count >= 2)
            {
                PlayerSession player1 = _queue[0];
                PlayerSession player2 = _queue[1];

                _queue.RemoveRange(0, 2);
                // List<PlayerSession> playersToMatch = new List<PlayerSession>();

                string gameRoomId = $"Game_{Guid.NewGuid().ToString().Substring(0, 8)}";

                player1.CurrentRoom = gameRoomId;
                player2.CurrentRoom = gameRoomId;

                OnMatchCreated?.Invoke(player1, player2, gameRoomId);
                Console.WriteLine($"[Matchmaking] Match created: {player1.Username} vs {player2.Username} in {gameRoomId}");
            }
        }
    }
}