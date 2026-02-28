using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Data;
using GameGUI;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    public class ConnectionManager : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private ChatService chatService;
        [SerializeField] private LobbyService lobbyService;
        [SerializeField] private MatchService matchService;
        [SerializeField] private GameService gameService;
        
        [Header("Channels")]
        [SerializeField] private ServicesChannel servicesChannel;
        [SerializeField] private GUIChannel guiChannel;
        
        private ClientWebSocket _socket;
        private const string URL = "ws://localhost:5235/ws";

        #region << Unity Functions >>
        private void Start()
        {
            servicesChannel.Subscribe(ServiceEventType.Login, Connect);
        }
        private void OnApplicationQuit()
        {
            CleanupSocket();
        }

        private void OnDisable()
        {
            CleanupSocket();
        }
        #endregion
        
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
                matchService.Initialize(_socket);
                gameService.Initialize(_socket);

                servicesChannel.Raise(ServiceEventType.Connect);
                //guiChannel.RaiseChanglePanelState(true);
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
                    // Inside your Message Handler
                    if (msg.Type == "MatchFound")
                    {
                        string opponentName = msg.Data;
                        matchService.HandleMatchFound(opponentName);
                    }
                    else if (msg.Type == "GameStart")
                        matchService.HandleGameStart();
                    else if (msg.Type == "MatchEnd")
                        matchService.HandleGameEnd(msg.Data);// Send Score to server and save to DB
                }
                catch
                {
                    // /* Handle non-JSON or malformed data */
                }
            }
        }
        private async void CleanupSocket()
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _socket.Dispose();
            }
        }
    }
}
