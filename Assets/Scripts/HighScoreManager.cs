using System;
using TMPro;
using UnityEngine;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager I;
    public TextMeshProUGUI highScoreText;
    
    public int highScore;
    public int latestScore;
    public int previousScore;
    
    private void Awake()
    {
        I = this;
    }

    public void UpdateHighScore(int score)
    {
        previousScore = latestScore;
        latestScore = score;
        if (latestScore > highScore)
        {
            highScore = latestScore;
        }

        highScoreText.text = "Your latest income:\n\n"+ latestScore + 
                             "\n\nYour previous income:\n\n" + previousScore + "\n\nYour best run:\n\n" + highScore;
    }
}
