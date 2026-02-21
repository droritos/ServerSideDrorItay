using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    public class ConnectionManager : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private ChatService chatService;
        [SerializeField] private LobbyService lobbyService;
        
        [Header("Channels")]
        [SerializeField] private ServicesChannel servicesChannel;
        [SerializeField] private GUIChannel guiChannel;
        
        private ClientWebSocket _socket;
        private const string URL = "ws://localhost:5235/ws";
        
        private void Start()
        {
            servicesChannel.Subscribe(ServiceEventType.Login, Connect);
        }
        
        private async void Connect(string token)
        {
            _socket = new ClientWebSocket();
            Uri serverUri = new Uri($"{URL}?access_token={token}");

            try
            {
                await _socket.ConnectAsync(serverUri, CancellationToken.None);
                
                // Hand the socket to our specialized services
                chatService.Initialize(_socket);
                lobbyService.Initialize(_socket);

                guiChannel.RaiseChanglePanelState(true);
                ReceiveLoop();
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection failed: {e.Message}");
            }
        }
        
        private async void ReceiveLoop()
        {
            byte[] buffer = new byte[1024 * 4];
            while (_socket.State == WebSocketState.Open)
            {
                var result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string rawText = Encoding.UTF8.GetString(buffer, 0, result.Count);

                try
                {
                    NetworkMessage msg = JsonUtility.FromJson<NetworkMessage>(rawText);

                    // ROUTING: Send the data to the correct service based on Type
                    if (msg.Type == "Chat")
                        chatService.HandleChatMessage(msg.Data);
                    else if (msg.Type == "PlayerList")
                        lobbyService.HandlePlayerList(msg.Data);
                }
                catch
                {
                    // /* Handle non-JSON or malformed data */
                }
            }
        }
    }
}
