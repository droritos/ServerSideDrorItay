
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatGUIHandler : MonoBehaviour
{
    public event UnityAction<string> OnMessageReceived;
    public event UnityAction OnDisconnected;
    public event UnityAction OnConnect;
    
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
    
    public void AddMessageToChat(string text)
    {
        TextMeshProUGUI textObject = Instantiate(textPrefab, chatPanel);
        textObject.SetText(text);
    }

    public void ChangePanels(bool connected)
    {
        chatRoomPanel.gameObject.SetActive(connected);
        disconnectedPanel.gameObject.SetActive(!connected);
    }
    #region << Button Events >>  

    public void RaiseOnMessageReceived() // Also Connect to the submitButton!
    {
        if(ValidMassage())
            OnMessageReceived?.Invoke(inputField.text);

        ClearInput();
    }

    public void RaiseDisconnected() // Also Connect to the submitButton!
    {
        OnDisconnected?.Invoke();
    }

    public void RaiseOnConnect()
    {
        OnConnect?.Invoke();
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
