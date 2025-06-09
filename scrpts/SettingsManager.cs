// Assets/Scripts/Core/SettingsManager.cs
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    
    [Header("Audio Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    [Header("Graphics Settings")]
    public int qualityLevel = 3;
    public bool fullscreen = true;
    public int targetFrameRate = 60;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        
        qualityLevel = PlayerPrefs.GetInt("QualityLevel", 3);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        targetFrameRate = PlayerPrefs.GetInt("TargetFrameRate", 60);
        
        ApplySettings();
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        
        PlayerPrefs.SetInt("QualityLevel", qualityLevel);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("TargetFrameRate", targetFrameRate);
        
        PlayerPrefs.Save();
        ApplySettings();
    }
    
    private void ApplySettings()
    {
        AudioListener.volume = masterVolume;
        QualitySettings.SetQualityLevel(qualityLevel);
        Screen.fullScreen = fullscreen;
        Application.targetFrameRate = targetFrameRate;
    }
}