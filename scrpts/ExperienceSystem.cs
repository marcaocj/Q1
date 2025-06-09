// Assets/Scripts/Progression/ExperienceSystem.cs
using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    [Header("Experience Settings")]
    public AnimationCurve experienceCurve;
    public float baseExperience = 100f;
    public float experienceMultiplier = 1.5f;
    
    private PlayerStats playerStats;
    
    // Events
    public System.Action<int> OnExperienceGained;
    public System.Action<int> OnLevelUp;
    
    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        
        if (playerStats != null)
        {
            playerStats.OnExperienceGained += HandleExperienceGained;
            playerStats.OnLevelUp += HandleLevelUp;
        }
    }
    
    private void HandleExperienceGained(int experience)
    {
        OnExperienceGained?.Invoke(experience);
        ShowExperienceGainedEffect(experience);
    }
    
    private void HandleLevelUp(int newLevel)
    {
        OnLevelUp?.Invoke(newLevel);
        ShowLevelUpEffect(newLevel);
    }
    
    public int CalculateExperienceForLevel(int level)
    {
        if (experienceCurve != null && experienceCurve.length > 0)
        {
            return Mathf.RoundToInt(baseExperience * experienceCurve.Evaluate(level) * Mathf.Pow(experienceMultiplier, level - 1));
        }
        
        // Default calculation if no curve is set
        return Mathf.RoundToInt(baseExperience * Mathf.Pow(experienceMultiplier, level - 1));
    }
    
    public float GetExperienceProgress()
    {
        if (playerStats == null) return 0f;
        
        return (float)playerStats.experience / playerStats.experienceToNextLevel;
    }
    
    private void ShowExperienceGainedEffect(int experience)
    {
        // Show floating text or effect
        Debug.Log($"+{experience} XP");
    }
    
    private void ShowLevelUpEffect(int newLevel)
    {
        // Show level up effect
        Debug.Log($"LEVEL UP! Now level {newLevel}");
        
        // Could trigger particle effects, sound, screen flash, etc.
    }
    
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnExperienceGained -= HandleExperienceGained;
            playerStats.OnLevelUp -= HandleLevelUp;
        }
    }
}
