using System.Collections.Generic;
using Data;
using TMPro;
using UnityEngine;

// Assuming MatchResult is here

namespace GameGUI
{
    public class LeaderboardUIHandler : MonoBehaviour
    {
        [SerializeField] private Transform container; // The 'Content' of your ScrollView
        [SerializeField] private TextMeshProUGUI rowPrefab;  // Prefab with Name/Score fields

        public void Populate(List<MatchResult> results)
        {
            foreach (Transform child in container) Destroy(child.gameObject);

            foreach (var res in results)
            {
                TextMeshProUGUI textInstance = Instantiate(rowPrefab, container);
                // Requirement 9: Show Name and Score clearly
                textInstance.SetText($"<color=#FFD700>{res.username}:</color> {res.score} pts");
            }
        }
    }
}