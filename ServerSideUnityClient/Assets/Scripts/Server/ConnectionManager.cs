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
    /// <summary>
    /// Manages the single WebSocket connection to the game server.
    /// Token comes from AuthService via ServicesChannel Login event.
    /// Routes incoming messages to the right service.
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private ChatService    chatService;
        [SerializeField] private LobbyService   lobbyService;
        [SerializeField] private MatchService   matchService;
        [SerializeField] private GameService    gameService;

        [Header("Channels")]
        [SerializeField] private ServicesChannel servicesChannel;
        [SerializeField] private GUIChannel      guiChannel;

        private ClientWebSocket _socket;
        private const string WS_URL = "ws://127.0.0.1:5235/ws";

        private void Start()
        {
            servicesChannel.Subscribe(ServiceEventType.Login, Connect);
        }

        private void OnApplicationQuit() => Disconnect();
        private void OnDisable()         => Disconnect();

        // ── Connect (called with JWT token) ───────────────────

        private async void Connect(string token)
        {
            _socket = new ClientWebSocket();
            try
            {
                await _socket.ConnectAsync(
                    new Uri($"{WS_URL}?access_token={token}"),
                    CancellationToken.None);

                chatService.Initialize(_socket);
                lobbyService.Initialize(_socket);
                matchService.Initialize(_socket);
                gameService.Initialize(_socket);

                servicesChannel.Raise(ServiceEventType.Connect);
                ReceiveLoop();
            }
            catch (Exception e)
            {
                Debug.LogError($"[WS] Connection failed: {e.Message}");
                PopUpGUIHandler.Instance.HandlePopupRequest(
                    "Could not connect to server.", InfoPopupType.Error);
            }
        }

        // ── Receive loop ──────────────────────────────────────

        private async void ReceiveLoop()
        {
            byte[] buf = new byte[1024 * 4];
            while (_socket?.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _socket.ReceiveAsync(
                        new ArraySegment<byte>(buf), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("[WS] Server closed connection.");
                        break;
                    }

                    string raw = Encoding.UTF8.GetString(buf, 0, result.Count);
                    NetworkMessage msg;
                    try { msg = JsonUtility.FromJson<NetworkMessage>(raw); }
                    catch { continue; }

                    Route(msg);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[WS] Receive error: {e.Message}");
                    break;
                }
            }

            servicesChannel.Raise(ServiceEventType.Disconnect);
        }

        private void Route(NetworkMessage msg)
        {
            switch (msg.Type)
            {
                case "Chat":         chatService.HandleChatMessage(msg.Data);        break;
                case "PlayerList":   lobbyService.HandlePlayerList(msg.Data);        break;
                case "MatchFound":   matchService.HandleMatchFound(msg.Data);        break;
                case "GameStart":    matchService.HandleGameStart();                 break;
                case "MatchEnd":     matchService.HandleGameEnd(msg.Data);           break;  // legacy
                case "MatchResult":  matchService.HandleMatchResult(msg.Data);       break;  // winner name
                case "OpponentScore":matchService.HandleOpponentScoreUpdate(msg.Data);break;
                case "AntiCheat":
                    PopUpGUIHandler.Instance.HandlePopupRequest(msg.Data, InfoPopupType.Error);
                    break;
            }
        }

        // ── Disconnect ────────────────────────────────────────

        private async void Disconnect()
        {
            if (_socket != null && _socket.State == WebSocketState.Open)
            {
                await _socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _socket.Dispose();
            }
        }
    }
}
