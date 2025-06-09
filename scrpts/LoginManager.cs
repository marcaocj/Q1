// Assets/Scripts/Login/LoginManager.cs
using UnityEngine;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    public void AttemptLogin(string username, string password, System.Action<bool, string> callback)
    {
        StartCoroutine(LoginCoroutine(username, password, callback));
    }
    
    private IEnumerator LoginCoroutine(string username, string password, System.Action<bool, string> callback)
    {
        yield return StartCoroutine(NetworkStub.AuthenticateUser(username, password, (success, message) =>
        {
            callback?.Invoke(success, message);
            
            if (success)
            {
                // Delay transition to character creation
                StartCoroutine(TransitionToCharacterCreation());
            }
        }));
    }
    
    private IEnumerator TransitionToCharacterCreation()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.ChangeGameState(GameState.CharacterCreation);
    }
}