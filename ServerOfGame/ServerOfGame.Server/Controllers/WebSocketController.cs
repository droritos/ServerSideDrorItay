using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;

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

                // Add them to our "Phone Book"
                _connectedClients.TryAdd(webSocket, token);
                Console.WriteLine("Someone connected! Total: " + _connectedClients.Count);

                var welcomeMsg = Encoding.UTF8.GetBytes("Welcome to the Chat!");
                await webSocket.SendAsync(new ArraySegment<byte>(welcomeMsg), WebSocketMessageType.Text, true, CancellationToken.None);

                // Keep the connection open (The Loop)

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
                    string name = _connectedClients[socket] + ":";

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

       
    }
}