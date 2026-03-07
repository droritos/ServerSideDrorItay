using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ServerOfGame.Server.Models;
using ServerOfGame.Server.Services;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        public static readonly ConcurrentDictionary<WebSocket, PlayerSession> _connectedClients = new();

        private readonly UserService    _userService;
        private readonly IConfiguration _config;

        public WebSocketController(UserService userService, IConfiguration config)
        {
            _userService = userService;
            _config      = config;
        }

        [Route("/ws")]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            string? token = HttpContext.Request.Query["access_token"];
            if (string.IsNullOrEmpty(token))
            {
                HttpContext.Response.StatusCode = 401;
                return;
            }

            // Manually validate JWT (endpoint has no [Authorize] so we do it ourselves)
            ClaimsPrincipal? principal = ValidateToken(token);
            if (principal == null)
            {
                HttpContext.Response.StatusCode = 401;
                return;
            }

            string userId   = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            string username = principal.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            var user = _userService.GetById(userId);
            if (user == null || user.IsBanned)
            {
                HttpContext.Response.StatusCode = 403;
                return;
            }

            var socket  = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var session = new PlayerSession
            {
                UserId      = userId,
                Username    = username,
                CurrentRoom = "Lobby",
                MySocket    = socket
            };

            _connectedClients.TryAdd(socket, session);
            Console.WriteLine($"[WS] {username} connected. Total: {_connectedClients.Count}");

            await LobbyService.Instance.BroadcastPlayerList(_connectedClients);
            await Send(socket, new NetworkMessage { Type = "Chat", Data = $"Welcome, {username}!" });

            await ListenLoop(socket, session);
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var handler = new JwtSecurityTokenHandler();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = _config["Jwt:Issuer"],
                    ValidAudience            = _config["Jwt:Audience"],
                    IssuerSigningKey         = key
                };
                return handler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }

        private async Task ListenLoop(WebSocket socket, PlayerSession session)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await Cleanup(socket);
                        break;
                    }

                    if (result.MessageType != WebSocketMessageType.Text) continue;

                    string raw = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    NetworkMessage? msg;
                    try { msg = JsonSerializer.Deserialize<NetworkMessage>(raw); }
                    catch { continue; }
                    if (msg == null) continue;

                    switch (msg.Type)
                    {
                        case "Chat":
                            await ChatService.Instance.HandleChat(session, msg.Data, _connectedClients);
                            break;
                        case "JoinRoom":
                            await LobbyService.Instance.SwitchRoom(session, msg.Data, _connectedClients);
                            break;
                        case "FindMatch":
                            MatchmakingService.Instance.AddToQueue(session);
                            break;
                        case "Ready":
                            await GameService.Instance.HandleReadySignal(session, _connectedClients);
                            break;
                        case "UpdateScore":
                            await GameService.Instance.HandleScoreUpdate(session, msg.Data, _connectedClients);
                            break;
                        case "MatchEnd":
                            await GameService.Instance.HandleMatchEnd(session, _connectedClients, _userService);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WS] Exception for {session.Username}: {ex.Message}");
                await Cleanup(socket);
            }
        }

        private async Task Cleanup(WebSocket socket)
        {
            if (!_connectedClients.TryRemove(socket, out var session)) return;

            Console.WriteLine($"[WS] {session.Username} disconnected from {session.CurrentRoom}");

            MatchmakingService.Instance.RemoveFromQueue(session);

            if (session.CurrentRoom != "Lobby")
                await GameService.Instance.HandleMatchEnd(session, _connectedClients, _userService);

            await LobbyService.Instance.BroadcastPlayerList(_connectedClients);

            try
            {
                if (socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
            catch { }
        }

        private static async Task Send(WebSocket socket, NetworkMessage msg)
        {
            if (socket.State != WebSocketState.Open) return;
            byte[] buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await socket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
