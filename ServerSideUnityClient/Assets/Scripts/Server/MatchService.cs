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
    /// Handles all match-lifecycle messages:
    ///  FindMatch, Ready, MatchFound, GameStart, OpponentScore, MatchResult.
    /// </summary>
    public class MatchService : MonoBehaviour
    {
        [SerializeField] private GUIMatchAndGame  guiMatchAndGame;
        [Header("Channels")]
        [SerializeField] private GUIChannel       guiChannel;
        [SerializeField] private ServicesChannel  servicesChannel;

        private ClientWebSocket _socket;
        private string _opponentName = string.Empty;

        private void Start()
        {
            servicesChannel.Subscribe(ServiceEventType.Connect,
                () => guiMatchAndGame.ChangePanels(false));

            guiMatchAndGame.OnReadyToMatchButtonClicked += SendReadySignal;
            guiMatchAndGame.FindMatchButtonClicked      += SendFindMatchRequest;
        }

        private void OnDestroy()
        {
            guiMatchAndGame.OnReadyToMatchButtonClicked -= SendReadySignal;
            guiMatchAndGame.FindMatchButtonClicked      -= SendFindMatchRequest;
        }

        public void Initialize(ClientWebSocket socket) => _socket = socket;

        // ── Incoming handlers ─────────────────────────────────

        public void HandleMatchFound(string opponentName)
        {
            _opponentName = opponentName;
            guiChannel.RaiseMatchFoundUI(opponentName);
            PopUpGUIHandler.Instance.HandlePopupRequest($"Match found! vs {opponentName}", InfoPopupType.Log);
        }

        public void HandleGameStart()
        {
            Debug.Log("[Match] Server says GO!");
            servicesChannel.Raise(ServiceEventType.StartGameMatch);
        }

        public void HandleOpponentScoreUpdate(string data)
        {
            // data format: "OpponentName:score"
            string[] parts = data.Split(':');
            if (parts.Length == 2)
                guiChannel.RaiseOpponentScoreChanged(data);
        }

        /// <summary>Legacy MatchEnd message – data is the winning score string.</summary>
        public void HandleGameEnd(string scoreData)
        {
            servicesChannel.Raise(ServiceEventType.EndGameMatch, scoreData);
        }

        /// <summary>New MatchResult message – data is the winner's username.</summary>
        public void HandleMatchResult(string winnerName)
        {
            string me = PlayerPrefs.GetString("LastUsername", "");
            bool iWon = winnerName.Equals(me, System.StringComparison.OrdinalIgnoreCase);

            string resultMsg = iWon ? "You WIN! 🏆" : $"{winnerName} wins!";
            PopUpGUIHandler.Instance.HandlePopupRequest(resultMsg,
                iWon ? InfoPopupType.Log : InfoPopupType.Warning);

            servicesChannel.Raise(ServiceEventType.EndGameMatch, winnerName);
        }

        // ── Outgoing ──────────────────────────────────────────

        private async void SendFindMatchRequest()
        {
            await SendMsg(new NetworkMessage { Type = "FindMatch", Data = "" });
            Debug.Log("[Match] FindMatch sent.");
        }

        private async void SendReadySignal()
        {
            await SendMsg(new NetworkMessage { Type = "Ready", Data = "" });
            Debug.Log("[Match] Ready sent.");
        }

        private async System.Threading.Tasks.Task SendMsg(NetworkMessage msg)
        {
            if (_socket?.State != WebSocketState.Open) return;
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(msg));
            await _socket.SendAsync(
                new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
