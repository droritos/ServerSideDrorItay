using System;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GameGUI
{
    public class GUIMatchAndGame : MonoBehaviour
    {
        public event UnityAction OnReadyToMatchButtonClicked;
        public event UnityAction FindMatchButtonClicked;
        public event UnityAction ScoreButtonClicked;
        public event UnityAction EndMatchButtonClicked;
        
        [Header("Panels References")]
        [SerializeField] GameObject matchPanel;
        [SerializeField] GameObject gamePanel;
        
        [Header("Match References")]
        [SerializeField] TextMeshProUGUI versusText;
        [SerializeField] Button ReadyButton;
        [SerializeField] Button FindMatchButton;
        
        [Header("Game References")]
        [SerializeField] Button ScoreButton;
        [SerializeField] Button EndMatchButton;
        
        [Header("Channels")]
        [SerializeField] GUIChannel  guiChannel;

        private void Start()
        {
            ButtonsOnClick();
            guiChannel.OnMatchFoundUI += SetVersusText;
        }

        private void OnDestroy()
        {
            guiChannel.OnMatchFoundUI -= SetVersusText;
        }

        public void ChangePanels(bool isInGame)
        {
            // From Match -> Game or Oppisite

            if (isInGame)
            {
                gamePanel.SetActive(true);
                matchPanel.SetActive(false);
            }
            else
            {
                gamePanel.SetActive(false);
                matchPanel.SetActive(true);
            }
            Debug.Log($"Now Panel Is Changed: {isInGame}");
        }
        
        private void SetVersusText(string opponentName)
        {
            versusText.SetText("You VS " + opponentName);
        }

        private void ButtonsOnClick()
        {
            ReadyButton.onClick.RemoveAllListeners();
            FindMatchButton.onClick.RemoveAllListeners();
            ScoreButton.onClick.RemoveAllListeners();
            EndMatchButton.onClick.RemoveAllListeners();
            
            // Match
            ReadyButton.onClick.AddListener(OnReadyToMatchButtonClicked);
            FindMatchButton.onClick.AddListener(FindMatchButtonClicked);
            
            // Game
            ScoreButton.onClick.AddListener(ScoreButtonClicked);
            EndMatchButton.onClick.AddListener(EndMatchButtonClicked);
        }
    }
}
