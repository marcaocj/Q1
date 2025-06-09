// Assets/Scripts/UI/PauseMenu.cs
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject attributesPanel;
    
    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button attributesButton;
    public Button saveButton;
    public Button mainMenuButton;
    public Button quitButton;
    
    [Header("Settings UI")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    
    [Header("Attributes UI")]
    public Text strengthText;
    public Text dexterityText;
    public Text intelligenceText;
    public Text vitalityText;
    public Text availablePointsText;
    public Button strengthButton;
    public Button dexterityButton;
    public Button intelligenceButton;
    public Button vitalityButton;
    
    private bool isPaused = false;
    private PlayerStats playerStats;
    private AttributeSystem attributeSystem;
    
    private void Start()
    {
        // Find player components
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
            attributeSystem = player.GetComponent<AttributeSystem>();
        }
        
        // Setup button events
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (attributesButton != null) attributesButton.onClick.AddListener(OpenAttributes);
        if (saveButton != null) saveButton.onClick.AddListener(SaveGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        
        // Setup attribute buttons
        if (strengthButton != null) strengthButton.onClick.AddListener(() => IncreaseAttribute(AttributeType.Strength));
        if (dexterityButton != null) dexterityButton.onClick.AddListener(() => IncreaseAttribute(AttributeType.Dexterity));
        if (intelligenceButton != null) intelligenceButton.onClick.AddListener(() => IncreaseAttribute(AttributeType.Intelligence));
        if (vitalityButton != null) vitalityButton.onClick.AddListener(() => IncreaseAttribute(AttributeType.Vitality));
        
        // Setup settings sliders
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        if (qualityDropdown != null) qualityDropdown.onValueChanged.AddListener(SetQuality);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        
        // Subscribe to input
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPauseToggle += TogglePause;
        }
        
        // Initialize
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        attributesPanel.SetActive(false);
        
        LoadSettingsUI();
    }
    
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        attributesPanel.SetActive(false);
        
        GameManager.Instance.ChangeGameState(GameState.Paused);
        
        UpdateAttributesUI();
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        attributesPanel.SetActive(false);
        
        GameManager.Instance.ChangeGameState(GameState.Gameplay);
    }
    
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        attributesPanel.SetActive(false);
        LoadSettingsUI();
    }
    
    public void OpenAttributes()
    {
        attributesPanel.SetActive(true);
        settingsPanel.SetActive(false);
        UpdateAttributesUI();
    }
    
    public void SaveGame()
    {
        GameManager.Instance.SaveGame();
        Debug.Log("Game saved!");
    }
    
    public void ReturnToMainMenu()
    {
        ResumeGame();
        GameManager.Instance.ChangeGameState(GameState.Login);
    }
    
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
    
    private void LoadSettingsUI()
    {
        if (SettingsManager.Instance != null)
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value = SettingsManager.Instance.masterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = SettingsManager.Instance.musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = SettingsManager.Instance.sfxVolume;
            if (qualityDropdown != null) qualityDropdown.value = SettingsManager.Instance.qualityLevel;
            if (fullscreenToggle != null) fullscreenToggle.isOn = SettingsManager.Instance.fullscreen;
        }
    }
    
    private void UpdateAttributesUI()
    {
        if (playerStats == null) return;
        
        if (strengthText != null) strengthText.text = $"Strength: {playerStats.strength}";
        if (dexterityText != null) dexterityText.text = $"Dexterity: {playerStats.dexterity}";
        if (intelligenceText != null) intelligenceText.text = $"Intelligence: {playerStats.intelligence}";
        if (vitalityText != null) vitalityText.text = $"Vitality: {playerStats.vitality}";
        if (availablePointsText != null) availablePointsText.text = $"Available Points: {playerStats.availableAttributePoints}";
        
        // Update button interactability
        bool hasPoints = playerStats.availableAttributePoints > 0;
        if (strengthButton != null) strengthButton.interactable = hasPoints;
        if (dexterityButton != null) dexterityButton.interactable = hasPoints;
        if (intelligenceButton != null) intelligenceButton.interactable = hasPoints;
        if (vitalityButton != null) vitalityButton.interactable = hasPoints;
    }
    
    private void IncreaseAttribute(AttributeType attribute)
    {
        if (attributeSystem != null)
        {
            attributeSystem.IncreaseAttribute(attribute);
            UpdateAttributesUI();
        }
    }
    
    private void SetMasterVolume(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.masterVolume = value;
            SettingsManager.Instance.SaveSettings();
        }
    }
    
    private void SetMusicVolume(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.musicVolume = value;
            SettingsManager.Instance.SaveSettings();
        }
    }
    
    private void SetSFXVolume(float value)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.sfxVolume = value;
            SettingsManager.Instance.SaveSettings();
        }
    }
    
    private void SetQuality(int qualityIndex)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.qualityLevel = qualityIndex;
            SettingsManager.Instance.SaveSettings();
        }
    }
    
    private void SetFullscreen(bool isFullscreen)
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.fullscreen = isFullscreen;
            SettingsManager.Instance.SaveSettings();
        }
    }
    
    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPauseToggle -= TogglePause;
        }
    }
}