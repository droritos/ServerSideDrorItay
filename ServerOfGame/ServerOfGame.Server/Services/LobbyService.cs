using ServerOfGame.Server.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class LobbyService
{
    private static readonly LobbyService _instance = new LobbyService();
    public static LobbyService Instance => _instance;
    private LobbyService() { }

    public async Task SwitchRoom(PlayerSession session, string newRoomName, ConcurrentDictionary<WebSocket, PlayerSession> allClients)
    {
        session.CurrentRoom = newRoomName; 
        Console.WriteLine($"[Lobby] {session.Username} moved to {newRoomName}");

        // Update everyone in the affected rooms
        await BroadcastPlayerList(allClients); 
    }

    public async Task BroadcastPlayerList(ConcurrentDictionary<WebSocket, PlayerSession> clients)
    {
        var rooms = clients.Values.GroupBy(s => s.CurrentRoom); // Filter

        foreach (var roomGroup in rooms)
        {
            List<string> players = roomGroup.Select(s => s.Username).ToList();
            string dataJson = JsonSerializer.Serialize(players);

            var msg = new NetworkMessage { Type = "PlayerList", Data = dataJson };
            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            var segment = new ArraySegment<byte>(bytes);

            foreach (var player in roomGroup)
            {
                if (player.MySocket.State == WebSocketState.Open)
                    await player.MySocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public async Task SendMatchNotification(PlayerSession player, PlayerSession opponent, string roomId)
    {
        var message = new NetworkMessage { Type = "MatchFound", Data = opponent.Username };
        byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        if (player.MySocket.State == WebSocketState.Open)
        {
            await player.MySocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}