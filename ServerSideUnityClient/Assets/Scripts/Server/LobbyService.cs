using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Data;
using Scriptable_Objects; // Make sure this matches your NetworkMessage namespace
using UnityEngine;

namespace Server
{
    public class LobbyService : MonoBehaviour
    {
        [SerializeField] private GUIChannel guiChannel;
        private ClientWebSocket _socket;

        private void Start() 
        {
            // Subscribe to the button click event from the UI
            guiChannel.OnRoomJoinRequested += SendJoinRoomRequest;
        }

        private void OnDestroy()
        {
            guiChannel.OnRoomJoinRequested -= SendJoinRoomRequest;
        }

        public void Initialize(ClientWebSocket socket) => _socket = socket;

        public void HandlePlayerList(string rawJson)
        {
            // 1. Clean the string: ["a","b"] -> a,b
            string cleanString = rawJson.Replace("[", "").Replace("]", "").Replace("\"", "");

            // 2. Split into array and convert to List
            string[] playerArray = cleanString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> players = new List<string>(playerArray);

            // 3. Now it's a List<string>, so guiChannel will accept it!
            guiChannel.RaiseOnPlayersInLobbyChanged(players);
        }

        private async void SendJoinRoomRequest(string roomName)
        {
            try
            {
                if (_socket == null || _socket.State != WebSocketState.Open) return;

                // Create the envelope
                NetworkMessage netMsg = new NetworkMessage
                {
                    Type = "JoinRoom",
                    Data = roomName
                };

                // Pack and Send
                string json = JsonUtility.ToJson(netMsg);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                
                Debug.Log($"Requested to join: {roomName}");
            }
            catch (Exception e)
            {
                Debug.Log($"Failed to join Room: {e}");
            }
        }
    }
}