using System;
using System.Collections.Generic;
using Scriptable_Objects; // Needed for List
using TMPro;
using UnityEngine;

namespace GameGUI
{
    public class GUILobby : MonoBehaviour
    {
        [SerializeField] private GUIChannel guiChannel;
        [SerializeField] TextMeshProUGUI playerListText;

        private const string Players = "Players Online\n";
        
        private void Start()
        {
            guiChannel.OnPlayersInLobbyChanged += UpdatePlayerList;
        }

        private void OnDestroy()
        {
            guiChannel.OnPlayersInLobbyChanged -= UpdatePlayerList;
        }

        // This is the "Public Entrance" for the data
        public void UpdatePlayerList(List<string> players)
        {
            // Clear the old text
            playerListText.text = Players;

            // Loop through the list and add each name on a new line
            foreach (string playerName in players)
            {
                playerListText.text += playerName + "\n";
            }
        }
    }
}