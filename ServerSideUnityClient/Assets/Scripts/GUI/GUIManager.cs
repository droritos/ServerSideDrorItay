using System;
using Data;
using Scriptable_Objects;
using UnityEngine;

namespace GameGUI
{
    public class GUIManager : MonoBehaviour
    {
        [Header("GUI Handlers")]
        [SerializeField] LoginUIManager loginUIManager;
        [SerializeField] ChatGUIHandler chatGUIHandler;
        [SerializeField] GUIPlayersOnline playersOnlineGUIHandler;
        [SerializeField] GameGUI.GUIMatchAndGame matchAndGameGUIHandler;
        
        // Add a reference to your Room Selection Panel if you made one
        [Header("Game Objects")]
        [SerializeField] private GameObject afterLoginObject; // Chat , Rooms , Match
        [SerializeField] GameObject roomSelectionPanel; 
        
        [Header("Services")]
        [SerializeField] ServicesChannel servicesChannel;
        [SerializeField] GUIChannel guiChannel; // We need this to listen for room changes

        private void Start()
        {
            guiChannel.RaiseRoomHeaderChanged("Login"); // First Room U see , There is no Main Menu in that Prototype
            
            servicesChannel.Subscribe(ServiceEventType.Connect, ConnectToLobby);
            servicesChannel.Subscribe(ServiceEventType.Disconnect, DisconnectFromLobby);
            
            // Listen for when a player successfully joins a room to update UI
            guiChannel.OnRoomJoinRequested += HandleRoomTransition;
            guiChannel.OnMatchFoundUI += HandleMatchFoundUI;
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.Connect, ConnectToLobby);
            servicesChannel.Unsubscribe(ServiceEventType.Disconnect, DisconnectFromLobby);
            
            guiChannel.OnRoomJoinRequested -= HandleRoomTransition;
            guiChannel.OnMatchFoundUI -= HandleMatchFoundUI;
        }

        public void ConnectToLobby()
        {
            loginUIManager.gameObject.SetActive(false);
            
            // When we first connect, we show the Lobby and Room Selection And Match Panel
            playersOnlineGUIHandler.gameObject.SetActive(true);
            matchAndGameGUIHandler.gameObject.SetActive(true);
            
            if(roomSelectionPanel != null) 
                roomSelectionPanel.SetActive(true);
            
            chatGUIHandler.gameObject.SetActive(true); 
            
            guiChannel.RaiseRoomHeaderChanged("Lobby"); // First Room U probely enter
        }

        public void DisconnectFromLobby()
        {
            loginUIManager.gameObject.SetActive(true);
            chatGUIHandler.gameObject.SetActive(false);
            playersOnlineGUIHandler.gameObject.SetActive(false);
            if(roomSelectionPanel != null) roomSelectionPanel.SetActive(false);
            
            chatGUIHandler.ClearChatPanel();
        }

        private void HandleRoomTransition(string roomName)
        {
            chatGUIHandler.ClearChatPanel();
            guiChannel.RaiseRoomHeaderChanged(roomName);
            
            Debug.Log($"GUI switching focus to: {roomName}");
        }

        private void HandleMatchFoundUI(string opponentName)
        {
            guiChannel.RaiseRoomHeaderChanged("Match VS " + opponentName); // First Room U see
            roomSelectionPanel.SetActive(false);
            playersOnlineGUIHandler.gameObject.SetActive(false);
        }
    }
}