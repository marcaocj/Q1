// Assets/Scripts/UI/NotificationManager.cs - VERSÃO CORRIGIDA
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }
    
    [Header("Notification UI")]
    public GameObject notificationPrefab;
    public Transform notificationParent;
    public float notificationDuration = 3f;
    public int maxNotifications = 5;
    
    private Queue<GameObject> activeNotifications = new Queue<GameObject>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Start()
    {
        // Subscribe to notification events
        if (GameManager.Instance != null)
        {
            // Usando sobrecarga do método que aceita apenas string
            GameManager.Instance.OnNotificationRequested += ShowNotificationString;
        }
    }
    
    // Método que aceita apenas string (compatível com o evento)
    public void ShowNotificationString(string message)
    {
        ShowNotification(message, NotificationType.Info);
    }
    
    public void ShowNotification(string message, NotificationType type = NotificationType.Info)
    {
        if (string.IsNullOrEmpty(message)) return;
        
        // Remove excess notifications
        while (activeNotifications.Count >= maxNotifications)
        {
            RemoveOldestNotification();
        }
        
        // Create new notification
        GameObject notification = CreateNotification(message, type);
        activeNotifications.Enqueue(notification);
        
        // Auto-remove after duration
        StartCoroutine(RemoveNotificationAfterDelay(notification, notificationDuration));
    }
    
    private GameObject CreateNotification(string message, NotificationType type)
    {
        GameObject notification = Instantiate(notificationPrefab, notificationParent);
        
        // Setup notification
        Text messageText = notification.GetComponentInChildren<Text>();
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        // Set color based on type
        Image background = notification.GetComponent<Image>();
        if (background != null)
        {
            background.color = GetNotificationColor(type);
        }
        
        // Animate in
        StartCoroutine(AnimateNotificationIn(notification));
        
        return notification;
    }
    
    private void RemoveOldestNotification()
    {
        if (activeNotifications.Count > 0)
        {
            GameObject oldest = activeNotifications.Dequeue();
            if (oldest != null)
            {
                StartCoroutine(AnimateNotificationOut(oldest));
            }
        }
    }
    
    private IEnumerator RemoveNotificationAfterDelay(GameObject notification, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (notification != null)
        {
            StartCoroutine(AnimateNotificationOut(notification));
        }
    }
    
    private IEnumerator AnimateNotificationIn(GameObject notification)
    {
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = notification.AddComponent<CanvasGroup>();
        
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator AnimateNotificationOut(GameObject notification)
    {
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Destroy(notification);
            yield break;
        }
        
        float elapsed = 0f;
        float duration = 0.3f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }
        
        Destroy(notification);
    }
    
    private Color GetNotificationColor(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Success: return new Color(0.2f, 0.8f, 0.2f, 0.9f);
            case NotificationType.Warning: return new Color(0.9f, 0.7f, 0.1f, 0.9f);
            case NotificationType.Error: return new Color(0.9f, 0.2f, 0.2f, 0.9f);
            case NotificationType.Info:
            default: return new Color(0.2f, 0.6f, 0.9f, 0.9f);
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNotificationRequested -= ShowNotificationString;
        }
    }
}