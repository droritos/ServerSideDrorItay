using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ServerOfGame.Server.Models;

namespace ServerOfGame.Server.Services
{
    /// <summary>
    /// Manages in-progress matches:
    ///  - Ready-up handshake before start
    ///  - Score updates with anti-cheat validation
    ///  - Match-end: determines winner, records W/L, broadcasts result
    /// </summary>
    public class GameService
    {
        private static readonly GameService _instance = new();
        public  static GameService Instance => _instance;

        // RoomId -> ready count
        private readonly ConcurrentDictionary<string, int> _readyCounts = new();

        // Anti-cheat: max score delta allowed per update (tune to your game)
        private const int MaxScoreDeltaPerUpdate = 50;

        private GameService() { }

        // ── Ready-up ───────────────────────────────────────────

        public async Task HandleReadySignal(
            PlayerSession session,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            if (string.IsNullOrEmpty(session.CurrentRoom) || session.CurrentRoom == "Lobby")
                return;

            if (session.IsReady) return;          // ignore duplicate ready
            session.IsReady = true;

            int count = _readyCounts.AddOrUpdate(session.CurrentRoom, 1, (_, v) => v + 1);
            Console.WriteLine($"[Game] {session.Username} ready in {session.CurrentRoom}. Count: {count}");

            if (count >= 2)
            {
                _readyCounts.TryRemove(session.CurrentRoom, out _);
                await StartGame(session.CurrentRoom, allClients);
            }
        }

        private async Task StartGame(
            string roomId,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            // Reset scores for fresh start
            foreach (var p in allClients.Values.Where(p => p.CurrentRoom == roomId))
            {
                p.CurrentScore = 0;
                p.IsReady      = false;
            }

            await BroadcastToRoom(roomId,
                new NetworkMessage { Type = "GameStart", Data = "Match Begins!" },
                allClients);

            Console.WriteLine($"[Game] Match started: {roomId}");
        }

        // ── Score update (with anti-cheat) ────────────────────

        public async Task HandleScoreUpdate(
            PlayerSession sender,
            string scoreData,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            if (!int.TryParse(scoreData, out int reported))
            {
                Console.WriteLine($"[AntiCheat] {sender.Username} sent non-numeric score: {scoreData}");
                return;
            }

            // Anti-cheat: score may only go up, and by a capped amount per message
            int delta = reported - sender.CurrentScore;
            if (delta < 0 || delta > MaxScoreDeltaPerUpdate)
            {
                Console.WriteLine($"[AntiCheat] {sender.Username} BLOCKED: score jump {sender.CurrentScore}->{reported}");
                await SendTo(sender,
                    new NetworkMessage { Type = "AntiCheat", Data = "Invalid score update detected." },
                    allClients);
                return;
            }

            sender.CurrentScore = reported;

            await BroadcastToOpponent(sender,
                new NetworkMessage { Type = "OpponentScore", Data = $"{sender.Username}:{reported}" },
                allClients);
        }

        // ── Match end ─────────────────────────────────────────

        public async Task HandleMatchEnd(
            PlayerSession sender,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients,
            UserService? userService = null)
        {
            string roomId = sender.CurrentRoom;
            if (roomId == "Lobby") return;

            Console.WriteLine($"[Game] Match ended in {roomId}");

            var players = allClients.Values
                .Where(p => p.CurrentRoom == roomId)
                .ToList();

            // Determine winner by score
            PlayerSession? winner = players.Count >= 2
                ? players.OrderByDescending(p => p.CurrentScore).First()
                : sender;

            string winnerName = winner?.Username ?? "Unknown";

            // Record W/L in persistent user data
            if (userService != null)
            {
                foreach (var p in players)
                {
                    if (p.Username == winnerName)
                        userService.RecordWin(p.Username);
                    else
                        userService.RecordLoss(p.Username);
                }
            }

            // Broadcast result to the whole room
            await BroadcastToRoom(roomId,
                new NetworkMessage { Type = "MatchResult", Data = winnerName },
                allClients);

            // Move everyone back to Lobby
            foreach (var p in players)
            {
                p.CurrentRoom  = "Lobby";
                p.CurrentScore = 0;
                p.IsReady      = false;
            }

            await LobbyService.Instance.BroadcastPlayerList(allClients);
        }

        // ── Helpers ───────────────────────────────────────────

        private async Task BroadcastToRoom(
            string roomId,
            NetworkMessage msg,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            byte[] bytes = Encode(msg);
            foreach (var p in allClients.Values)
            {
                if (p.CurrentRoom == roomId && p.MySocket?.State == WebSocketState.Open)
                    await p.MySocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task BroadcastToOpponent(
            PlayerSession sender,
            NetworkMessage msg,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            byte[] bytes = Encode(msg);
            foreach (var p in allClients.Values)
            {
                if (p.CurrentRoom == sender.CurrentRoom
                    && p != sender
                    && p.MySocket?.State == WebSocketState.Open)
                {
                    await p.MySocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private async Task SendTo(
            PlayerSession target,
            NetworkMessage msg,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            if (target.MySocket?.State == WebSocketState.Open)
            {
                byte[] bytes = Encode(msg);
                await target.MySocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private static byte[] Encode(NetworkMessage msg)
            => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
    }
}
