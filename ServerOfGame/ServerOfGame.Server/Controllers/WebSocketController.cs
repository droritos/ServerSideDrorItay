using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace ServerOfGame.Server.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        // 1. A static list to keep track of everyone currently chatting

        // We use 'static' so all players share the same list.
        private static List<WebSocket> _connectedClients = new List<WebSocket>();

        [Route("/ws")] // The address will be ws://localhost:port/ws
        public async Task Get()
        {
            // Check: Is this a WebSocket request? (Or just a normal HTTP GET?)
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                // Accept the call!
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                // Add them to our "Phone Book"
                _connectedClients.Add(webSocket);
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
                    // 3. BROADCAST: Send this message to everyone else!
                    await BroadcastMessage(buffer, result.Count, socket);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // 4. Handle Disconnect
                    _connectedClients.Remove(socket);
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
                if (client != senderSocket && client.State == WebSocketState.Open)
                {
                    await client.SendAsync(messageSegment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

       
    }
}