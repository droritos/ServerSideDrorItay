using System;
using Data;
using GUI;
using Scriptable_Objects;
using UnityEngine;

namespace GameGUI
{
    public class GUIManager : MonoBehaviour
    {
        [SerializeField] LoginUIManager loginUIManager;
        [SerializeField] ChatGUIHandler chatGUIHandler;
        [SerializeField] GUILobby lobbyGUIHandler;
        
        [SerializeField] ServicesChannel  servicesChannel;

        private void Awake()
        {
            servicesChannel.Subscribe(ServiceEventType.Connect,ConnectToChat);
            servicesChannel.Subscribe(ServiceEventType.Disconnect,DisconnectFromChat);
        }

        private void OnDestroy()
        {
            servicesChannel.Unsubscribe(ServiceEventType.Connect,ConnectToChat);
            servicesChannel.Unsubscribe(ServiceEventType.Disconnect,DisconnectFromChat);
        }

        public void ConnectToChat()
        {
            loginUIManager.gameObject.SetActive(false);
            chatGUIHandler.gameObject.SetActive(true);
            lobbyGUIHandler.gameObject.SetActive(true);
        }
        public void DisconnectFromChat()
        {
            loginUIManager.gameObject.SetActive(true);
            chatGUIHandler.gameObject.SetActive(false);
            lobbyGUIHandler.gameObject.SetActive(false);
            chatGUIHandler.ClearChatPanel();
        }

    }
}
