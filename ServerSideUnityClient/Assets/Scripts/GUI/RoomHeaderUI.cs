using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace GameGUI
{
    public class RoomHeaderUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private GUIChannel guiChannel;

        private void Start()
        {
            guiChannel.OnRoomHeaderChanged += UpdateHeader;
            // Set initial state
            UpdateHeader("Lobby"); 
        }

        private void OnDestroy()
        {
            guiChannel.OnRoomHeaderChanged -= UpdateHeader;
        }

        private void UpdateHeader(string roomName)
        {
            headerText.text = $"Current Room: {roomName}";
        }
    }
}
