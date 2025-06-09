// Assets/Scripts/Combat/CooldownManager.cs - VERSÃO MELHORADA
using UnityEngine;
using System.Collections.Generic;

public class CooldownManager : MonoBehaviour
{
    public static CooldownManager Instance { get; private set; }
    
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>();
    private Dictionary<string, float> totalDurations = new Dictionary<string, float>();
    
    // Events
    public System.Action<string, float, float> OnCooldownStarted; // ability name, current time, total time
    public System.Action<string> OnCooldownFinished;
    public System.Action<string, float> OnCooldownUpdated; // ability name, remaining time
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Update()
    {
        UpdateCooldowns();
    }
    
    public void StartCooldown(string abilityName, float duration)
    {
        if (string.IsNullOrEmpty(abilityName) || duration <= 0) return;
        
        cooldowns[abilityName] = duration;
        totalDurations[abilityName] = duration;
        OnCooldownStarted?.Invoke(abilityName, duration, duration);
    }
    
    public bool IsOnCooldown(string abilityName)
    {
        return !string.IsNullOrEmpty(abilityName) && 
               cooldowns.ContainsKey(abilityName) && 
               cooldowns[abilityName] > 0;
    }
    
    public float GetCooldownRemaining(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName) || !cooldowns.ContainsKey(abilityName))
            return 0f;
            
        return Mathf.Max(0f, cooldowns[abilityName]);
    }
    
    public float GetCooldownProgress(string abilityName, float totalDuration)
    {
        if (string.IsNullOrEmpty(abilityName) || !cooldowns.ContainsKey(abilityName))
            return 1f;
        
        if (totalDuration <= 0f)
        {
            // Try to get stored total duration
            if (totalDurations.ContainsKey(abilityName))
                totalDuration = totalDurations[abilityName];
            else
                return 1f;
        }
        
        return 1f - (cooldowns[abilityName] / totalDuration);
    }
    
    public float GetCooldownProgressNormalized(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName) || !cooldowns.ContainsKey(abilityName) || !totalDurations.ContainsKey(abilityName))
            return 1f;
        
        return 1f - (cooldowns[abilityName] / totalDurations[abilityName]);
    }
    
    private void UpdateCooldowns()
    {
        var keys = new List<string>(cooldowns.Keys);
        
        foreach (string abilityName in keys)
        {
            float previousTime = cooldowns[abilityName];
            cooldowns[abilityName] -= Time.deltaTime;
            
            // Update event
            OnCooldownUpdated?.Invoke(abilityName, cooldowns[abilityName]);
            
            if (cooldowns[abilityName] <= 0)
            {
                cooldowns.Remove(abilityName);
                totalDurations.Remove(abilityName);
                OnCooldownFinished?.Invoke(abilityName);
            }
        }
    }
    
    public void ClearCooldown(string abilityName)
    {
        if (string.IsNullOrEmpty(abilityName)) return;
        
        if (cooldowns.ContainsKey(abilityName))
        {
            cooldowns.Remove(abilityName);
            totalDurations.Remove(abilityName);
            OnCooldownFinished?.Invoke(abilityName);
        }
    }
    
    public void ClearAllCooldowns()
    {
        var keys = new List<string>(cooldowns.Keys);
        
        foreach (string abilityName in keys)
        {
            ClearCooldown(abilityName);
        }
    }
    
    public Dictionary<string, float> GetAllCooldowns()
    {
        return new Dictionary<string, float>(cooldowns);
    }
}
            