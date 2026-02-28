using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using ServerOfGame.Server.Services;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        public static readonly ConcurrentDictionary<WebSocket, PlayerSession> _connectedClients = new ConcurrentDictionary<WebSocket, PlayerSession>();

        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var token = HttpContext.Request.Query["access_token"];
                if (string.IsNullOrEmpty(token))
                {
                    HttpContext.Response.StatusCode = 401;
                    return;
                }

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                string displayName = "UnknownUser";
                var allUsers = LoadUsers();
                var foundUser = allUsers.FirstOrDefault(u => u.Id == token);

                if (foundUser != null) displayName = foundUser.Username;

                var session = new PlayerSession
                {
                    Username = displayName,
                    CurrentRoom = "Lobby",
                    MySocket = webSocket
                };

                _connectedClients.TryAdd(webSocket, session);
                Console.WriteLine($"{displayName} connected! Total: " + _connectedClients.Count);

                await LobbyService.Instance.BroadcastPlayerList(_connectedClients);

                // Welcome Message
                await SendWelcomeMessage(webSocket, displayName);

                await ListenForMessages(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task ListenForMessages(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string rawJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var incomingMsg = JsonSerializer.Deserialize<NetworkMessage>(rawJson);

                        if (_connectedClients.TryGetValue(socket, out var session))
                        {
                            if (incomingMsg.Type == "Chat")
                                await ChatService.Instance.HandleChat(session, incomingMsg.Data, _connectedClients);
                            else if (incomingMsg.Type == "JoinRoom")
                                await LobbyService.Instance.SwitchRoom(session, incomingMsg.Data, _connectedClients);
                            else if (incomingMsg.Type == "FindMatch")
                                MatchmakingService.Instance.AddToQueue(session);
                            else if (incomingMsg.Type == "Ready")
                                await GameService.Instance.HandleReadySignal(session, _connectedClients);
                            else if (incomingMsg.Type == "UpdateScore")
                                await GameService.Instance.HandleScoreUpdate(session, incomingMsg.Data, _connectedClients);
                            else if (incomingMsg.Type == "MatchEnd")
                                await GameService.Instance.HandleMatchEnd(session, _connectedClients);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleCleanup(socket); // Use a central cleanup method
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Socket exception: {ex.Message}");
                await HandleCleanup(socket);
            }
        }
        private async Task SendWelcomeMessage(WebSocket socket, string name)
        {
            var msg = new NetworkMessage { Type = "Chat", Data = $"{name}! Welcome to the Chat!" };
            var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private async Task HandleCleanup(WebSocket socket)
        {
            if (_connectedClients.TryRemove(socket, out PlayerSession session))
            {
                string username = session?.Username ?? "Unknown";
                string room = session?.CurrentRoom ?? "Lobby";
                Console.WriteLine($"{username} removed from {room}.");

                // Requirement: Handle "client disconnected - ends in who won"
                if (room != "Lobby")
                {
                    // Tell GameService to handle the forfeit/win logic
                    await GameService.Instance.HandleMatchEnd(session, _connectedClients);
                }

                // Refresh the lobby for everyone else
                await LobbyService.Instance.BroadcastPlayerList(_connectedClients);
            }

            if (socket.State != WebSocketState.Aborted && socket.State != WebSocketState.Closed)
            {
                try 
                { 
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None); 
                } 
                catch { }
            }
        }
        private List<User> LoadUsers() // Need to be SQL or anther DB
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
            if (!System.IO.File.Exists(filePath)) return new List<User>();
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch { return new List<User>(); }
        }
    }
}