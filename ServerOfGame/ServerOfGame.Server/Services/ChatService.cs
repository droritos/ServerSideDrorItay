using ServerOfGame.Server.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class ChatService
{
    private static readonly ChatService _instance = new ChatService();
    public static ChatService Instance => _instance;
    private ChatService() { }

    public async Task HandleChat(PlayerSession sender, string content, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
    {
        var msg = new NetworkMessage { Type = "Chat", Data = $"{sender.Username}: {content}" }; // [cite: 14]
        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        var segment = new ArraySegment<byte>(bytes);

        foreach (var client in allClients.Values)
        {
            if (client != sender && client.CurrentRoom == sender.CurrentRoom && client.MySocket.State == WebSocketState.Open)
            {
                await client.MySocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}