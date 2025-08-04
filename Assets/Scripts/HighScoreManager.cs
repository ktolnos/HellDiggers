using TMPro;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance;
    private TextMeshProUGUI highscoreText;
    
    private int highScore;
    private int latestScore;
    private int previousScore;
    
    private void Awake()
    {
        highscoreText = GetComponent<TextMeshProUGUI>();
        Instance = this;
    }
    
    public void UpdateHighScore(int score)
    {
        previousScore = latestScore;
        latestScore = score;
        if (latestScore > highScore)
        {
            highScore = latestScore;
        }

        highscoreText.text = "Your latest income:\n\n"+ latestScore + 
                             "\n\nYour previous income:\n\n" + previousScore + "\n\nYour best run:\n\n" + highScore;
    }
}
