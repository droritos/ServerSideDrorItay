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

        /// <summary>
        /// For each room, sends the list of players in that room to every member.
        /// </summary>
        public async Task BroadcastPlayerList(ConcurrentDictionary<WebSocket, PlayerSession> clients)
        {
            var rooms = clients.Values.GroupBy(s => s.CurrentRoom);

            foreach (var roomGroup in rooms)
            {
                var playerNames = roomGroup.Select(s => s.Username).ToList();
                string dataJson = JsonSerializer.Serialize(playerNames);
                var msg    = new NetworkMessage { Type = "PlayerList", Data = dataJson };
                byte[] buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

                foreach (var p in roomGroup)
                {
                    if (p.MySocket?.State == WebSocketState.Open)
                        await p.MySocket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task SendMatchNotification(
            PlayerSession player,
            PlayerSession opponent,
            string roomId)
        {
            var msg = new NetworkMessage { Type = "MatchFound", Data = opponent.Username };
            byte[] buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            if (player.MySocket?.State == WebSocketState.Open)
                await player.MySocket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
