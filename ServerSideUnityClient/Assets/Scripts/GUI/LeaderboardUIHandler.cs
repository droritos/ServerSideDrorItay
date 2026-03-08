using System.Collections.Generic;
using Data;
using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace GameGUI
{
    public class LeaderboardUIHandler : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private TextMeshProUGUI rowPrefab;
        [SerializeField] private GUIChannel guiChannel; // ADD THIS IN INSPECTOR

        private void Start()
        {
            guiChannel.OnLeaderboardChanged += Populate;
        }

        private void OnDestroy()
        {
            guiChannel.OnLeaderboardChanged -= Populate;
        }

        public void Populate(List<MatchResult> results)
        {
            Debug.Log($"[Leaderboard] Populating with {results.Count} entries");

            foreach (Transform child in container)
                Destroy(child.gameObject);

            foreach (var res in results)
            {
                TextMeshProUGUI textInstance = Instantiate(rowPrefab, container);
                textInstance.SetText($"<color=#FFD700>{res.username}:</color> {res.score} pts");
            }
        }
    }
}
