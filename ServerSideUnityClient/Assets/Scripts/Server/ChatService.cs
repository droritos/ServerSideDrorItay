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
            //TryConnectAgain();

            chatGUIHandler.OnMessageReceived += OnChatSubmitted;
            chatGUIHandler.OnDisconnected += Disconnect;
            //chatGUIHandler.OnConnect += TryConnectAgain;
        }

      

        private void OnDestroy()
        {
            chatGUIHandler.OnMessageReceived -= OnChatSubmitted;
            chatGUIHandler.OnDisconnected -= Disconnect;
            //chatGUIHandler.OnConnect -= TryConnectAgain;
        }

        private void OnValidate()
        {
            if(!chatGUIHandler)
                chatGUIHandler = FindFirstObjectByType<ChatGUIHandler>();
        }
        #endregion

        public async void Connect(string token)
        {
            _currentWebSocket = new ClientWebSocket();

          
            var finalUrl = $"{URL}?access_token={token}";
            Uri serverUri = new Uri(finalUrl);

            Debug.Log($"Attempting to connect to {URL}...");

            try
            {
                
                await _currentWebSocket.ConnectAsync(serverUri, CancellationToken.None);
                Debug.Log("Connected to Server!");
            
            
                
                ReceiveMessages(); 
                
               
                chatGUIHandler.ChangePanels(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection Error: {e.Message}");
            }
        }
        public async void Disconnect()
        {
            if (_currentWebSocket != null && _currentWebSocket.State == WebSocketState.Open)
            {
                Debug.Log("Disconnecting...");
        
                
                await _currentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User Quit", CancellationToken.None);
        
               
                _currentWebSocket = null;
        
                Debug.Log("Disconnected.");
                chatGUIHandler.ChangePanels(false);
                //chatGUIHandler.ClearChat(); 
            }
        }
        private void TryConnectAgain()
        {
            Connect(RandomNameTester());
        }
        private async void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];
            
            while (_currentWebSocket.State == WebSocketState.Open)
            {
                
                var result = await _currentWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _currentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closed", CancellationToken.None);
                    chatGUIHandler.ChangePanels(false);
                    Debug.Log("Server closed the connection.");
                    break; 
                }
                
                
                string stringText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //Debug.Log($"Received: {stringText}");
                chatGUIHandler.AddMessageToChat(stringText);
            }
        }
        
        private async void OnChatSubmitted(string message)
        {
          
            await SendChatMessage(message);

          
            chatGUIHandler.AddMessageToChat($"You: {message}");
            

        }

        private async Task SendChatMessage(string message)
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
            
//            Debug.Log($"Sent: {message}");
        }
        
        
        private static string RandomNameTester()
        {
            // Generate a random name like "Player42"
            string fakeName = "Player" + UnityEngine.Random.Range(1, 999);
            return fakeName;
        }
    }
}
