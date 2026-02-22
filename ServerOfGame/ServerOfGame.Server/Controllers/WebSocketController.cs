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
                        {
                            // Refactored to ChatService
                            await ChatService.Instance.HandleChat(session, incomingMsg.Data, _connectedClients);
                        }
                        else if (incomingMsg.Type == "JoinRoom")
                        {
                            // Refactored to LobbyService
                            await LobbyService.Instance.SwitchRoom(session, incomingMsg.Data, _connectedClients);
                        }
                        else if (incomingMsg.Type == "FindMatch")
                        {
                            MatchmakingService.Instance.AddToQueue(session);
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _connectedClients.TryRemove(socket, out PlayerSession session);
                    Console.WriteLine($"{session?.Username ?? "Unknown"} disconnected.");

                    await LobbyService.Instance.BroadcastPlayerList(_connectedClients);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
        }

        private async Task SendWelcomeMessage(WebSocket socket, string name)
        {
            var msg = new NetworkMessage { Type = "Chat", Data = $"{name}! Welcome to the Chat!" };
            var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
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