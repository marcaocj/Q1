// Assets/Scripts/Login/LoginUIController.cs
using UnityEngine;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    [Header("UI Elements")]
    public InputField usernameField;
    public InputField passwordField;
    public Button loginButton;
    public Button quitButton;
    public Text statusText;
    public GameObject loadingPanel;
    
    private LoginManager loginManager;
    
    private void Start()
    {
        loginManager = GetComponent<LoginManager>();
        
        // Setup button events
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        
        // Clear status text
        statusText.text = "";
        loadingPanel.SetActive(false);
    }
    
    private void OnLoginButtonClicked()
    {
        string username = usernameField.text;
        string password = passwordField.text;
        
        if (ValidateInput(username, password))
        {
            SetUIState(false);
            loadingPanel.SetActive(true);
            statusText.text = "Authenticating...";
            
            loginManager.AttemptLogin(username, password, OnLoginResult);
        }
    }
    
    private bool ValidateInput(string username, string password)
    {
        if (string.IsNullOrEmpty(username))
        {
            statusText.text = "Please enter a username.";
            return false;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter a password.";
            return false;
        }
        
        return true;
    }
    
    private void OnLoginResult(bool success, string message)
    {
        SetUIState(true);
        loadingPanel.SetActive(false);
        statusText.text = message;
        
        if (success)
        {
            statusText.color = Color.green;
        }
        else
        {
            statusText.color = Color.red;
        }
    }
    
    private void SetUIState(bool interactable)
    {
        usernameField.interactable = interactable;
        passwordField.interactable = interactable;
        loginButton.interactable = interactable;
    }
    
    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}