// Assets/Scripts/Core/SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }
    
    [Header("Loading UI")]
    public GameObject loadingScreen;
    public UnityEngine.UI.Slider progressBar;
    public UnityEngine.UI.Text loadingText;
    
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
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            if (progressBar != null)
                progressBar.value = progress;
            
            if (loadingText != null)
                loadingText.text = $"Loading... {progress * 100:F0}%";
            
            if (operation.progress >= 0.9f)
            {
                if (loadingText != null)
                    loadingText.text = "Press any key to continue...";
                
                if (Input.anyKeyDown)
                    operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}