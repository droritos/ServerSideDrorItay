using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using ServerOfGame.Server.Services;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        // Shared dictionary: all connected sockets -> their session
        public static readonly ConcurrentDictionary<WebSocket, PlayerSession> _connectedClients = new();

        private readonly UserService        _userService;
        private readonly LobbyService       _lobbyService;
        private readonly ChatService        _chatService;
        private readonly GameService        _gameService;
        private readonly MatchmakingService _matchmaking;

        public WebSocketController(
            UserService        userService,
            LobbyService       lobbyService,
            ChatService        chatService,
            GameService        gameService,
            MatchmakingService matchmaking)
        {
            _userService  = userService;
            _lobbyService = lobbyService;
            _chatService  = chatService;
            _gameService  = gameService;
            _matchmaking  = matchmaking;
        }

        [Route("/ws")]
        public async Task Connect()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            // ── JWT validation ──────────────────────────────────
            string? token = HttpContext.Request.Query["access_token"];
            if (string.IsNullOrEmpty(token))
            {
                HttpContext.Response.StatusCode = 401;
                return;
            }

            // Parse claims from validated token (middleware already ran, use User)
            string userId   = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            string username = User.FindFirstValue(ClaimTypes.Name)           ?? "Unknown";

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

            await _lobbyService.BroadcastPlayerList(_connectedClients);
            await Send(socket, new NetworkMessage { Type = "Chat", Data = $"Welcome, {username}!" });

            await ListenLoop(socket, session);
        }

        // ── Message loop ───────────────────────────────────────

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
                            await _chatService.HandleChat(session, msg.Data, _connectedClients);
                            break;

                        case "JoinRoom":
                            await _lobbyService.SwitchRoom(session, msg.Data, _connectedClients);
                            break;

                        case "FindMatch":
                            _matchmaking.AddToQueue(session);
                            break;

                        case "Ready":
                            await _gameService.HandleReadySignal(session, _connectedClients);
                            break;

                        case "UpdateScore":
                            await _gameService.HandleScoreUpdate(session, msg.Data, _connectedClients);
                            break;

                        case "MatchEnd":
                            await _gameService.HandleMatchEnd(session, _connectedClients, _userService);
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

        // ── Cleanup on disconnect ──────────────────────────────

        private async Task Cleanup(WebSocket socket)
        {
            if (!_connectedClients.TryRemove(socket, out var session)) return;

            Console.WriteLine($"[WS] {session.Username} disconnected from {session.CurrentRoom}");

            _matchmaking.RemoveFromQueue(session);

            if (session.CurrentRoom != "Lobby")
                await _gameService.HandleMatchEnd(session, _connectedClients, _userService);

            await _lobbyService.BroadcastPlayerList(_connectedClients);

            try
            {
                if (socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
            }
            catch { /* already closed */ }
        }

        // ── Helpers ───────────────────────────────────────────

        private static async Task Send(WebSocket socket, NetworkMessage msg)
        {
            if (socket.State != WebSocketState.Open) return;
            byte[] buf = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await socket.SendAsync(buf, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
