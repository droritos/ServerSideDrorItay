using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "GUIEvents", menuName = "Channels/GUI Events")]
    public class GUIChannel : ScriptableObject
    {
        public event UnityAction<string> OnMessageToPrint;
        public event UnityAction<bool> ChanglePanelState;
        public event UnityAction<List<string>> OnPlayersInLobbyChanged;
        public event UnityAction<string> OnRoomJoinRequested;

        public void RaiseMessageToPrint(string message)
        {
            OnMessageToPrint?.Invoke(message);
        }

        public void RaiseChanglePanelState(bool changlePanelState)
        {
            ChanglePanelState?.Invoke(changlePanelState);
        }

        public void RaiseOnPlayersInLobbyChanged(List<string> playersInLobby)
        {
            OnPlayersInLobbyChanged?.Invoke(playersInLobby);
        }
        public void RaiseRoomJoinRequested(string roomName)
        {
            OnRoomJoinRequested?.Invoke(roomName);
        }
    }
}
