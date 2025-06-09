// Assets/Scripts/Combat/DamageNumberManager.cs
using UnityEngine;

public class DamageNumberManager : MonoBehaviour
{
    public static DamageNumberManager Instance { get; private set; }
    
    [Header("Damage Number Settings")]
    public GameObject damageNumberPrefab;
    public float floatHeight = 2f;
    public float floatDuration = 1f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    public void ShowDamage(Vector3 position, int damage, bool isCritical)
    {
        if (damageNumberPrefab == null) return;
        
        GameObject damageNumber = Instantiate(damageNumberPrefab, position + Vector3.up, Quaternion.identity);
        
        // Configure the damage number
        DamageNumber damageScript = damageNumber.GetComponent<DamageNumber>();
        if (damageScript != null)
        {
            damageScript.Initialize(damage, isCritical, floatHeight, floatDuration);
        }
    }
}
