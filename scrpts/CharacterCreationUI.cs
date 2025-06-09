// Assets/Scripts/CharacterCreation/CharacterCreationUI.cs - VERSÃO SIMPLIFICADA
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreationUI : MonoBehaviour
{
    [Header("UI Elements")]
    public InputField characterNameField;
    public Dropdown classDropdown;
    public Button createButton;
    public Button backButton;
    public Text statusText;
    public GameObject loadingPanel;
    
    [Header("Character Preview")]
    public Transform characterPreviewPoint;
    public GameObject[] characterPrefabs;
    
    [Header("Class Info Display")]
    public Text classNameText;
    public Text classDescriptionText;
    public Text classStatsText;
    
    private CharacterCreator characterCreator;
    private GameObject currentPreview;
    
    private void Start()
    {
        characterCreator = GetComponent<CharacterCreator>();
        
        if (characterCreator == null)
        {
            Debug.LogError("CharacterCreator component not found!");
            return;
        }
        
        // Setup UI events
        createButton.onClick.AddListener(OnCreateButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
        classDropdown.onValueChanged.AddListener(OnClassChanged);
        
        // Initialize
        statusText.text = "";
        loadingPanel.SetActive(false);
        
        SetupClassDropdown();
        UpdateCharacterPreview();
        UpdateClassInfo();
    }
    
    private void SetupClassDropdown()
    {
        classDropdown.ClearOptions();
        
        string[] classNames = characterCreator.GetClassNames();
        classDropdown.AddOptions(new System.Collections.Generic.List<string>(classNames));
        
        if (characterCreator.HasValidClasses())
        {
            classDropdown.value = 0;
            classDropdown.interactable = true;
            createButton.interactable = true;
        }
        else
        {
            classDropdown.interactable = false;
            createButton.interactable = false;
            statusText.text = "No character classes available!";
            statusText.color = Color.red;
        }
    }
    
    private void OnCreateButtonClicked()
    {
        string characterName = characterNameField.text;
        int selectedClass = classDropdown.value;
        
        if (ValidateInput(characterName))
        {
            SetUIState(false);
            loadingPanel.SetActive(true);
            statusText.text = "Creating character...";
            statusText.color = Color.white;
            
            characterCreator.CreateCharacter(characterName, selectedClass, OnCharacterCreated);
        }
    }
    
    private bool ValidateInput(string characterName)
    {
        if (string.IsNullOrEmpty(characterName))
        {
            ShowError("Please enter a character name.");
            return false;
        }
        
        if (characterName.Length < 2 || characterName.Length > 16)
        {
            ShowError("Character name must be 2-16 characters long.");
            return false;
        }
        
        if (!characterCreator.HasValidClasses())
        {
            ShowError("No valid character classes available.");
            return false;
        }
        
        // Check for invalid characters
        string trimmedName = characterName.Trim();
        if (trimmedName.Length != characterName.Length)
        {
            ShowError("Character name cannot start or end with spaces.");
            return false;
        }
        
        return true;
    }
    
    private void ShowError(string message)
    {
        statusText.text = message;
        statusText.color = Color.red;
    }
    
    private void OnCharacterCreated(bool success, string message, PlayerData playerData)
    {
        SetUIState(true);
        loadingPanel.SetActive(false);
        statusText.text = message;
        
        if (success)
        {
            statusText.color = Color.green;
            
            // Pass PlayerData directly to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentPlayer = playerData;
                
                // Add delay before transitioning
                Invoke(nameof(StartGame), 1.5f);
            }
            else
            {
                ShowError("Error: Game Manager not found!");
            }
        }
        else
        {
            statusText.color = Color.red;
        }
    }
    
    private void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
    
    private void OnClassChanged(int classIndex)
    {
        UpdateCharacterPreview();
        UpdateClassInfo();
    }
    
    private void UpdateCharacterPreview()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        
        int classIndex = classDropdown.value;
        if (characterPrefabs != null && classIndex >= 0 && classIndex < characterPrefabs.Length && characterPrefabs[classIndex] != null)
        {
            if (characterPreviewPoint != null)
            {
                currentPreview = Instantiate(characterPrefabs[classIndex], characterPreviewPoint);
                currentPreview.transform.localPosition = Vector3.zero;
                currentPreview.transform.localRotation = Quaternion.identity;
            }
        }
    }
    
    private void UpdateClassInfo()
    {
        int classIndex = classDropdown.value;
        ClassTemplate selectedClass = characterCreator.GetClassTemplate(classIndex);
        
        if (selectedClass != null)
        {
            if (classNameText != null)
                classNameText.text = selectedClass.className;
            
            if (classDescriptionText != null)
                classDescriptionText.text = selectedClass.description;
            
            if (classStatsText != null)
            {
                classStatsText.text = $"<b>Starting Stats:</b>\n" +
                                    $"Strength: {selectedClass.baseStrength}\n" +
                                    $"Dexterity: {selectedClass.baseDexterity}\n" +
                                    $"Intelligence: {selectedClass.baseIntelligence}\n" +
                                    $"Vitality: {selectedClass.baseVitality}\n\n" +
                                    $"<b>Resources:</b>\n" +
                                    $"Health: {selectedClass.baseHealth}\n" +
                                    $"Mana: {selectedClass.baseMana}";
            }
        }
        else
        {
            if (classNameText != null)
                classNameText.text = "Unknown Class";
            
            if (classDescriptionText != null)
                classDescriptionText.text = "No description available.";
            
            if (classStatsText != null)
                classStatsText.text = "No stats available.";
        }
    }
    
    private void SetUIState(bool interactable)
    {
        characterNameField.interactable = interactable;
        classDropdown.interactable = interactable && characterCreator.HasValidClasses();
        createButton.interactable = interactable && characterCreator.HasValidClasses();
    }
    
    private void OnBackButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeGameState(GameState.Login);
        }
    }
    
    private void OnDestroy()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
    }
}