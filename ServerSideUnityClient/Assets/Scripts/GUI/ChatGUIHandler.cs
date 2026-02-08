
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatGUIHandler : MonoBehaviour
{
    public event UnityAction<string> OnMessageReceived;
    
    [SerializeField] Transform chatPanel;
    [SerializeField] private TextMeshProUGUI textPrefab;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button submitButton;

    public void AddMessageToChat(string text)
    {
        TextMeshProUGUI textObject = Instantiate(textPrefab, chatPanel);
        textObject.SetText(text);
    }

    public void RaiseOnMessageReceived()
    {
        if(ValidMassage())
            OnMessageReceived?.Invoke(inputField.text);

        ClearInput();
    }

    private bool ValidMassage()
    {
        if(String.IsNullOrWhiteSpace(inputField.text)) 
            return false;
        
        return true;
    }
    
    public void ClearInput()
    {
        inputField.text = "";
        // Optional: Keep focus on the input field so you can type again immediately
        inputField.ActivateInputField(); 
    }


}
