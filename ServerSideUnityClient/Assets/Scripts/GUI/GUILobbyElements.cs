using System;
using System.Collections.Generic;
using Data;
using Scriptable_Objects; // Needed for List
using TMPro;
using UnityEngine;

namespace GameGUI
{
    public class GUILobbyElements : MonoBehaviour
    {
        [SerializeField] private GUIChannel guiChannel;
        [SerializeField] PlayersOnlineHandler playersOnlineHandler;
        [SerializeField] LeaderboardUIHandler leaderboardUIHandler;

        private void Start()
        {
            guiChannel.OnPlayersInLobbyChanged += UpdatePlayerList;
            guiChannel.OnLeaderboardChanged += UpdateLeaderboard;
        }

 

        private void OnDestroy()
        {
            guiChannel.OnPlayersInLobbyChanged -= UpdatePlayerList;
            guiChannel.OnLeaderboardChanged -= UpdateLeaderboard;
        }

        // This is the "Public Entrance" for the data
        public void UpdatePlayerList(List<string> players)
        {
            playersOnlineHandler.UpdatePlayerList(players);
        }
        private void UpdateLeaderboard(List<MatchResult> results)
        {
            leaderboardUIHandler.Populate(results);
        }
    }
}