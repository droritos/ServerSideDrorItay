using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ServerOfGame.Server.Models;

namespace ServerOfGame.Server.Services
{
    public class GameService
    {
        private static readonly GameService _instance = new GameService();
        public static GameService Instance => _instance;
        private GameService() { }

        // Tracks: RoomID -> Number of Ready Players
        private readonly ConcurrentDictionary<string, int> _readyCounts = new ConcurrentDictionary<string, int>();

        public async Task HandleReadySignal(PlayerSession session, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            if (string.IsNullOrEmpty(session.CurrentRoom) || session.CurrentRoom == "Lobby")
            {
                Console.WriteLine($"[Game] {session.Username} tried to ready in Lobby. Ignored.");
                return;
            }

            _readyCounts.AddOrUpdate(session.CurrentRoom, 1, (roomKey, oldValue) => oldValue + 1);
            int count = _readyCounts[session.CurrentRoom];

            Console.WriteLine($"[Game] {session.Username} ready in {session.CurrentRoom}. Count: {count}");

            if (count >= 2)
            {
                await StartGame(session.CurrentRoom, allClients);
                // Clean up the counter for this room
                _readyCounts.TryRemove(session.CurrentRoom, out _);
            }
        }

        private async Task StartGame(string roomId, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            var msg = new NetworkMessage { Type = "GameStart", Data = "Match Begins!" };
            string json = JsonSerializer.Serialize(msg);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            // Send to everyone in THIS room
            foreach (var client in allClients.Values)
            {
                if (client.CurrentRoom == roomId && client.MySocket.State == WebSocketState.Open)
                {
                    await client.MySocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            Console.WriteLine($"[Game] Match started in room: {roomId}");
        }

        public async Task HandleScoreUpdate(PlayerSession sender, string scoreData, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            // Store it on the server session so the player can't fake it at the end
            if (int.TryParse(scoreData, out int newScore))
            {
                sender.CurrentScore = newScore;
            }

            var msg = new NetworkMessage { Type = "OpponentScore", Data = $"{sender.Username}:{scoreData}" };
            await BroadcastToOpponent(sender, msg, allClients);
        }

        public async Task HandleMatchEnd(PlayerSession sender, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            string matchRoomId = sender.CurrentRoom; 
            Console.WriteLine($"[Game] Match ended in room {matchRoomId}. Resetting players to Lobby.");

            var msg = new NetworkMessage { Type = "MatchEnd", Data = sender.CurrentScore.ToString() };

            await BroadcastToRoom(matchRoomId, msg, allClients);

            foreach (var client in allClients.Values)
            {
                if (client.CurrentRoom == matchRoomId)
                {
                    client.CurrentRoom = "Lobby";
                    client.CurrentScore = 0; // Reset score for next time
                }
            }

            await LobbyService.Instance.BroadcastPlayerList(allClients);
        }
        private async Task BroadcastToRoom(string roomId, NetworkMessage msg, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            string json = JsonSerializer.Serialize(msg);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var client in allClients.Values)
            {
                // Send to EVERYONE in the room, including the sender
                if (client.CurrentRoom == roomId && client.MySocket.State == WebSocketState.Open)
                {
                    await client.MySocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private async Task BroadcastToOpponent(PlayerSession sender, NetworkMessage msg, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            string json = JsonSerializer.Serialize(msg);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            foreach (var client in allClients.Values)
            {
                // Send ONLY to the person in the same room who IS NOT the sender
                if (client.CurrentRoom == sender.CurrentRoom && client != sender && client.MySocket.State == WebSocketState.Open)
                {
                    await client.MySocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}