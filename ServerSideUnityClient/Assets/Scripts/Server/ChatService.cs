using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Server
{
    public class ChatService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] ChatGUIHandler chatGUIHandler;
        
        private ClientWebSocket _currentWebSocket;
        private const string URL = "ws://localhost:5235/ws";

        #region  << Unity Functions >>
        private void Start()
        {
            Connect();

            chatGUIHandler.OnMessageReceived += OnChatSubmitted;
        }

        private void OnDestroy()
        {
            chatGUIHandler.OnMessageReceived -= OnChatSubmitted;
        }

        private void OnValidate()
        {
            if(!chatGUIHandler)
                chatGUIHandler = FindFirstObjectByType<ChatGUIHandler>();
        }
        #endregion

        public async void Connect()
        {
            _currentWebSocket = new ClientWebSocket();

            // 2. Create the URI
            Uri serverUri = new Uri(URL);

            Debug.Log($"Attempting to connect to {URL}...");

            try
            {
                // 3. NOW we connect
                await _currentWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                Debug.Log("Connected to Server!");
            
            
                // 4. (Future Step) Start listening for messages...
                ReceiveMessages(); 
                
                await SendMessage("Hello World! TEST");
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection Error: {e.Message}");
            }
        }
        
        private async void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            
            while (_currentWebSocket.State == WebSocketState.Open)
            {
                // Wait to get data
                var result = await _currentWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                // Safe code against server disconnection
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _currentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None);
                    Debug.Log("Server closed the connection.");
                    break; 
                }
                
                // Log the String Data!
                string stringText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //Debug.Log($"Received: {stringText}");
                chatGUIHandler.AddMessageToChat(stringText);
            }
        }
        
        private async void OnChatSubmitted(string message)
        {
            // 1. Send to Server (Networking)
            await SendMessage(message);

            // 2. Show on my own screen (UI)
            chatGUIHandler.AddMessageToChat($"You: {message}");

            // 3. Clear the text box (UI)
            //chatGUIHandler.ClearInput();
        }

        private async Task SendMessage(string message)
        {
            if (_currentWebSocket.State != WebSocketState.Open)
            {
                Debug.LogWarning("Cannot send message: Not connected!");
                return;
            }
            
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Send ONCE 
            await _currentWebSocket.SendAsync
            (
                new ArraySegment<byte>(messageBytes), // Wrap in ArraySegment
                WebSocketMessageType.Text,
                true, // End of message
                CancellationToken.None
            );
            
            Debug.Log($"Sent: {message}");
        }
    }
}
