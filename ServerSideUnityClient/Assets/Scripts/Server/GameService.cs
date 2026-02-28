using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Data;
using GameGUI;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    public class GameService : MonoBehaviour
    {
        [SerializeField] GUIMatchAndGame guiMatchAndGame;
        
        [Header("Channels")]
        [SerializeField] private ServicesChannel servicesChannel;
        [SerializeField] private GUIChannel guiChannel;
        
        private ClientWebSocket _socket;
        private bool _isGameActive = false;
        private int _localScore = 0;

        private void Start()
        {
            // Listen for the start signal from MatchService
            servicesChannel.Subscribe(ServiceEventType.StartGameMatch, OnGameStartReceived);
            
            guiMatchAndGame.ScoreButtonClicked += OnGetScoreButtonClicked;
            guiMatchAndGame.EndMatchButtonClicked += OnEndMatchButtonClicked;
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.StartGameMatch, OnGameStartReceived);
            
            guiMatchAndGame.ScoreButtonClicked -= OnGetScoreButtonClicked;
            guiMatchAndGame.EndMatchButtonClicked -= OnEndMatchButtonClicked;
        }


        // Initialize called by ConnectionManager
        public void Initialize(ClientWebSocket socket)
        {
            _socket = socket;
        }
    
        private void OnGameStartReceived()
        {
            _isGameActive = true;
            guiMatchAndGame.ChangePanels(true);
            // Change Panels
            Debug.Log("<color=cyan>[GameService]</color> Game Loop Started. Ready to send scores!");
        }

        // Triggered by "Get Score" Button
        public async void OnGetScoreButtonClicked()
        {
            _localScore++;
            await SendScoreUpdate(_localScore);
            Debug.Log($"Score updated locally and sent: {_localScore}");
        }

        // Triggered by "End Match" Button
        public async void OnEndMatchButtonClicked()
        {
            await SendMatchEnd();
        }

        private async Task SendScoreUpdate(int score)
        {
            if (_socket?.State != WebSocketState.Open) return;

            var msg = new NetworkMessage { Type = "UpdateScore", Data = score.ToString() };
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task SendMatchEnd()
        {
            if (_socket?.State != WebSocketState.Open) return;

            var msg = new NetworkMessage { Type = "MatchEnd", Data = "User ended match" };
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Debug.Log("Match end request sent to server.");
        }
    }
}