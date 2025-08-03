using UnityEngine;

public class SceneManager: MonoBehaviour
{
    public void LoadMain()
    {
        Debug.Log("Loading main scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    
    public void LoadMenu()
    {
        Debug.Log("Loading main menu...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    public void LoadCredits()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
}