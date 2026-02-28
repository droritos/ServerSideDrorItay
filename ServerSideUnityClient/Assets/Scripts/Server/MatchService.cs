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
    public class MatchService : MonoBehaviour
    {
        
        [SerializeField] GUIMatchAndGame  guiMatchAndGame;
        [Header("Channels")]
        [SerializeField] private GUIChannel guiChannel;
        [SerializeField] ServicesChannel servicesChannel;

        private ClientWebSocket _socket;
        private string _opponentName;

        private void Start()
        {
            servicesChannel.Subscribe(ServiceEventType.Connect, () => guiMatchAndGame.ChangePanels(false));

            guiMatchAndGame.OnReadyToMatchButtonClicked += SendReadySignal;
    
            // This button tells the server "I want to play!"
            guiMatchAndGame.FindMatchButtonClicked += SendFindMatchAndGameRequest; 
        }
        public void Initialize(ClientWebSocket socket)
        {
            _socket = socket;
        }
        public void HandleMatchFound(string opponentName)
        {
            _opponentName = opponentName;
            Debug.Log($"Match Found! Enemy: {_opponentName}");

            // We need to tell the UI to show the Match/VS panel
            // You can add a new event to your GUIChannel for this
            guiChannel.RaiseMatchFoundUI(opponentName);
            
            PopUpGUIHandler.Instance.HandlePopupRequest("Found Match!",InfoPopupType.Log);
        }
        public void HandleGameStart()
        {
            Debug.Log("Both players ready! The server says GO!");
            servicesChannel.Raise(ServiceEventType.StartGameMatch);
            // Example: SceneManager.LoadScene("GameLevel");
        }
        public void HandleOpponentScoreUpdate(string data)
        {
            // You can split the string if you sent Name:Score
            // string[] split = data.Split(':');
            // string score = split[1];

            // Trigger the UI to update the opponent's score text
            //guiChannel.RaiseOpponentScoreChanged(data); 
        }
        public void HandleGameEnd(string scoreData)
        {
            Debug.Log($"<color=orange>Server signaled Match End. Score: {scoreData}</color>");
    
            //guiMatchAndGame.ChangePanels(false); 
            servicesChannel.Raise(ServiceEventType.EndGameMatch, scoreData);
        }
        private async void SendFindMatchAndGameRequest()
        {
            if (_socket == null || _socket.State != WebSocketState.Open) return;

            NetworkMessage msg = new NetworkMessage { Type = "FindMatch", Data = "" };
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log("Sent FindMatch request to server...");
        }
     
        private async void SendReadySignal()
        {
            if (_socket == null || _socket.State != WebSocketState.Open) return;

            NetworkMessage msg = new NetworkMessage { Type = "Ready", Data = "" };
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            
            Debug.Log("Ready signal sent. Waiting for opponent...");
        }
        
    }
}