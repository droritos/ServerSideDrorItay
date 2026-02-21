using Microsoft.AspNetCore.Mvc;
using ServerOfGame.Server.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        // 1. A static list to keep track of everyone currently chatting

        // We use 'static' so all players share the same list.
        public static readonly ConcurrentDictionary<WebSocket, PlayerSession> _connectedClients = new ConcurrentDictionary<WebSocket, PlayerSession>();

        [Route("/ws")] // The address will be ws://localhost:port/ws
        public async Task Get()
        {   
            // Check: Is this a WebSocket request? (Or just a normal HTTP GET?)
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                // Validate Token
                var token = HttpContext.Request.Query["access_token"];
                if(string.IsNullOrEmpty(token))
                {
                    HttpContext.Response.StatusCode = 401;
                    return;
                }

                // Accept the call!
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                string displayName = "UnknownUser";

                var allUsers = LoadUsers();
                var foundUser = allUsers.FirstOrDefault(u => u.Id == token);

                if (foundUser != null)
                {
                    displayName = foundUser.Username;
                }

                var session = new PlayerSession
                {
                    Username = displayName,
                    CurrentRoom = "Lobby", // Everyone starts in the Lobby [cite: 10]
                    MySocket = webSocket
                };

                // Add them to our "Phone Book"
                _connectedClients.TryAdd(webSocket, session);
                Console.WriteLine($"{displayName} connected! Total: " + _connectedClients.Count);
                await BroadcastPlayerList();


                NetworkMessage msg = new NetworkMessage
                {
                    Type = "Chat",
                    Data = $"{displayName}! Welcome to the Chat!"
                };

                string json = JsonSerializer.Serialize(msg);

                var welcomeMsg = Encoding.UTF8.GetBytes(json);

                await webSocket.SendAsync(new ArraySegment<byte>(welcomeMsg), WebSocketMessageType.Text, true, CancellationToken.None);

                await ListenForMessages(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400; // Bad Request
            }
        }

        // We will fill this in next!
        private async Task ListenForMessages(WebSocket socket)
        {
            // Just a placeholder to keep the connection open for now
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);


                if(result.MessageType == WebSocketMessageType.Text)
                {
                    string rawJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var incomingMsg = JsonSerializer.Deserialize<NetworkMessage>(rawJson);

                    // 3. WHO sent this? Get their session from our dictionary
                    if (_connectedClients.TryGetValue(socket, out var session))
                    {
                        if (incomingMsg.Type == "Chat")
                        {
                            await BroadcastToRoom(session.CurrentRoom, $"{session.Username}: {incomingMsg.Data}");
                        }
                        else if (incomingMsg.Type == "JoinRoom")
                        {
                            // Update their room in their "Passport"
                            session.CurrentRoom = incomingMsg.Data; // "Room1", "Room2", etc.
                            Console.WriteLine($"{session.Username} moved to {session.CurrentRoom}");

                            // Optional: Tell everyone in the lobby the list changed
                            await BroadcastPlayerList();
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // Update this line to expect a PlayerSession object instead of a string
                    _connectedClients.TryRemove(socket, out PlayerSession session);

                    string username = session?.Username ?? "Unknown";
                    Console.WriteLine($"{username} disconnected.");

                    await BroadcastPlayerList();
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                }
            }
        }

        private async Task BroadcastToRoom(string roomName, string messageContent)
        {
            NetworkMessage message = new NetworkMessage() { Type = "Chat", Data = messageContent };
            string json = JsonSerializer.Serialize(message);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

            foreach (PlayerSession client in _connectedClients.Values)
            {
                // * ONLY send if they are in the correct room!
                if (client.CurrentRoom == roomName && client.MySocket.State == WebSocketState.Open)
                {
                    await client.MySocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }

        }

        private async Task BroadcastMessage(byte[] buffer, int count, WebSocket senderSocket)
        {
            // Copy the message correctly
            var messageSegment = new ArraySegment<byte>(buffer, 0, count);

            // Loop through all connected clients
            foreach (var client in _connectedClients.ToList()) // ToList() prevents crashes if someone leaves during the loop
            {
                // Don't send the message back to the person who sent it!
                // (Requirement: "broadcasts to all other connected clients (not back to the sender)")
                if (client.Key != senderSocket && client.Key.State == WebSocketState.Open)
                {
                    await client.Key.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        private async Task BroadcastPlayerList()
        {
            // 1. Get the names and turn them into text
            List<string> usernames = _connectedClients.Values
                                      .Select(s => s.Username)
                                      .Where(name => !string.IsNullOrWhiteSpace(name))
                                      .Distinct()
                                      .ToList();

            string dataJson = JsonSerializer.Serialize(usernames);

            // 2. Put it in the Envelope (Fill in the blanks!)
            NetworkMessage msg = new NetworkMessage();
            msg.Type = "PlayerList";
            msg.Data = dataJson;

            // 3. Turn the envelope into text
            string finalJson = JsonSerializer.Serialize(msg);

            // 4. Translate the text into Bytes (Fill in the blank!)
            byte[] messageBytes = Encoding.UTF8.GetBytes(finalJson);
            var messageSegment = new ArraySegment<byte>(messageBytes);

            // 5. Send to EVERYONE (I removed the 'if' check so everyone gets the update!)
            foreach (var client in _connectedClients.Values)
            {
                if (client.MySocket.State == WebSocketState.Open)
                {
                    await client.MySocket.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private List<User> LoadUsers()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
                catch { return new List<User>(); }
            }
            return new List<User>();
        }
    }
}