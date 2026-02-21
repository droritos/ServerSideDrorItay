using System;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace GameGUI
{
    public class GUIManager : MonoBehaviour
    {
        [SerializeField] LoginUIManager loginUIManager;
        [SerializeField] ChatGUIHandler chatGUIHandler;
        [SerializeField] GUILobby lobbyGUIHandler;
        
        // Add a reference to your Room Selection Panel if you made one
        [SerializeField] GameObject roomSelectionPanel; 
        
        [SerializeField] ServicesChannel servicesChannel;
        [SerializeField] GUIChannel guiChannel; // We need this to listen for room changes

        private void Start()
        {
            guiChannel.RaiseRoomHeaderChanged("Login"); // First Room U see
            
            servicesChannel.Subscribe(ServiceEventType.Connect, ConnectToChat);
            servicesChannel.Subscribe(ServiceEventType.Disconnect, DisconnectFromChat);
            
            // Listen for when a player successfully joins a room to update UI
            guiChannel.OnRoomJoinRequested += HandleRoomTransition;
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.Connect, ConnectToChat);
            servicesChannel.Unsubscribe(ServiceEventType.Disconnect, DisconnectFromChat);
            
            guiChannel.OnRoomJoinRequested -= HandleRoomTransition;
        }

        public void ConnectToChat()
        {
            loginUIManager.gameObject.SetActive(false);
            
            // When we first connect, we show the Lobby and Room Selection
            lobbyGUIHandler.gameObject.SetActive(true);
            if(roomSelectionPanel != null) 
                roomSelectionPanel.SetActive(true);
            
            chatGUIHandler.gameObject.SetActive(true); 
            
            guiChannel.RaiseRoomHeaderChanged("Lobby"); // First Room U see
        }

        public void DisconnectFromChat()
        {
            loginUIManager.gameObject.SetActive(true);
            chatGUIHandler.gameObject.SetActive(false);
            lobbyGUIHandler.gameObject.SetActive(false);
            if(roomSelectionPanel != null) roomSelectionPanel.SetActive(false);
            
            chatGUIHandler.ClearChatPanel();
        }

        private void HandleRoomTransition(string roomName)
        {
            // Optional: When they join a room, you could clear the old chat 
            // so they don't see the Lobby's messages in Room 1.
            chatGUIHandler.ClearChatPanel();
            guiChannel.RaiseRoomHeaderChanged(roomName);
            
            Debug.Log($"GUI switching focus to: {roomName}");
        }
    }
}