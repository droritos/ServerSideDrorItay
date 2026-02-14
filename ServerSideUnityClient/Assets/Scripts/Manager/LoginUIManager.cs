using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using Server; 

public class LoginUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AuthService authService; 

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI errorText; 

    private void Start()
    {
       
        loginButton.onClick.AddListener(OnLoginClicked);

       
        if (errorText != null) errorText.text = "";
    }

    private void OnLoginClicked()
    {
        string user = usernameInput.text;
        string pass = passwordInput.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            if (errorText != null) errorText.text = "Please enter both username and password.";
            return;
        }

        Debug.Log($"Attempting login for: {user}");
        authService.Login(user, pass);
    }
}