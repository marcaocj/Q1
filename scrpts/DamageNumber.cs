// Assets/Scripts/Combat/DamageNumber.cs
using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
    private Text damageText;
    private float floatSpeed;
    private float fadeSpeed;
    private float timer;
    
    public void Initialize(int damage, bool isCritical, float height, float duration)
    {
        damageText = GetComponentInChildren<Text>();
        
        if (damageText != null)
        {
            damageText.text = damage.ToString();
            
            if (isCritical)
            {
                damageText.color = Color.yellow;
                damageText.fontSize = Mathf.RoundToInt(damageText.fontSize * 1.5f);
            }
            else
            {
                damageText.color = Color.white;
            }
        }
        
        floatSpeed = height / duration;
        fadeSpeed = 1f / duration;
        timer = duration;
        
        // Face camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }
    
    private void Update()
    {
        // Move upward
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        
        // Fade out
        timer -= Time.deltaTime;
        float alpha = timer * fadeSpeed;
        
        if (damageText != null)
        {
            Color color = damageText.color;
            color.a = alpha;
            damageText.color = color;
        }
        
        // Destroy when done
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}