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
        public static readonly ConcurrentDictionary<WebSocket, string> _connectedClients = new ConcurrentDictionary<WebSocket, string>();

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

                // Add them to our "Phone Book"
                _connectedClients.TryAdd(webSocket, displayName);
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
                    string name = _connectedClients[socket] + ": ";

                    string finalString = name + Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var newBuffer = Encoding.UTF8.GetBytes(finalString);
                    // 3. BROADCAST: Send this message to everyone else!

                    await BroadcastMessage(newBuffer, newBuffer.Length, socket);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // 4. Handle Disconnect
                    _connectedClients.TryRemove(socket, value: out string username);
                    string exitMsg = "Server: " + username + " disconnected.";
                    await BroadcastPlayerList();

                    var newBuffer = Encoding.UTF8.GetBytes(exitMsg);
                    await BroadcastMessage(newBuffer, newBuffer.Length, socket);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
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
            var usernames = _connectedClients.Values.ToList();
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
            foreach (var client in _connectedClients.ToList())
            {
                if (client.Key.State == WebSocketState.Open)
                {
                    await client.Key.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
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