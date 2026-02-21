using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    public class ChatService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] ChatGUIHandler chatGUIHandler;
        [SerializeField] ServicesChannel servicesChannel;
        [SerializeField] GUIChannel guiChannel;
        
        private ClientWebSocket _currentWebSocket;
        private const string URL = "ws://localhost:5235/ws";

        #region  << Unity Functions >>
        private void Start()
        {
            //TryConnectAgain();
            servicesChannel.Subscribe(ServiceEventType.Login,Connect);
            chatGUIHandler.OnMessageReceived += OnChatSubmitted;
            chatGUIHandler.OnDisconnected += Disconnect;
            //chatGUIHandler.OnConnect += TryConnectAgain;
        }
        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.Login,Connect);
            
            chatGUIHandler.OnMessageReceived -= OnChatSubmitted;
            chatGUIHandler.OnDisconnected -= Disconnect;
            //chatGUIHandler.OnConnect -= TryConnectAgain;
        }
        private void OnApplicationQuit() => Disconnect();
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
                PopUpGUIHandler.Instance.HandlePopupRequest($"Connected to Server! Token-{token}",InfoPopupType.Log);
                
                ReceiveMessages(); 
                
                guiChannel.RaiseChanglePanelState(true);
                servicesChannel.Raise(ServiceEventType.Connect);
            }
            catch (Exception e)
            {
                PopUpGUIHandler.Instance.HandlePopupRequest($"Connection Error: {e.Message}",InfoPopupType.Error);
            }
        }
        public async void Disconnect()
        {
            if (_currentWebSocket != null && _currentWebSocket.State == WebSocketState.Open)
            {
                PopUpGUIHandler.Instance.HandlePopupRequest("Disconnecting...",InfoPopupType.Error);
                await _currentWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "User Quit", CancellationToken.None);
               
                _currentWebSocket = null;
        
                PopUpGUIHandler.Instance.HandlePopupRequest("Disconnected",InfoPopupType.Error);
                guiChannel.RaiseChanglePanelState(false);
                //chatGUIHandler.ChangePanels(false); // Should Be Handled with servicesChannel
                servicesChannel.Raise(ServiceEventType.Disconnect);
                //chatGUIHandler.ClearChat(); 
            }
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
                    //chatGUIHandler.ChangePanels(false);
                    guiChannel.RaiseChanglePanelState(false);
                    Debug.Log("Server closed the connection.");
                    break; 
                }
                
                
                string stringText = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // 1. Log the exact raw text so we can see what the server sent
                Debug.Log($"<color=yellow>RAW SERVER TEXT:</color> {stringText}");

                try
                {
                    // 2. Try to open the Envelope
                    NetworkMessage incomingMessage = JsonUtility.FromJson<NetworkMessage>(stringText);
                    HandleMessageType(incomingMessage);
                }
                catch (Exception)
                {
                    // 3. If it crashes, it means it's NOT JSON. It's probably an old plain-text chat!
                    Debug.LogWarning("Message was not JSON. Treating as plain text.");
                    guiChannel.RaiseMessageToPrint(stringText);
                }
            }
        }
        
        private async void OnChatSubmitted(string message)
        {
            await SendChatMessage(message); 
            guiChannel.RaiseMessageToPrint($"You: {message}");
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
            
            Debug.Log($"Sent: {message}");
        }

        private void HandleMessageType(NetworkMessage message)
        {
            //Debug.Log($"Processing Message Type {message.Type}, Data {message.Data}");
            if (message.Type == "Chat")
            {
                // It's a chat message! Put the Data in the chat box.
                guiChannel.RaiseMessageToPrint(message.Data);
            }
            else if (message.Type == "PlayerList")
            {
                // 1. Take the raw string: ["a","b"]
                string rawJson = message.Data;

                // 2. Clean it up (Remove [ ] and quotes ")
                string cleanString = rawJson.Replace("[", "").Replace("]", "").Replace("\"", "");

                // 3. Split it into an array by the comma
                string[] playerArray = cleanString.Split(',');

                // 4. Send it to our new Lobby UI!
                List<string> players = new List<string>(playerArray);
                guiChannel.RaiseOnPlayersInLobbyChanged(players);
            }
        }
    }
}
