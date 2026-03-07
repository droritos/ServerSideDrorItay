using ServerOfGame.Server.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Services
{
    public class LobbyService
    {
        private static readonly LobbyService _instance = new();
        public  static LobbyService Instance => _instance;
        private LobbyService() { }

        public async Task SwitchRoom(
            PlayerSession session,
            string newRoom,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            session.CurrentRoom = newRoom;
            Console.WriteLine($"[Lobby] {session.Username} moved to {newRoom}");
            await BroadcastPlayerList(allClients);
        }

        public async Task BroadcastPlayerList(
            ConcurrentDictionary<WebSocket, PlayerSession> clients)
        {
            var rooms = clients.Values.GroupBy(s => s.CurrentRoom);

            foreach (var room in rooms)
            {
                var players = room.Select(s => s.Username).ToList();
                string data = JsonSerializer.Serialize(players);
                var msg  = new NetworkMessage { Type = "PlayerList", Data = data };
                byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

                foreach (var player in room)
                {
                    if (player.MySocket?.State == WebSocketState.Open)
                        await player.MySocket.SendAsync(
                            bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task SendMatchNotification(
            PlayerSession player,
            string opponentName,
            string roomId)
        {
            var msg  = new NetworkMessage { Type = "MatchFound", Data = opponentName };
            byte[] buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            if (player.MySocket?.State == WebSocketState.Open)
                await player.MySocket.SendAsync(
                    buf, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
