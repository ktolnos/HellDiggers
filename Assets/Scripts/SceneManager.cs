using UnityEngine;

public class SceneManager: MonoBehaviour
{
    public void LoadMain()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    
    public void LoadMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    public void LoadCredits()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
        
}