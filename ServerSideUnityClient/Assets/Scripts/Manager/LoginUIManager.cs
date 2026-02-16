using Data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoginUIManager : MonoBehaviour
{
    public event UnityAction<string, string> LoginResponse;
    public event UnityAction<string, string> RegisterResponse;

    [Header("UI Text")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("UI Button")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;

    private void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
        registerButton.onClick.AddListener(OnRegisterClicked);
    }

    private void OnLoginClicked()
    {
        TrySubmit(LoginResponse, "login");
    }

    private void OnRegisterClicked()
    {
        TrySubmit(RegisterResponse, "register");
    }

    private void TrySubmit(UnityAction<string, string> response, string actionName)
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        InfoPopupArgs infoPopupArgs = new InfoPopupArgs();
        
        
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {

            // Pop Up
            infoPopupArgs.Text = "Please enter both username and password.";
            infoPopupArgs.Type = InfoPopupType.Error;
            PopUpGUIHandler.Instance.HandlePopupRequest(infoPopupArgs);
            return;
        }

        // Pop Up
        infoPopupArgs.Text = $"Attempting {actionName} for: {user}";
        infoPopupArgs.Type = InfoPopupType.Error;

        response?.Invoke(user, pass);
    }
}