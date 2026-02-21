
using System;
using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatGUIHandler : MonoBehaviour
{
    public event UnityAction<string> OnMessageReceived; // Massage is read from the Input Text Mesh Pro Object
    
    [SerializeField] GUIChannel guiChannel;
    
    [Header("Refences")]
    [SerializeField] Transform chatPanel;
    [SerializeField] private TextMeshProUGUI textPrefab;
    [SerializeField] TMP_InputField inputField;
    
    [Header("Chat Buttons")]
    [SerializeField] Button submitButton;
    [SerializeField] Button disconnectButton;
    [SerializeField] Button connectButton;
    
    [Header("Chat Panels")]
    [SerializeField] RectTransform chatRoomPanel;
    [SerializeField] RectTransform disconnectedPanel;

    #region << Unity Functions >>
    private void Start()
    {
        guiChannel.OnMessageToPrint += AddMessageToChat;
        guiChannel.ChanglePanelState += ChangePanels;
    }

    private void OnDestroy()
    {
        guiChannel.OnMessageToPrint -= AddMessageToChat;
        guiChannel.ChanglePanelState -= ChangePanels;
    }
    private void Update()
    {
        bool input = Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return);
    
        if (input && !string.IsNullOrWhiteSpace(inputField.text))
        {
            RaiseOnMessageReceived(); 
        
            inputField.text = ""; // Clear

            // Keep the cursor blinking inside
            inputField.ActivateInputField(); 
        }
    }
    #endregion
    
    private void AddMessageToChat(string text)
    {
        TextMeshProUGUI textObject = Instantiate(textPrefab, chatPanel);
        textObject.SetText(text);
    }

    private void ChangePanels(bool connected) // // Should Be Handled with servicesChannel instead
    {
        chatRoomPanel.gameObject.SetActive(connected);
        disconnectedPanel.gameObject.SetActive(!connected);
    }

    public void ClearChatPanel()
    {
        foreach (Transform child in chatPanel)
        {
            Destroy(child.gameObject);
        }
    }
    #region << Button Events >>  

    public void RaiseOnMessageReceived() // Also Connect to the submitButton!
    {
        if(ValidMassage())
            OnMessageReceived?.Invoke(inputField.text);

        ClearInput();
    }

    #endregion
    private bool ValidMassage()
    {
        if(String.IsNullOrWhiteSpace(inputField.text)) 
            return false;
        
        return true;
    }
    
    private void ClearInput()
    {
        inputField.text = "";
        // Optional: Keep focus on the input field so you can type again immediately
        inputField.ActivateInputField(); 
    }


}
