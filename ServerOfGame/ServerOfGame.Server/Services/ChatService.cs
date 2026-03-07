using ServerOfGame.Server.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Services
{
    public class ChatService
    {
        private static readonly ChatService _instance = new();
        public  static ChatService Instance => _instance;
        private ChatService() { }

        /// <summary>
        /// Broadcasts a chat message to everyone in the SAME room as the sender (excluding sender).
        /// </summary>
        public async Task HandleChat(
            PlayerSession sender,
            string content,
            ConcurrentDictionary<WebSocket, PlayerSession> allClients)
        {
            var msg   = new NetworkMessage { Type = "Chat", Data = $"{sender.Username}: {content}" };
            byte[] buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

            foreach (var p in allClients.Values)
            {
                if (p != sender
                    && p.CurrentRoom == sender.CurrentRoom
                    && p.MySocket?.State == WebSocketState.Open)
                {
                    await p.MySocket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
