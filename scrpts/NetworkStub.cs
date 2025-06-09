// Assets/Scripts/Core/NetworkStub.cs
using UnityEngine;
using System.Collections;

public static class NetworkStub
{
    public static IEnumerator AuthenticateUser(string username, string password, System.Action<bool, string> callback)
    {
        // Simulate network delay
        yield return new WaitForSeconds(1f);
        
        // Simple validation (in a real game, this would be server-side)
        bool isValid = !string.IsNullOrEmpty(username) && username.Length >= 3 && 
                      !string.IsNullOrEmpty(password) && password.Length >= 6;
        
        string message = isValid ? "Login successful!" : "Invalid username or password.";
        
        callback?.Invoke(isValid, message);
    }
    
    public static IEnumerator ValidateCharacterName(string characterName, System.Action<bool, string> callback)
    {
        yield return new WaitForSeconds(0.5f);
        
        bool isValid = !string.IsNullOrEmpty(characterName) && 
                      characterName.Length >= 2 && 
                      characterName.Length <= 16;
        
        string message = isValid ? "Character name is available!" : "Invalid character name.";
        
        callback?.Invoke(isValid, message);
    }
}