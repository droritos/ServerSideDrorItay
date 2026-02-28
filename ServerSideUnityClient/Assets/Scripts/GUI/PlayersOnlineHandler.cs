using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameGUI
{
    public class PlayersOnlineHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform container;      // The 'Content' of your ScrollRect
        [SerializeField] private TextMeshProUGUI rowPrefab; // A simple TextMeshPro prefab

        public void UpdatePlayerList(List<string> players)
        {
            // 1. Clear existing rows to prevent duplicates
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            // 2. Spawn a new row for every player currently in the lobby
            foreach (string playerName in players)
            {
                TextMeshProUGUI playerRow = Instantiate(rowPrefab, container);
                
                // Requirement 7: Show current players in lobby
                playerRow.SetText(playerName);
            }
            
            Debug.Log($"[UI] Updated Player List. {players.Count} players online.");
        }
    }
}