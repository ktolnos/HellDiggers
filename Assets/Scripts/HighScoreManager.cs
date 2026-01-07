using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager I;
    public TextMeshProUGUI currentIncomeText;
    public TextMeshProUGUI previousScoreText;
    public TextMeshProUGUI previousHighScoreText;
    public GameObject newHighScoreIndicator;

    public TextMeshProUGUI copperText;
    public TextMeshProUGUI ironText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI emeraldText;
    public TextMeshProUGUI diamondText;
    
    public GameObject highScorePanel;
    
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
        previousHighScoreText.text = highScore.ToString();
        if (latestScore > highScore)
        {
            highScore = latestScore;
        }
        previousScoreText.text = previousScore.ToString();
    }
    
    public void OpenHighScorePanel()
    {
        highScorePanel.SetActive(true);
        GM.OnUIOpen(CloseHighScorePanel);
        StartCoroutine("HighScoreAnimation");
        newHighScoreIndicator.SetActive(false);
        copperText.transform.parent.gameObject.SetActive(false);
        ironText.transform.parent.gameObject.SetActive(false);
        goldText.transform.parent.gameObject.SetActive(false);
        emeraldText.transform.parent.gameObject.SetActive(false);
        diamondText.transform.parent.gameObject.SetActive(false);
    }
    
    public void CloseHighScorePanel()
    {
        highScorePanel.SetActive(false);
        GM.I.money = GM.I.GetTotalMoney();
        GM.I.resources = new GM.Resources();
        StopCoroutine("HighScoreAnimation");
    }

    private void Update()
    {
        if (highScorePanel.activeSelf && (Keyboard.current.spaceKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame))
        {
            GM.PopTopUI();
        }
    }

    private IEnumerator HighScoreAnimation()
    {
        var appearanceDelay = new WaitForSeconds(0.5f);
        var lastIncomeTotal = 0;
        var initialMoney = GM.I.money;
        currentIncomeText.text = "0";
        yield return appearanceDelay;
        lastIncomeTotal += GM.I.resources.copper * GM.I.copperPrice;
        copperText.text = GM.I.resources.copper + " x " + GM.I.copperPrice + " = " + (GM.I.resources.copper * GM.I.copperPrice);
        copperText.transform.parent.gameObject.SetActive(true);
        GM.I.resources.copper = 0;
        GM.I.money = initialMoney + lastIncomeTotal;
        currentIncomeText.text = lastIncomeTotal.ToString();

        if (GM.I.resources.iron > 0)
        {
            yield return appearanceDelay;
            lastIncomeTotal += GM.I.resources.iron * GM.I.ironPrice;
            ironText.text = GM.I.resources.iron + " x " + GM.I.ironPrice + " = " + (GM.I.resources.iron * GM.I.ironPrice);
            ironText.transform.parent.gameObject.SetActive(true);
            GM.I.resources.iron = 0;
            GM.I.money = initialMoney + lastIncomeTotal;
            currentIncomeText.text = lastIncomeTotal.ToString(); 
        }

        if (GM.I.resources.gold > 0)
        {
            yield return appearanceDelay;
            lastIncomeTotal += GM.I.resources.gold * GM.I.goldPrice;
            goldText.text = GM.I.resources.gold + " x " + GM.I.goldPrice + " = " +
                            (GM.I.resources.gold * GM.I.goldPrice);
            goldText.transform.parent.gameObject.SetActive(true);
            GM.I.resources.gold = 0;
            GM.I.money = initialMoney + lastIncomeTotal;
            currentIncomeText.text = lastIncomeTotal.ToString();
        }

        if (GM.I.resources.emerald > 0)
        {
            yield return appearanceDelay;
            lastIncomeTotal += GM.I.resources.emerald * GM.I.emeraldPrice;
            emeraldText.text = GM.I.resources.emerald + " x " + GM.I.emeraldPrice + " = " +
                               (GM.I.resources.emerald * GM.I.emeraldPrice);
            emeraldText.transform.parent.gameObject.SetActive(true);
            GM.I.resources.emerald = 0;
            GM.I.money = initialMoney + lastIncomeTotal;
            currentIncomeText.text = lastIncomeTotal.ToString();
        }

        if (GM.I.resources.diamond > 0)
        {
            yield return appearanceDelay;
            lastIncomeTotal += GM.I.resources.diamond * GM.I.diamondPrice;
            diamondText.text = GM.I.resources.diamond + " x " + GM.I.diamondPrice + " = " +
                               (GM.I.resources.diamond * GM.I.diamondPrice);
            diamondText.transform.parent.gameObject.SetActive(true);
            GM.I.resources.diamond = 0;
            GM.I.money = initialMoney + lastIncomeTotal;    
            currentIncomeText.text = lastIncomeTotal.ToString();
        }
        
        if (lastIncomeTotal >= highScore)
        {
            yield return appearanceDelay;
            newHighScoreIndicator.SetActive(true);
            (newHighScoreIndicator.transform as RectTransform).DORotate(new Vector3(0f, 0f, -30f), 0.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
    }
}
