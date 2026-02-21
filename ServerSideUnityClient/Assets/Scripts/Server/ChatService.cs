using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace Server
{
    public class ChatService : MonoBehaviour
    {
        [Header("Channels")]
        [SerializeField] GUIChannel guiChannel;
        [SerializeField] ChatGUIHandler chatGUIHandler;

        private ClientWebSocket _socket;

        private void Start()
        {
            chatGUIHandler.OnMessageReceived += OnChatSubmitted;
        }

        private void OnDestroy()
        {
            chatGUIHandler.OnMessageReceived -= OnChatSubmitted;
        }

        // The ConnectionManager will call this once the socket is ready
        public void Initialize(ClientWebSocket socket)
        {
            _socket = socket;
        }

        private async void OnChatSubmitted(string messageText)
        {
            if (_socket == null || _socket.State != WebSocketState.Open) return;

            // 1. Pack into Envelope
            NetworkMessage msg = new NetworkMessage { Type = "Chat", Data = messageText };
            string json = JsonUtility.ToJson(msg);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            // 2. Send
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            
            // 3. Local echo
            guiChannel.RaiseMessageToPrint($"You: {messageText}");
        }

        public void HandleChatMessage(string data)
        {
            guiChannel.RaiseMessageToPrint(data);
        }
    }
}