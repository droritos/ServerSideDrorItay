using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Events;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "GUIEvents", menuName = "Channels/GUI Events")]
    public class GUIChannel : ScriptableObject
    {
        public event UnityAction<string>          OnMessageToPrint;
        public event UnityAction<string>          OnMatchFoundUI;
        public event UnityAction<bool>            ChanglePanelState;
        public event UnityAction<List<string>>    OnPlayersInLobbyChanged;
        public event UnityAction<string>          OnRoomJoinRequested;
        public event UnityAction<string>          OnRoomHeaderChanged;
        public event UnityAction<List<MatchResult>> OnLeaderboardChanged;
        public event UnityAction<string>          OnOpponentScoreChanged;  // NEW

        public void RaiseMessageToPrint(string message)        => OnMessageToPrint?.Invoke(message);
        public void RaiseChanglePanelState(bool state)         => ChanglePanelState?.Invoke(state);
        public void RaiseOnPlayersInLobbyChanged(List<string> p)=> OnPlayersInLobbyChanged?.Invoke(p);
        public void RaiseRoomJoinRequested(string room)        => OnRoomJoinRequested?.Invoke(room);
        public void RaiseRoomHeaderChanged(string room)        => OnRoomHeaderChanged?.Invoke(room);
        public void RaiseMatchFoundUI(string opponent)         => OnMatchFoundUI?.Invoke(opponent);
        public void RaiseLeaderboardChanged(List<MatchResult> r)=> OnLeaderboardChanged?.Invoke(r);
        public void RaiseOpponentScoreChanged(string data)     => OnOpponentScoreChanged?.Invoke(data); // NEW
    }
}
