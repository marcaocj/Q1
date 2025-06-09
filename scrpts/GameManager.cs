// Assets/Scripts/Core/GameManager.cs - VERSÃO COMPLETA E CORRIGIDA
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public GameState currentState = GameState.Loading;
    public float autoSaveInterval = 300f;
    
    [Header("Player Data")]
    public PlayerData currentPlayer;
    
    [Header("Game Configuration")]
    public bool enableAutoSave = true;
    public bool enableDebugMode = false;
    
    // Events
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<string> OnNotificationRequested;
    
    private Coroutine autoSaveCoroutine;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeGame();
    }
    
    private void Start()
    {
        // Subscribe to events after all objects are initialized
        SubscribeToEvents();
    }
    
    private void Update()
    {
        UpdatePlayTime();
    }
    
    private void InitializeGame()
    {
        try
        {
            SaveSystem.Initialize();
            
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.LoadSettings();
            
            ChangeGameState(GameState.Login);
            
            DebugLog("Game initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize game: {e.Message}");
        }
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to scene loading
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Subscribe to game events for statistics tracking
        GameEvents.OnEnemyDeath += HandleEnemyKilled;
        GameEvents.OnItemPickup += HandleItemPickup;
        if (GameEvents.OnQuestCompleted != null)
            GameEvents.OnQuestCompleted += HandleQuestCompleted;
    }
    
    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from scene loading
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Unsubscribe from game events
        GameEvents.OnEnemyDeath -= HandleEnemyKilled;
        GameEvents.OnItemPickup -= HandleItemPickup;
        if (GameEvents.OnQuestCompleted != null)
            GameEvents.OnQuestCompleted -= HandleQuestCompleted;
    }
    
    #region State Management
    
    public void ChangeGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        OnGameStateChanged?.Invoke(newState);
        GameEvents.OnGameStateChanged?.Invoke(newState);
        HandleStateChange(previousState, newState);
        
        DebugLog($"Game state changed: {previousState} -> {newState}");
    }
    
    private void HandleStateChange(GameState from, GameState to)
    {
        Time.timeScale = (to == GameState.Paused) ? 0f : 1f;
        
        switch (to)
        {
            case GameState.Login:
                LoadScene("LoginScene");
                StopAutoSave();
                break;
                
            case GameState.CharacterCreation:
                LoadScene("CharacterCreationScene");
                StopAutoSave();
                break;
                
            case GameState.Gameplay:
                LoadScene("GameplayScene");
                if (enableAutoSave)
                    StartAutoSave();
                break;
                
            case GameState.GameOver:
                StopAutoSave();
                ShowNotification("Game Over! Progress saved.");
                break;
        }
    }
    
    #endregion
    
    #region Scene Management
    
    private void LoadScene(string sceneName)
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DebugLog($"Scene loaded: {scene.name}");
        
        if (scene.name == "GameplayScene" && currentPlayer != null)
        {
            StartCoroutine(SetupPlayerInGameplay());
        }
    }
    
    private IEnumerator SetupPlayerInGameplay()
    {
        // Wait for scene to fully load
        yield return new WaitForEndOfFrame();
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                Debug.Log($"Setting up player: {currentPlayer.characterName} ({currentPlayer.characterClass})");
                Debug.Log($"PlayerData stats - STR:{currentPlayer.strength} DEX:{currentPlayer.dexterity} INT:{currentPlayer.intelligence} VIT:{currentPlayer.vitality}");
                
                // Apply saved data to player - FORCE the stats
                stats.level = currentPlayer.level;
                stats.experience = currentPlayer.experience;
                stats.strength = currentPlayer.strength;
                stats.dexterity = currentPlayer.dexterity;
                stats.intelligence = currentPlayer.intelligence;
                stats.vitality = currentPlayer.vitality;
                stats.availableAttributePoints = currentPlayer.availableAttributePoints;
                stats.maxHealth = currentPlayer.maxHealth;
                stats.currentHealth = currentPlayer.currentHealth;
                stats.maxMana = currentPlayer.maxMana;
                stats.currentMana = currentPlayer.currentMana;
                
                // Force recalculation and UI update
                stats.CalculateDerivedStats();
                stats.ForceStatsUpdate();
                
                // Set position if saved
                if (currentPlayer.position != Vector3.zero)
                {
                    player.transform.position = currentPlayer.position;
                    player.transform.rotation = Quaternion.Euler(0, currentPlayer.rotationY, 0);
                }
                
                Debug.Log($"Player setup complete for {currentPlayer.characterName}");
                Debug.Log($"Final stats - STR:{stats.strength} DEX:{stats.dexterity} INT:{stats.intelligence} VIT:{stats.vitality}");
                Debug.Log($"Health: {stats.currentHealth}/{stats.maxHealth} Mana: {stats.currentMana}/{stats.maxMana}");
            }
            else
            {
                Debug.LogError("PlayerStats component not found on player!");
            }
        }
        else
        {
            Debug.LogError("Player GameObject not found in GameplayScene!");
        }
    }
    
    #endregion
    
    #region Game Flow
    
    public void StartNewGame(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogError("Cannot start new game with null player data");
            return;
        }
        
        currentPlayer = playerData;
        ShowNotification($"Welcome, {playerData.characterName}!");
        ChangeGameState(GameState.Gameplay);
    }
    
    public void LoadGame(string saveFileName)
    {
        try
        {
            currentPlayer = SaveSystem.LoadPlayerData(saveFileName);
            if (currentPlayer != null)
            {
                currentPlayer.UpdateLastPlayed();
                ShowNotification($"Welcome back, {currentPlayer.characterName}!");
                ChangeGameState(GameState.Gameplay);
            }
            else
            {
                ShowNotification("Failed to load save file.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading game: {e.Message}");
            ShowNotification("Error loading save file.");
        }
    }
    
    public void ReturnToMainMenu()
    {
        SaveGame();
        ChangeGameState(GameState.Login);
    }
    
    public void RestartGame()
    {
        SaveGame();
        currentPlayer = null;
        ChangeGameState(GameState.CharacterCreation);
    }
    
    public void QuitGame()
    {
        SaveGame();
        DebugLog("Quitting game");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Save System
    
    public void SaveGame()
    {
        if (currentPlayer == null)
        {
            DebugLog("No player data to save");
            return;
        }
        
        try
        {
            UpdatePlayerDataFromScene();
            SaveSystem.SavePlayerData(currentPlayer);
            ShowNotification("Game saved successfully!");
            DebugLog("Game saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
            ShowNotification("Failed to save game!");
        }
    }
    
    private void UpdatePlayerDataFromScene()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                currentPlayer.SyncWithPlayerStats(stats);
            }
            
            // Save position
            currentPlayer.position = player.transform.position;
            currentPlayer.rotationY = player.transform.eulerAngles.y;
            currentPlayer.currentScene = SceneManager.GetActiveScene().name;
        }
    }
    
    private void StartAutoSave()
    {
        StopAutoSave();
        if (autoSaveInterval > 0 && enableAutoSave)
        {
            autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
        }
    }
    
    private void StopAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
    }
    
    private IEnumerator AutoSaveRoutine()
    {
        while (currentState == GameState.Gameplay)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            
            if (currentPlayer != null)
            {
                SaveGame();
            }
        }
    }
    
    #endregion
    
    #region Statistics Tracking
    
    public void UpdatePlayTime()
    {
        if (currentPlayer != null && currentState == GameState.Gameplay)
        {
            currentPlayer.UpdatePlayTime(Time.deltaTime);
        }
    }
    
    public void IncrementEnemiesKilled()
    {
        if (currentPlayer != null)
        {
            currentPlayer.enemiesKilled++;
        }
    }
    
    public void IncrementItemsCollected()
    {
        if (currentPlayer != null)
        {
            currentPlayer.itemsCollected++;
        }
    }
    
    public void IncrementQuestsCompleted()
    {
        if (currentPlayer != null)
        {
            currentPlayer.questsCompleted++;
        }
    }
    
    private void HandleEnemyKilled(GameObject enemy)
    {
        IncrementEnemiesKilled();
        DebugLog($"Enemy killed. Total: {currentPlayer?.enemiesKilled}");
    }
    
    private void HandleItemPickup(Item item, int quantity)
    {
        if (currentPlayer != null)
        {
            currentPlayer.itemsCollected += quantity;
            DebugLog($"Items collected: +{quantity}. Total: {currentPlayer.itemsCollected}");
        }
    }
    
    private void HandleQuestCompleted(Quest quest)
    {
        IncrementQuestsCompleted();
        DebugLog($"Quest completed: {quest?.questName}. Total: {currentPlayer?.questsCompleted}");
    }
    
    #endregion
    
    #region Utility Methods
    
    public void ShowNotification(string message)
    {
        OnNotificationRequested?.Invoke(message);
        GameEvents.OnNotification?.Invoke(message);
        DebugLog($"Notification: {message}");
    }
    
    public bool HasActivePlayer()
    {
        return currentPlayer != null;
    }
    
    public string GetCurrentPlayerName()
    {
        return currentPlayer?.characterName ?? "No Player";
    }
    
    public int GetCurrentPlayerLevel()
    {
        return currentPlayer?.level ?? 1;
    }
    
    public void ForceSync()
    {
        if (currentPlayer != null)
        {
            UpdatePlayerDataFromScene();
        }
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugMode)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }
    
    #endregion
    
    #region Properties
    
    public GameState CurrentState => currentState;
    public bool IsInGameplay => currentState == GameState.Gameplay;
    public bool IsPaused => currentState == GameState.Paused;
    public bool HasSaveData => currentPlayer != null;
    
    #endregion
    
    #region Debug Methods
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugResetPlayer()
    {
        currentPlayer = null;
        ChangeGameState(GameState.Login);
        Debug.Log("Player data reset (Debug mode only)");
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugCreateTestPlayer()
    {
        PlayerData testData = new PlayerData
        {
            characterName = "TestPlayer",
            characterClass = "Warrior",
            level = 5,
            strength = 15,
            dexterity = 12,
            intelligence = 10,
            vitality = 13,
            availableAttributePoints = 10,
            maxHealth = 150,
            currentHealth = 150,
            maxMana = 60,
            currentMana = 60
        };
        
        currentPlayer = testData;
        Debug.Log("Test player created (Debug mode only)");
    }
    
    #endregion
    
    #region Unity Events
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentPlayer != null)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentPlayer != null)
        {
            SaveGame();
        }
    }
    
    private void OnDestroy()
    {
        StopAutoSave();
        UnsubscribeFromEvents();
    }
    
    #endregion
}