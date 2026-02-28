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
        [SerializeField] GUILobbyElements lobbyElementsGUIHandler;
        [SerializeField] GUIMatchAndGame matchAndGameGUIHandler;
        
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
            servicesChannel.Subscribe(ServiceEventType.StartGameMatch,TransitionStartMatch);
            servicesChannel.Subscribe(ServiceEventType.EndGameMatch, TransitionEndMatch);
            
            
            // Listen for when a player successfully joins a room to update UI
            guiChannel.OnRoomJoinRequested += HandleRoomTransition;
            guiChannel.OnMatchFoundUI += HandleMatchFoundUI;
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.Connect, ConnectToLobby);
            servicesChannel.Unsubscribe(ServiceEventType.Disconnect, DisconnectFromLobby);
            servicesChannel.Unsubscribe(ServiceEventType.StartGameMatch,TransitionStartMatch);
            servicesChannel.Unsubscribe(ServiceEventType.EndGameMatch, TransitionEndMatch);
            
            guiChannel.OnRoomJoinRequested -= HandleRoomTransition;
            guiChannel.OnMatchFoundUI -= HandleMatchFoundUI;
        }

        public void ConnectToLobby()
        {
            loginUIManager.gameObject.SetActive(false);
    
            matchAndGameGUIHandler.ChangePanels(false);
            matchAndGameGUIHandler.gameObject.SetActive(true);
    
            // Show Lobby and Social elements
            lobbyElementsGUIHandler.gameObject.SetActive(true);
            chatGUIHandler.gameObject.SetActive(true); 
    
            if(roomSelectionPanel != null) 
                roomSelectionPanel.SetActive(true);
    
            guiChannel.RaiseRoomHeaderChanged("Lobby");
        }
        public void DisconnectFromLobby()
        {
            loginUIManager.gameObject.SetActive(true);
            chatGUIHandler.gameObject.SetActive(false);
            lobbyElementsGUIHandler.gameObject.SetActive(false);
            matchAndGameGUIHandler.gameObject.SetActive(false);
            if(roomSelectionPanel != null) roomSelectionPanel.SetActive(false);
            
            chatGUIHandler.ClearChatPanel();
            Debug.Log($"[GUIManager] Disconnected From Lobby.");
            guiChannel.RaiseRoomHeaderChanged("Unknown");
        }
        private void TransitionEndMatch(string score)
        {
            Debug.Log($"[GUIManager] Match ended with score: {score}. Transitioning to Lobby.");
            guiChannel.RaiseMatchFoundUI(String.Empty);  // Update versus text to genric
            ConnectToLobby();
        }
        private void TransitionStartMatch()
        {
            chatGUIHandler.ClearChatPanel();
            if(roomSelectionPanel != null) roomSelectionPanel.SetActive(false);
            lobbyElementsGUIHandler.gameObject.SetActive(false);
        }
        private void HandleRoomTransition(string roomName)
        {
            chatGUIHandler.ClearChatPanel();
            guiChannel.RaiseRoomHeaderChanged(roomName);
            
            Debug.Log($"GUI switching focus to: {roomName}");
        }

        private void HandleMatchFoundUI(string opponentName) // Update Match vs Text
        {
            if(string.IsNullOrEmpty(opponentName))
                guiChannel.RaiseRoomHeaderChanged("Search Match"); // First Room U see
            else
                guiChannel.RaiseRoomHeaderChanged("Match VS " + opponentName); // First Room U see
            
            roomSelectionPanel.SetActive(false);
        }
    }
}