using UnityEngine;
using Scriptable_Objects;

public class RoomButton : MonoBehaviour
{
    [SerializeField] private GUIChannel guiChannel;
    private string _roomName; // Set this to "Room1", "Room2", etc.
    public void OnClick()
    {
        _roomName = this.gameObject.name;
        // This shouts "Hey, I want to join this room!" into the Channel
        guiChannel.RaiseRoomJoinRequested(_roomName);
    }
}